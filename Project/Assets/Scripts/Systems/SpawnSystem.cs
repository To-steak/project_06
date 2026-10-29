using System.Collections.Generic;
using Project06.Components;
using Project06.Entities;
using Project06.Requests;
using UnityEngine;

namespace Project06.Systems
{
    public static class SpawnSystem
    {
        /// <summary>
        /// 생성 요청 큐를 비우며 엔티티를 생성한다. 틱 마지막에 호출한다. 빈 슬롯이 없으면 그 요청은 버린다.
        /// </summary>
        /// <param name="playerSpawnRequests">플레이어 생성 요청 큐.</param>
        /// <param name="players">플레이어를 넣을 저장소.</param>
        /// <param name="shelterSpawnRequests">쉘터 생성 요청 큐.</param>
        /// <param name="shelters">쉘터를 넣을 저장소.</param>
        /// <param name="dayNightSpawnRequests">낮밤 생성 요청 큐.</param>
        /// <param name="dayNights">낮밤을 넣을 저장소.</param>
        /// <param name="enemySpawnerSpawnRequests">적 스포너 생성 요청 큐.</param>
        /// <param name="enemySpawners">적 스포너를 넣을 저장소.</param>
        /// <param name="bossSpawnerSpawnRequests">보스 스포너 생성 요청 큐.</param>
        /// <param name="bossSpawners">보스 스포너를 넣을 저장소.</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐.</param>
        /// <param name="enemies">적을 넣을 저장소.</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐.</param>
        /// <param name="projectiles">투사체를 넣을 저장소.</param>
        /// <param name="enemyProjectileSpawnRequests">적 투사체 생성 요청 큐.</param>
        /// <param name="enemyProjectiles">적 투사체를 넣을 저장소.</param>
        public static void Tick(
            Queue<PlayerSpawnRequest> playerSpawnRequests, PlayerStorage players,
            Queue<ShelterSpawnRequest> shelterSpawnRequests, ShelterStorage shelters,
            Queue<DayNightSpawnRequest> dayNightSpawnRequests, DayNightStorage dayNights,
            Queue<EnemySpawnerSpawnRequest> enemySpawnerSpawnRequests, EnemySpawnerStorage enemySpawners,
            Queue<BossSpawnerSpawnRequest> bossSpawnerSpawnRequests, BossSpawnerStorage bossSpawners,
            Queue<EnemySpawnRequest> enemySpawnRequests, EnemyStorage enemies,
            Queue<ProjectileSpawnRequest> projectileSpawnRequests, ProjectileStorage projectiles,
            Queue<EnemyProjectileSpawnRequest> enemyProjectileSpawnRequests, EnemyProjectileStorage enemyProjectiles)
        {
            SpawnPlayers(playerSpawnRequests, players);
            SpawnShelters(shelterSpawnRequests, shelters);
            SpawnDayNights(dayNightSpawnRequests, dayNights);
            SpawnEnemySpawners(enemySpawnerSpawnRequests, enemySpawners);
            SpawnBossSpawners(bossSpawnerSpawnRequests, bossSpawners);
            SpawnEnemies(enemySpawnRequests, enemies);
            SpawnProjectiles(projectileSpawnRequests, projectiles);
            SpawnEnemyProjectiles(enemyProjectileSpawnRequests, enemyProjectiles);
        }

        private static void SpawnPlayers(Queue<PlayerSpawnRequest> playerSpawnRequests, PlayerStorage players)
        {
            while (playerSpawnRequests.Count > 0)
            {
                PlayerSpawnRequest request = playerSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(players.freeIndices, out int index))
                {
                    continue;
                }

                ref PlayerEntity player = ref players.entities[index];
                int generation = player.generation + 1;
                player = new PlayerEntity
                {
                    isAlive = true,
                    generation = generation,
                    previousPosition = request.position,
                    currentPosition = request.position,
                    facingDirection = request.facingDirection,
                    spawnPosition = request.position,
                    spawnFacingDirection = request.facingDirection,
                    player = request.player,
                    health = request.player.maxHealth,
                    movement = request.movement,
                    weaponIndex = request.weaponIndex,
                    weapon = request.weapon,
                    caliberIndex = request.caliberIndex,
                    magazineAmmo = request.magazineAmmo,
                    reserveAmmo = request.reserveAmmo,
                };
            }
        }

        private static void SpawnShelters(Queue<ShelterSpawnRequest> shelterSpawnRequests, ShelterStorage shelters)
        {
            while (shelterSpawnRequests.Count > 0)
            {
                ShelterSpawnRequest request = shelterSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(shelters.freeIndices, out int index))
                {
                    continue;
                }

                ref ShelterEntity shelter = ref shelters.entities[index];
                shelter.isAlive = true;
                shelter.generation++;
                shelter.position = request.position;
                shelter.shelter = request.shelter;
                shelter.health = request.shelter.maxHealth;
                shelter.isDestroyed = false;
            }
        }

        private static void SpawnDayNights(Queue<DayNightSpawnRequest> dayNightSpawnRequests, DayNightStorage dayNights)
        {
            while (dayNightSpawnRequests.Count > 0)
            {
                DayNightSpawnRequest request = dayNightSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(dayNights.freeIndices, out int index))
                {
                    continue;
                }

                ref DayNightEntity dayNight = ref dayNights.entities[index];
                dayNight.isAlive = true;
                dayNight.generation++;
                dayNight.dayNight = request.dayNight;
                dayNight.isNight = false;
                dayNight.remainingTicks = Mathf.Max(1, request.dayNight.dayTicks);
                dayNight.dayCount = 1;
            }
        }

        private static void SpawnEnemySpawners(Queue<EnemySpawnerSpawnRequest> enemySpawnerSpawnRequests, EnemySpawnerStorage enemySpawners)
        {
            while (enemySpawnerSpawnRequests.Count > 0)
            {
                EnemySpawnerSpawnRequest request = enemySpawnerSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(enemySpawners.freeIndices, out int index))
                {
                    continue;
                }

                ref EnemySpawnerEntity enemySpawner = ref enemySpawners.entities[index];
                enemySpawner.isAlive = true;
                enemySpawner.generation++;
                enemySpawner.position = request.position;
                enemySpawner.facingDirection = request.facingDirection;
                enemySpawner.spawner = request.spawner;
                enemySpawner.enemyTypeIndex = request.enemyTypeIndex;
                enemySpawner.enemy = request.enemy;
                enemySpawner.spawnRemainingTicks = request.spawner.firstSpawnDelayTicks;
                enemySpawner.spawnedCount = 0;
            }
        }

        private static void SpawnBossSpawners(Queue<BossSpawnerSpawnRequest> bossSpawnerSpawnRequests, BossSpawnerStorage bossSpawners)
        {
            while (bossSpawnerSpawnRequests.Count > 0)
            {
                BossSpawnerSpawnRequest request = bossSpawnerSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(bossSpawners.freeIndices, out int index))
                {
                    continue;
                }

                ref BossSpawnerEntity bossSpawner = ref bossSpawners.entities[index];
                bossSpawner.isAlive = true;
                bossSpawner.generation++;
                bossSpawner.position = request.position;
                bossSpawner.facingDirection = request.facingDirection;
                bossSpawner.spawner = request.spawner;
                bossSpawner.enemyTypeIndex = request.enemyTypeIndex;
                bossSpawner.enemy = request.enemy;
                bossSpawner.spawnRemainingTicks = request.spawner.firstSpawnDelayTicks;
                bossSpawner.hasSpawned = false;
            }
        }

        private static void SpawnEnemies(Queue<EnemySpawnRequest> enemySpawnRequests, EnemyStorage enemies)
        {
            while (enemySpawnRequests.Count > 0)
            {
                EnemySpawnRequest request = enemySpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(enemies.freeIndices, out int index))
                {
                    continue;
                }

                ref EnemyEntity enemy = ref enemies.entities[index];
                enemy.isAlive = true;
                enemy.generation++;
                enemy.enemyTypeIndex = request.enemyTypeIndex;
                enemy.spawner = request.spawner;
                enemy.isBoss = request.isBoss;
                enemy.previousPosition = request.position;
                enemy.currentPosition = request.position;
                enemy.facingDirection = request.facingDirection;
                enemy.velocity = Vector3.zero;
                enemy.enemy = request.enemy;
                enemy.health = request.enemy.maxHealth;
                enemy.state = EnemyState.Idle;
                enemy.targetKind = EnemyTargetKind.Player;
                enemy.idleRemainingTicks = 0;
                enemy.deathRemainingTicks = 0;
                enemy.hitStunRemainingTicks = 0;
                enemy.attackRemainingTicks = 0;
                enemy.attackPhase = EnemyAttackPhase.Windup;
                enemy.hasAttackDamaged = false;
                // 슬롯의 쿨다운 배열이 이 적의 공격 수보다 작을 때만 새로 만들고, 아니면 재사용한다.
                int attackCount = request.enemy.attacks != null ? request.enemy.attacks.Length : 0;
                if (enemy.attackCooldownRemainingTicks.Length < attackCount)
                {
                    enemy.attackCooldownRemainingTicks = new int[attackCount];
                }
                else
                {
                    System.Array.Clear(enemy.attackCooldownRemainingTicks, 0, enemy.attackCooldownRemainingTicks.Length);
                }
                enemy.pathCornerCount = 0;
                enemy.pathCornerIndex = 0;
                enemy.pathUpdateRemainingTicks = 0;
            }
        }

        private static void SpawnProjectiles(Queue<ProjectileSpawnRequest> projectileSpawnRequests, ProjectileStorage projectiles)
        {
            while (projectileSpawnRequests.Count > 0)
            {
                ProjectileSpawnRequest request = projectileSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(projectiles.freeIndices, out int index))
                {
                    continue;
                }

                ref ProjectileEntity projectile = ref projectiles.entities[index];
                projectile.isAlive = true;
                projectile.generation++;
                projectile.weaponIndex = request.weaponIndex;
                projectile.previousPosition = request.position;
                projectile.currentPosition = request.position;
                projectile.velocity = request.velocity;
                projectile.remainingDistance = request.range;
                projectile.damage = request.damage;
            }
        }

        private static void SpawnEnemyProjectiles(Queue<EnemyProjectileSpawnRequest> enemyProjectileSpawnRequests, EnemyProjectileStorage enemyProjectiles)
        {
            while (enemyProjectileSpawnRequests.Count > 0)
            {
                EnemyProjectileSpawnRequest request = enemyProjectileSpawnRequests.Dequeue();
                if (!TryTakeFreeIndex(enemyProjectiles.freeIndices, out int index))
                {
                    continue;
                }

                ref EnemyProjectileEntity enemyProjectile = ref enemyProjectiles.entities[index];
                enemyProjectile.isAlive = true;
                enemyProjectile.generation++;
                enemyProjectile.enemyTypeIndex = request.enemyTypeIndex;
                enemyProjectile.previousPosition = request.position;
                enemyProjectile.currentPosition = request.position;
                enemyProjectile.velocity = request.velocity;
                enemyProjectile.remainingDistance = request.range;
                enemyProjectile.damage = request.damage;
            }
        }

        // 빈 슬롯 목록의 마지막 인덱스를 꺼낸다. 빈 슬롯이 없으면 false를 돌려준다.
        private static bool TryTakeFreeIndex(List<int> freeIndices, out int index)
        {
            index = -1;
            if (freeIndices.Count == 0)
            {
                return false;
            }

            index = freeIndices[freeIndices.Count - 1];
            freeIndices.RemoveAt(freeIndices.Count - 1);
            return true;
        }
    }
}
