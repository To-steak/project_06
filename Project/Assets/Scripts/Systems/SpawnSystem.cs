using System.Collections.Generic;
using Project06.Entities;
using Project06.Requests;

namespace Project06.Systems
{
    public static class SpawnSystem
    {
        /// <summary>
        /// 생성 요청 큐를 비우며 엔티티를 생성한다. 틱 마지막에 호출한다. 빈 슬롯이 없으면 그 요청은 버린다.
        /// </summary>
        /// <param name="playerSpawnRequests">플레이어 생성 요청 큐.</param>
        /// <param name="players">플레이어를 넣을 저장소.</param>
        /// <param name="enemySpawnerSpawnRequests">적 스포너 생성 요청 큐.</param>
        /// <param name="enemySpawners">적 스포너를 넣을 저장소.</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐.</param>
        /// <param name="enemies">적을 넣을 저장소.</param>
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐.</param>
        /// <param name="projectiles">투사체를 넣을 저장소.</param>
        public static void Tick(
            Queue<PlayerSpawnRequest> playerSpawnRequests, PlayerStorage players,
            Queue<EnemySpawnerSpawnRequest> enemySpawnerSpawnRequests, EnemySpawnerStorage enemySpawners,
            Queue<EnemySpawnRequest> enemySpawnRequests, EnemyStorage enemies,
            Queue<ProjectileSpawnRequest> projectileSpawnRequests, ProjectileStorage projectiles)
        {
            SpawnPlayers(playerSpawnRequests, players);
            SpawnEnemySpawners(enemySpawnerSpawnRequests, enemySpawners);
            SpawnEnemies(enemySpawnRequests, enemies);
            SpawnProjectiles(projectileSpawnRequests, projectiles);
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
                    movement = request.movement,
                    weaponIndex = request.weaponIndex,
                    weapon = request.weapon,
                    caliberIndex = request.caliberIndex,
                    magazineAmmo = request.magazineAmmo,
                    reserveAmmo = request.reserveAmmo,
                };
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
                enemy.position = request.position;
                enemy.facingDirection = request.facingDirection;
                enemy.enemy = request.enemy;
                enemy.health = request.enemy.maxHealth;
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
