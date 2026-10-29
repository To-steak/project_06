using System.Collections.Generic;
using Project06.Components;
using Project06.Entities;
using Project06.Records;
using Project06.Requests;
using UnityEngine;

namespace Project06.Systems
{
    public static class EnemyAttackSystem
    {
        /// <summary>
        /// 살아 있는 적마다 공격별 쿨다운을 한 틱 줄이고, 추격 중인 적은 쿨다운이 끝났고 사거리 안에 플레이어가 있는 공격 중 목록 앞쪽 공격을 시작한다.
        /// 공격 중인 적은 제자리에 서서 선딜레이 → 타격 구간 → 후딜레이를 거친 뒤 추격으로 돌아간다.
        /// 근접은 타격 구간 동안 매 틱 판정 모양(바라보는 방향 기준 원·캡슐)과 플레이어 피격 원이 겹치는지 검사해 한 번만 데미지를 준다. 원거리는 타격 구간에 들어오는 틱에 그 순간 플레이어 쪽으로 투사체 생성을 요청한다.
        /// EnemyBehaviorSystem 다음, EnemySeparationSystem 앞에 호출한다.
        /// </summary>
        /// <param name="enemies">공격할 적 저장소.</param>
        /// <param name="players">공격받을 플레이어 저장소.</param>
        /// <param name="enemyProjectileSpawnRequests">적 투사체 생성 요청 큐.</param>
        /// <param name="enemyAttackStartedRecords">이번 틱에 공격을 시작한 기록 목록.</param>
        public static void Tick(
            EnemyStorage enemies, PlayerStorage players,
            Queue<EnemyProjectileSpawnRequest> enemyProjectileSpawnRequests, List<EnemyAttackStartedRecord> enemyAttackStartedRecords)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                if (!enemy.isAlive || enemy.state == EnemyState.Dead)
                {
                    continue;
                }

                int[] cooldowns = enemy.attackCooldownRemainingTicks;
                for (int attackIndex = 0; attackIndex < cooldowns.Length; attackIndex++)
                {
                    if (cooldowns[attackIndex] > 0)
                    {
                        cooldowns[attackIndex]--;
                    }
                }

                if (enemy.state == EnemyState.Chase)
                {
                    TryStartAttack(ref enemy, index, players, enemyAttackStartedRecords);
                }
                else if (enemy.state == EnemyState.Attack)
                {
                    TickAttack(ref enemy, players, enemyProjectileSpawnRequests);
                }
            }
        }

        /// <summary>
        /// 근접 판정 모양을 XZ 평면의 선분과 반지름으로 구한다. 원은 시작점과 끝점이 같다. 바라보는 방향 기준이며 높이(Y)는 적 위치를 따른다.
        /// </summary>
        /// <param name="enemy">공격하는 적.</param>
        /// <param name="meleeHit">판정 모양.</param>
        /// <param name="start">선분 시작점.</param>
        /// <param name="end">선분 끝점.</param>
        /// <returns>판정 반지름(m).</returns>
        public static float GetMeleeHitSegment(in EnemyEntity enemy, in EnemyMeleeHitComponent meleeHit, out Vector3 start, out Vector3 end)
        {
            start = enemy.currentPosition + enemy.facingDirection * meleeHit.offset;
            end = meleeHit.shape == EnemyMeleeHitShape.Capsule
                ? start + enemy.facingDirection * meleeHit.length
                : start;
            return meleeHit.radius;
        }

        // 가장 가까운 플레이어가 쿨다운이 끝난 공격의 사거리 안에 있으면 그중 목록 앞쪽 공격을 시작하고, 그 공격의 쿨다운을 건다.
        private static void TryStartAttack(ref EnemyEntity enemy, int index, PlayerStorage players, List<EnemyAttackStartedRecord> enemyAttackStartedRecords)
        {
            EnemyAttackComponent[] attacks = enemy.enemy.attacks;
            if (attacks == null || attacks.Length == 0)
            {
                return;
            }

            if (!EnemyBehaviorSystem.TryFindNearestPlayer(players, enemy.currentPosition, out Vector3 targetPosition, out float targetSqrDistance))
            {
                return;
            }

            int attackCount = Mathf.Min(attacks.Length, enemy.attackCooldownRemainingTicks.Length);
            for (int attackIndex = 0; attackIndex < attackCount; attackIndex++)
            {
                float range = attacks[attackIndex].range;
                if (enemy.attackCooldownRemainingTicks[attackIndex] > 0 || range <= 0f || targetSqrDistance > range * range)
                {
                    continue;
                }

                enemy.state = EnemyState.Attack;
                enemy.attackIndex = attackIndex;
                enemy.attackPhase = EnemyAttackPhase.Windup;
                enemy.attackRemainingTicks = attacks[attackIndex].windupTicks;
                enemy.hasAttackDamaged = false;
                enemy.attackCooldownRemainingTicks[attackIndex] = attacks[attackIndex].cooldownTicks;
                enemy.velocity = Vector3.zero;
                Face(ref enemy, targetPosition - enemy.currentPosition);
                enemyAttackStartedRecords.Add(new EnemyAttackStartedRecord
                {
                    enemy = new EntityIdentifier { index = index, generation = enemy.generation },
                    attackIndex = attackIndex,
                });
                return;
            }
        }

        // 제자리에 둔 채 공격 단계를 진행한다. 한 틱 안에서 남은 시간이 0인 단계는 바로 다음 단계로 넘어간다.
        // 선딜레이가 끝나면 타격 구간에 들어가며 원거리는 이때 투사체를 쏜다. 근접은 타격 구간 동안 매 틱 판정한다. 후딜레이가 끝나면 추격으로 돌아간다.
        private static void TickAttack(ref EnemyEntity enemy, PlayerStorage players, Queue<EnemyProjectileSpawnRequest> enemyProjectileSpawnRequests)
        {
            enemy.previousPosition = enemy.currentPosition;
            EnemyAttackComponent attack = enemy.enemy.attacks[enemy.attackIndex];
            bool isMelee = attack.kind == EnemyAttackKind.Melee;

            if (enemy.attackPhase == EnemyAttackPhase.Windup)
            {
                enemy.attackRemainingTicks--;
                if (enemy.attackRemainingTicks > 0)
                {
                    return;
                }

                enemy.attackPhase = EnemyAttackPhase.Active;
                enemy.attackRemainingTicks = isMelee ? Mathf.Max(1, attack.meleeHit.activeTicks) : 0;
                if (!isMelee)
                {
                    FireRanged(ref enemy, in attack, players, enemyProjectileSpawnRequests);
                }
            }

            if (enemy.attackPhase == EnemyAttackPhase.Active)
            {
                if (isMelee && !enemy.hasAttackDamaged)
                {
                    enemy.hasAttackDamaged = TryHitMelee(in enemy, in attack, players);
                }

                enemy.attackRemainingTicks--;
                if (enemy.attackRemainingTicks > 0)
                {
                    return;
                }

                enemy.attackPhase = EnemyAttackPhase.Recovery;
                enemy.attackRemainingTicks = attack.recoveryTicks;
                if (enemy.attackRemainingTicks > 0)
                {
                    return;
                }
            }

            if (enemy.attackPhase == EnemyAttackPhase.Recovery)
            {
                enemy.attackRemainingTicks--;
                if (enemy.attackRemainingTicks > 0)
                {
                    return;
                }
            }

            enemy.state = EnemyState.Chase;
            // 공격하는 동안 플레이어가 움직였을 수 있으므로 다음 틱에 경로를 바로 다시 계산한다.
            enemy.pathUpdateRemainingTicks = 0;
        }

        // 판정 모양과 피격 원이 겹치는, 살아 있고 사망 중이 아닌 첫 플레이어에게 데미지를 준다. 맞혔으면 true를 돌려준다.
        private static bool TryHitMelee(in EnemyEntity enemy, in EnemyAttackComponent attack, PlayerStorage players)
        {
            float hitRadius = GetMeleeHitSegment(in enemy, in attack.meleeHit, out Vector3 start, out Vector3 end);
            PlayerEntity[] entities = players.entities;
            for (int playerIndex = 0; playerIndex < entities.Length; playerIndex++)
            {
                ref PlayerEntity player = ref entities[playerIndex];
                if (!player.isAlive || player.isDead)
                {
                    continue;
                }

                float reach = hitRadius + player.player.hitRadius;
                if (GetSqrDistanceToSegment(player.currentPosition, start, end) > reach * reach)
                {
                    continue;
                }

                player.health -= attack.damage;
                return true;
            }

            return false;
        }

        // XZ 평면에서 point와 start~end 선분 사이의 최단 거리의 제곱을 구한다.
        private static float GetSqrDistanceToSegment(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector2 segment = new Vector2(end.x - start.x, end.z - start.z);
            Vector2 toPoint = new Vector2(point.x - start.x, point.z - start.z);
            float segmentSqrLength = segment.sqrMagnitude;
            float ratio = segmentSqrLength > 0f ? Mathf.Clamp01(Vector2.Dot(toPoint, segment) / segmentSqrLength) : 0f;
            return (toPoint - segment * ratio).sqrMagnitude;
        }

        // 가장 가까운 플레이어 쪽(XZ 평면)으로 투사체 생성을 요청한다. 플레이어가 없으면 바라보는 방향으로 쏜다.
        private static void FireRanged(ref EnemyEntity enemy, in EnemyAttackComponent attack, PlayerStorage players, Queue<EnemyProjectileSpawnRequest> enemyProjectileSpawnRequests)
        {
            if (EnemyBehaviorSystem.TryFindNearestPlayer(players, enemy.currentPosition, out Vector3 targetPosition, out _))
            {
                Face(ref enemy, targetPosition - enemy.currentPosition);
            }

            enemyProjectileSpawnRequests.Enqueue(new EnemyProjectileSpawnRequest
            {
                enemyTypeIndex = enemy.enemyTypeIndex,
                position = enemy.currentPosition + Vector3.up * attack.projectile.height,
                velocity = enemy.facingDirection * attack.projectile.speed,
                range = attack.projectile.distance,
                damage = attack.damage,
            });
        }

        // XZ 평면에서 direction 쪽을 바라보게 한다. 길이가 0이면 방향을 유지한다.
        private static void Face(ref EnemyEntity enemy, Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0f)
            {
                enemy.facingDirection = direction.normalized;
            }
        }
    }
}
