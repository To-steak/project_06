using System.Collections.Generic;
using Project06.Components;
using Project06.Entities;
using Project06.Records;
using UnityEngine;

namespace Project06.Systems
{
    public static class PlayerMeleeSystem
    {
        // 몫의 수를 내림할 때 부동소수점 오차를 메우는 값
        private const float SHARE_EPSILON = 0.0001f;

        /// <summary>
        /// 근접 무기를 든 플레이어의 근접 공격 쿨다운을 한 틱 줄이고, 근접 공격 중이 아니면서 발사 입력을 누르고 있고 쿨다운이 끝났으면 근접 공격을 시작한다.
        /// 공격 중인 플레이어는 제자리에 서서 선딜레이 → 타격 구간 → 후딜레이를 거친다. 바라보는 방향은 CommandSystem이 고정한다.
        /// 타격 구간 동안 매 틱 바라보는 방향 기준 판정 상자(XZ 평면)와 적·쉘터·자원 노드의 피격 원이 겹치는지 검사해, 겹친 대상 중 피격 원 경계가 플레이어에게 가장 가까운 하나만 한 번 때린다.
        /// 적은 데미지를 받고(경직 확률은 투사체와 같다), 쉘터는 플레이어의 구조물이므로 체력을 회복한다.
        /// 자원 노드는 종류에 맞는 근접 데미지만큼 체력이 깎이고, 깎인 체력이 최대 체력을 그 노드의 몫 수로 나눈 만큼 넘을 때마다 플레이어가 몫당 자원을 얻는다. 체력이 0이 되면 다 캔 것이다.
        /// 지금 무기의 데미지가 0 이하인 종류의 자원 노드는 맞히지 않는다.
        /// 공격 하나마다 소리를 낼 기록을 하나만 남긴다. 자원 노드를 맞히면 그 틱에 자원 노드 피격 기록을, 적이나 쉘터를 맞히면 그 틱에 휘두른 기록을, 아무것도 맞히지 못하면 타격 구간이 끝나는 틱에 휘두른 기록을 남긴다.
        /// CommandSystem 다음, PlayerMoveSystem 앞에 호출한다.
        /// </summary>
        /// <param name="players">근접 공격할 플레이어 저장소.</param>
        /// <param name="enemyStorage">맞을 수 있는 적 저장소.</param>
        /// <param name="shelterStorage">수리될 수 있는 쉘터 저장소.</param>
        /// <param name="resourceNodeStorage">캘 수 있는 자원 노드 저장소.</param>
        /// <param name="random">경직 여부를 정할 난수 생성기.</param>
        /// <param name="playerMeleeStartedRecords">이번 틱에 근접 공격을 시작한 기록 목록.</param>
        /// <param name="playerMeleeSwungRecords">이번 틱에 근접 공격이 자원 노드를 맞히지 않고 휘둘러진 기록 목록.</param>
        /// <param name="enemyHitStunnedRecords">이번 틱에 경직에 걸린 기록 목록.</param>
        /// <param name="resourceNodeHitRecords">이번 틱에 근접 공격이 자원 노드를 맞힌 기록 목록.</param>
        public static void Tick(
            PlayerStorage players, EnemyStorage enemyStorage, ShelterStorage shelterStorage, ResourceNodeStorage resourceNodeStorage, System.Random random,
            List<PlayerMeleeStartedRecord> playerMeleeStartedRecords, List<PlayerMeleeSwungRecord> playerMeleeSwungRecords,
            List<EnemyHitStunnedRecord> enemyHitStunnedRecords, List<ResourceNodeHitRecord> resourceNodeHitRecords)
        {
            PlayerEntity[] entities = players.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref PlayerEntity player = ref entities[index];
                if (!player.isAlive || player.isDead || player.weaponIndex < 0 || player.weapon.kind != WeaponKind.Melee)
                {
                    continue;
                }

                Attack(
                    ref player, enemyStorage, shelterStorage, resourceNodeStorage, random,
                    playerMeleeStartedRecords, playerMeleeSwungRecords, enemyHitStunnedRecords, resourceNodeHitRecords);
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
            ref PlayerEntity player, EnemyStorage enemyStorage, ShelterStorage shelterStorage, ResourceNodeStorage resourceNodeStorage, System.Random random,
            List<PlayerMeleeStartedRecord> playerMeleeStartedRecords, List<PlayerMeleeSwungRecord> playerMeleeSwungRecords,
            List<EnemyHitStunnedRecord> enemyHitStunnedRecords, List<ResourceNodeHitRecord> resourceNodeHitRecords)
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
            TickAttack(ref player, enemyStorage, shelterStorage, resourceNodeStorage, random, playerMeleeSwungRecords, enemyHitStunnedRecords, resourceNodeHitRecords);
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
        private static void TickAttack(
            ref PlayerEntity player, EnemyStorage enemyStorage, ShelterStorage shelterStorage, ResourceNodeStorage resourceNodeStorage, System.Random random,
            List<PlayerMeleeSwungRecord> playerMeleeSwungRecords, List<EnemyHitStunnedRecord> enemyHitStunnedRecords, List<ResourceNodeHitRecord> resourceNodeHitRecords)
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
                    player.hasMeleeHit = TryHit(
                        ref player, in melee, enemyStorage, shelterStorage, resourceNodeStorage, random,
                        playerMeleeSwungRecords, enemyHitStunnedRecords, resourceNodeHitRecords);
                }

                player.meleeRemainingTicks--;
                if (player.meleeRemainingTicks > 0)
                {
                    return;
                }

                // 끝까지 아무것도 맞히지 못했으면 휘두르는 소리만 낸다.
                if (!player.hasMeleeHit)
                {
                    playerMeleeSwungRecords.Add(new PlayerMeleeSwungRecord { weaponIndex = player.weaponIndex });
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

        // 판정 상자와 피격 원이 겹치는 살아 있는 적, 파괴되지 않은 쉘터, 다 캐지 않은 자원 노드 중 피격 원 경계가 플레이어에게 가장 가까운 하나를 때린다. 맞혔으면 true를 돌려준다.
        private static bool TryHit(
            ref PlayerEntity player, in MeleeWeaponComponent melee, EnemyStorage enemyStorage, ShelterStorage shelterStorage, ResourceNodeStorage resourceNodeStorage,
            System.Random random, List<PlayerMeleeSwungRecord> playerMeleeSwungRecords, List<EnemyHitStunnedRecord> enemyHitStunnedRecords, List<ResourceNodeHitRecord> resourceNodeHitRecords)
        {
            GetHitBox(in player, out Vector3 center, out Vector3 forward, out float halfLength, out float halfWidth);
            int hitEnemyIndex = -1;
            int hitShelterIndex = -1;
            int hitResourceNodeIndex = -1;
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

            ResourceNodeEntity[] resourceNodes = resourceNodeStorage.entities;
            for (int resourceNodeIndex = 0; resourceNodeIndex < resourceNodes.Length; resourceNodeIndex++)
            {
                ref ResourceNodeEntity resourceNode = ref resourceNodes[resourceNodeIndex];
                if (!resourceNode.isAlive || resourceNode.isDepleted || GetResourceNodeDamage(in melee, resourceNode.node.kind) <= 0f)
                {
                    continue;
                }

                if (!OverlapsHitBox(center, forward, halfLength, halfWidth, resourceNode.position, resourceNode.node.hitRadius))
                {
                    continue;
                }

                float distance = GetDistanceToEdge(player.currentPosition, resourceNode.position, resourceNode.node.hitRadius);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    hitEnemyIndex = -1;
                    hitShelterIndex = -1;
                    hitResourceNodeIndex = resourceNodeIndex;
                }
            }

            if (hitEnemyIndex >= 0)
            {
                ProjectileHitSystem.DamageEnemy(ref enemies[hitEnemyIndex], hitEnemyIndex, melee.enemyDamage, random, enemyHitStunnedRecords);
                playerMeleeSwungRecords.Add(new PlayerMeleeSwungRecord { weaponIndex = player.weaponIndex });
                return true;
            }

            if (hitShelterIndex >= 0)
            {
                ref ShelterEntity hitShelter = ref shelters[hitShelterIndex];
                hitShelter.health = Mathf.Min(hitShelter.shelter.maxHealth, hitShelter.health + melee.buildingRepair);
                playerMeleeSwungRecords.Add(new PlayerMeleeSwungRecord { weaponIndex = player.weaponIndex });
                return true;
            }

            if (hitResourceNodeIndex >= 0)
            {
                ref ResourceNodeEntity hitResourceNode = ref resourceNodes[hitResourceNodeIndex];
                Harvest(ref player, ref hitResourceNode, GetResourceNodeDamage(in melee, hitResourceNode.node.kind));
                resourceNodeHitRecords.Add(new ResourceNodeHitRecord
                {
                    node = new EntityIdentifier { index = hitResourceNodeIndex, generation = hitResourceNode.generation },
                    isDepleted = hitResourceNode.isDepleted,
                });
                return true;
            }

            return false;
        }

        // 자원 노드 종류에 맞는 근접 무기 데미지를 고른다.
        private static float GetResourceNodeDamage(in MeleeWeaponComponent melee, ResourceNodeKind kind)
        {
            return kind == ResourceNodeKind.Tree ? melee.treeDamage : melee.rockDamage;
        }

        // 자원 노드의 체력을 깎고, 새로 넘은 몫의 수 × 몫당 자원만큼 플레이어에게 자원을 준다. 체력이 0이 되면 다 캔 것으로 표시하고 재생까지 남은 날을 정한다.
        private static void Harvest(ref PlayerEntity player, ref ResourceNodeEntity resourceNode, float damage)
        {
            resourceNode.health = Mathf.Max(0f, resourceNode.health - damage);
            int harvestedShareCount = GetHarvestedShareCount(in resourceNode);
            int amount = (harvestedShareCount - resourceNode.harvestedShareCount) * resourceNode.node.resourcePerShare;
            resourceNode.harvestedShareCount = harvestedShareCount;
            if (resourceNode.node.kind == ResourceNodeKind.Tree)
            {
                player.wood += amount;
            }
            else
            {
                player.stone += amount;
            }

            if (resourceNode.health <= 0f)
            {
                resourceNode.isDepleted = true;
                resourceNode.regrowRemainingDays = resourceNode.node.regrowDays;
            }
        }

        // 깎인 체력으로 지금까지 넘은 몫의 수를 구한다. 마지막 몫은 체력이 0이 되어야 넘은 것으로 본다.
        private static int GetHarvestedShareCount(in ResourceNodeEntity resourceNode)
        {
            float maxHealth = resourceNode.node.maxHealth;
            int shareCount = Mathf.Max(1, resourceNode.node.shareCount);
            if (resourceNode.health <= 0f || maxHealth <= 0f)
            {
                return shareCount;
            }

            // 20 / 100 × 10이 1.9999로 계산되어 몫 하나를 덜 주지 않도록 작은 값을 더한다.
            float lostShareCount = (maxHealth - resourceNode.health) / maxHealth * shareCount;
            return Mathf.Clamp(Mathf.FloorToInt(lostShareCount + SHARE_EPSILON), 0, shareCount - 1);
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
