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
        /// 공격 중인 적은 제자리에 서서 선딜레이를 한 틱 줄이고, 끝나면 근접은 사거리 안의 플레이어에게 데미지를 주고 원거리는 그 순간 플레이어 쪽으로 투사체 생성을 요청한다. 그 뒤 후딜레이 동안 제자리에 있다가 추격으로 돌아간다.
        /// EnemyBehaviorSystem 다음, ProjectileHitSystem 앞에 호출한다.
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
                enemy.attackRemainingTicks = attacks[attackIndex].windupTicks;
                enemy.hasAttackHit = false;
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

        // 제자리에 둔 채 남은 틱을 한 틱 줄인다. 선딜레이가 끝나면 타격하고 후딜레이를 시작하며, 후딜레이가 끝나면 추격으로 돌아간다. 후딜레이가 0이면 타격한 틱에 바로 돌아간다.
        private static void TickAttack(ref EnemyEntity enemy, PlayerStorage players, Queue<EnemyProjectileSpawnRequest> enemyProjectileSpawnRequests)
        {
            enemy.previousPosition = enemy.currentPosition;
            enemy.attackRemainingTicks--;
            if (enemy.attackRemainingTicks > 0)
            {
                return;
            }

            if (!enemy.hasAttackHit)
            {
                EnemyAttackComponent attack = enemy.enemy.attacks[enemy.attackIndex];
                if (attack.kind == EnemyAttackKind.Melee)
                {
                    HitMelee(in enemy, in attack, players);
                }
                else
                {
                    FireRanged(ref enemy, in attack, players, enemyProjectileSpawnRequests);
                }

                enemy.hasAttackHit = true;
                enemy.attackRemainingTicks = attack.recoveryTicks;
                if (enemy.attackRemainingTicks > 0)
                {
                    return;
                }
            }

            enemy.state = EnemyState.Chase;
            // 공격하는 동안 플레이어가 움직였을 수 있으므로 다음 틱에 경로를 바로 다시 계산한다.
            enemy.pathUpdateRemainingTicks = 0;
        }

        // 가장 가까운 플레이어가 사거리 안에 있으면 데미지를 준다. 벗어났으면 빗나간다.
        private static void HitMelee(in EnemyEntity enemy, in EnemyAttackComponent attack, PlayerStorage players)
        {
            if (!EnemyBehaviorSystem.TryFindNearestPlayer(players, enemy.currentPosition, out int targetIndex, out _, out float targetSqrDistance))
            {
                return;
            }

            if (targetSqrDistance > attack.range * attack.range)
            {
                return;
            }

            ref PlayerEntity player = ref players.entities[targetIndex];
            player.health -= attack.damage;
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
