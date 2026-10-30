using System.Collections.Generic;
using Project06.Components;
using Project06.Entities;
using Project06.Records;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerMeleeSystem
    {
        /// <summary>
        /// 근접 무기를 든 플레이어의 근접 공격 쿨다운을 한 틱 줄이고, 근접 공격 중이 아니면서 발사 입력을 누르고 있고 쿨다운이 끝났으면 근접 공격을 시작한다.
        /// 공격 중인 플레이어는 제자리에 서서 선딜레이 → 타격 구간 → 후딜레이를 거친다. 바라보는 방향은 CommandSystem이 고정한다.
        /// 타격 구간 동안 매 틱 바라보는 방향 기준 판정 상자(XZ 평면)와 적·쉘터의 피격 원이 겹치는지 검사해, 겹친 대상 중 피격 원 경계가 플레이어에게 가장 가까운 하나만 한 번 때린다.
        /// 적은 데미지를 받고(경직 확률은 투사체와 같다), 쉘터는 플레이어의 구조물이므로 체력을 회복한다.
        /// CommandSystem 다음, PlayerMoveSystem 앞에 호출한다.
        /// </summary>
        /// <param name="players">근접 공격할 플레이어 저장소.</param>
        /// <param name="enemyStorage">맞을 수 있는 적 저장소.</param>
        /// <param name="shelterStorage">수리될 수 있는 쉘터 저장소.</param>
        /// <param name="random">경직 여부를 정할 난수 생성기.</param>
        /// <param name="playerMeleeStartedRecords">이번 틱에 근접 공격을 시작한 기록 목록.</param>
        /// <param name="enemyHitStunnedRecords">이번 틱에 경직에 걸린 기록 목록.</param>
        public static void Tick(
            PlayerStorage players, EnemyStorage enemyStorage, ShelterStorage shelterStorage, System.Random random,
            List<PlayerMeleeStartedRecord> playerMeleeStartedRecords, List<EnemyHitStunnedRecord> enemyHitStunnedRecords)
        {
            PlayerEntity[] entities = players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref PlayerEntity player = ref entities[index];
                if (!player.isAlive || player.isDead || player.weaponIndex < 0 || player.weapon.kind != WeaponKind.Melee)
                {
                    continue;
                }

                Attack(ref player, enemyStorage, shelterStorage, random, playerMeleeStartedRecords, enemyHitStunnedRecords);
            }
        }

        /// <summary>
        /// 판정 상자를 XZ 평면의 중심, 앞쪽 방향, 절반 길이와 절반 폭으로 구한다. 바라보는 방향 기준이며 높이(Y)는 플레이어 위치를 따른다.
        /// </summary>
        /// <param name="player">공격하는 플레이어.</param>
        /// <param name="center">판정 상자의 중심.</param>
        /// <param name="forward">판정 상자의 길이 방향. 길이는 1이다.</param>
        /// <param name="halfLength">판정 상자 길이의 절반(m).</param>
        /// <param name="halfWidth">판정 상자 폭의 절반(m).</param>
        public static void GetHitBox(in PlayerEntity player, out Vector3 center, out Vector3 forward, out float halfLength, out float halfWidth)
        {
            MeleeWeaponComponent melee = player.weapon.melee;
            forward = player.facingDirection;
            halfLength = melee.hitLength * 0.5f;
            halfWidth = melee.hitWidth * 0.5f;
            center = player.currentPosition + forward * (melee.hitOffset + halfLength);
        }

        private static void Attack(
            ref PlayerEntity player, EnemyStorage enemyStorage, ShelterStorage shelterStorage, System.Random random,
            List<PlayerMeleeStartedRecord> playerMeleeStartedRecords, List<EnemyHitStunnedRecord> enemyHitStunnedRecords)
        {
            if (player.meleeCooldownTicks > 0)
            {
                player.meleeCooldownTicks--;
            }

            if (player.meleePhase == PlayerMeleePhase.None)
            {
                if (player.isFiring && player.meleeCooldownTicks <= 0)
                {
                    StartAttack(ref player);
                    playerMeleeStartedRecords.Add(new PlayerMeleeStartedRecord { weaponIndex = player.weaponIndex });
                }

                return;
            }

            Stand(ref player);
            TickAttack(ref player, enemyStorage, shelterStorage, random, enemyHitStunnedRecords);
        }

        private static void StartAttack(ref PlayerEntity player)
        {
            MeleeWeaponComponent melee = player.weapon.melee;
            player.meleePhase = PlayerMeleePhase.Windup;
            player.meleeRemainingTicks = melee.windupTicks;
            player.hasMeleeHit = false;
            player.meleeCooldownTicks = melee.cooldownTicks;
            Stand(ref player);
        }

        // 공격하는 동안 제자리에 세운다. CommandSystem이 매 틱 이동 입력을 다시 넣으므로 매 틱 지운다.
        private static void Stand(ref PlayerEntity player)
        {
            player.moveDirection = Vector3.zero;
            player.velocity = Vector3.zero;
            player.isRunning = false;
        }

        // 공격 단계를 진행한다. 한 틱 안에서 남은 시간이 0인 단계는 바로 다음 단계로 넘어간다.
        // 선딜레이가 끝나면 타격 구간에 들어가 매 틱 판정한다. 후딜레이가 끝나면 공격을 끝낸다.
        private static void TickAttack(ref PlayerEntity player, EnemyStorage enemyStorage, ShelterStorage shelterStorage, System.Random random, List<EnemyHitStunnedRecord> enemyHitStunnedRecords)
        {
            MeleeWeaponComponent melee = player.weapon.melee;

            if (player.meleePhase == PlayerMeleePhase.Windup)
            {
                player.meleeRemainingTicks--;
                if (player.meleeRemainingTicks > 0)
                {
                    return;
                }

                player.meleePhase = PlayerMeleePhase.Active;
                player.meleeRemainingTicks = Mathf.Max(1, melee.activeTicks);
            }

            if (player.meleePhase == PlayerMeleePhase.Active)
            {
                if (!player.hasMeleeHit)
                {
                    player.hasMeleeHit = TryHit(in player, in melee, enemyStorage, shelterStorage, random, enemyHitStunnedRecords);
                }

                player.meleeRemainingTicks--;
                if (player.meleeRemainingTicks > 0)
                {
                    return;
                }

                player.meleePhase = PlayerMeleePhase.Recovery;
                player.meleeRemainingTicks = melee.recoveryTicks;
                if (player.meleeRemainingTicks > 0)
                {
                    return;
                }
            }

            if (player.meleePhase == PlayerMeleePhase.Recovery)
            {
                player.meleeRemainingTicks--;
                if (player.meleeRemainingTicks > 0)
                {
                    return;
                }
            }

            player.meleePhase = PlayerMeleePhase.None;
            player.meleeRemainingTicks = 0;
        }

        // 판정 상자와 피격 원이 겹치는 살아 있는 적과 파괴되지 않은 쉘터 중 피격 원 경계가 플레이어에게 가장 가까운 하나를 때린다. 맞혔으면 true를 돌려준다.
        private static bool TryHit(
            in PlayerEntity player, in MeleeWeaponComponent melee, EnemyStorage enemyStorage, ShelterStorage shelterStorage,
            System.Random random, List<EnemyHitStunnedRecord> enemyHitStunnedRecords)
        {
            GetHitBox(in player, out Vector3 center, out Vector3 forward, out float halfLength, out float halfWidth);
            int hitEnemyIndex = -1;
            int hitShelterIndex = -1;
            float nearestDistance = float.MaxValue;

            EnemyEntity[] enemies = enemyStorage.entities;
            for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++)
            {
                ref EnemyEntity enemy = ref enemies[enemyIndex];
                if (!enemy.isAlive || enemy.state == EnemyState.Dead)
                {
                    continue;
                }

                if (!OverlapsHitBox(center, forward, halfLength, halfWidth, enemy.currentPosition, enemy.enemy.hitRadius))
                {
                    continue;
                }

                float distance = GetDistanceToEdge(player.currentPosition, enemy.currentPosition, enemy.enemy.hitRadius);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    hitEnemyIndex = enemyIndex;
                }
            }

            ShelterEntity[] shelters = shelterStorage.entities;
            for (int shelterIndex = 0; shelterIndex < shelters.Length; shelterIndex++)
            {
                ref ShelterEntity shelter = ref shelters[shelterIndex];
                if (!shelter.isAlive || shelter.isDestroyed)
                {
                    continue;
                }

                if (!OverlapsHitBox(center, forward, halfLength, halfWidth, shelter.position, shelter.shelter.hitRadius))
                {
                    continue;
                }

                float distance = GetDistanceToEdge(player.currentPosition, shelter.position, shelter.shelter.hitRadius);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    hitEnemyIndex = -1;
                    hitShelterIndex = shelterIndex;
                }
            }

            if (hitEnemyIndex >= 0)
            {
                ProjectileHitSystem.DamageEnemy(ref enemies[hitEnemyIndex], hitEnemyIndex, melee.enemyDamage, random, enemyHitStunnedRecords);
                return true;
            }

            if (hitShelterIndex >= 0)
            {
                ref ShelterEntity hitShelter = ref shelters[hitShelterIndex];
                hitShelter.health = Mathf.Min(hitShelter.shelter.maxHealth, hitShelter.health + melee.buildingRepair);
                return true;
            }

            return false;
        }

        // XZ 평면에서 판정 상자와 point를 중심으로 한 반지름 radius의 원이 겹치는지 검사한다.
        private static bool OverlapsHitBox(Vector3 center, Vector3 forward, float halfLength, float halfWidth, Vector3 point, float radius)
        {
            float offsetX = point.x - center.x;
            float offsetZ = point.z - center.z;
            float alongLength = offsetX * forward.x + offsetZ * forward.z;
            // 오른쪽 방향은 (forward.z, -forward.x)다.
            float alongWidth = offsetX * forward.z - offsetZ * forward.x;
            float outsideLength = Mathf.Max(0f, Mathf.Abs(alongLength) - halfLength);
            float outsideWidth = Mathf.Max(0f, Mathf.Abs(alongWidth) - halfWidth);
            return outsideLength * outsideLength + outsideWidth * outsideWidth <= radius * radius;
        }

        // XZ 평면에서 from부터 center를 중심으로 한 반지름 radius의 원 경계까지의 거리를 구한다. 원 안이면 0이다.
        private static float GetDistanceToEdge(Vector3 from, Vector3 center, float radius)
        {
            Vector2 offset = new Vector2(center.x - from.x, center.z - from.z);
            return Mathf.Max(0f, offset.magnitude - radius);
        }
    }
}
