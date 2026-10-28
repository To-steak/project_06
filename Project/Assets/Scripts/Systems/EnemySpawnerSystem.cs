using System.Collections.Generic;
using Project06.Entities;
using Project06.Requests;
using UnityEngine;

namespace Project06.Systems
{
    public static class EnemySpawnerSystem
    {
        /// <summary>
        /// 살아 있는 스포너마다 자기가 낸 적 중 살아 있는 수를 세어, 최대 동시 수보다 적으면 대기 시간을 한 틱 줄이고 대기가 끝나면 적 생성을 요청한다.
        /// 적은 스포너 위치를 중심으로 한 반지름 안의 무작위 지점에 스포너가 바라보는 방향으로 생긴다. 총수를 다 낸 스포너는 소멸시킨다.
        /// SpawnSystem 바로 앞에 호출한다.
        /// </summary>
        /// <param name="spawners">적 스포너 저장소.</param>
        /// <param name="enemies">살아 있는 수를 셀 적 저장소. 읽기만 한다.</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐.</param>
        /// <param name="random">생성 위치를 정할 난수 생성기.</param>
        public static void Tick(EnemySpawnerStorage spawners, EnemyStorage enemies, Queue<EnemySpawnRequest> enemySpawnRequests, System.Random random)
        {
            EnemySpawnerEntity[] entities = spawners.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemySpawnerEntity spawner = ref entities[index];
                if (!spawner.isAlive)
                {
                    continue;
                }

                EntityIdentifier spawnerIdentifier = new EntityIdentifier { index = index, generation = spawner.generation };
                int maxAliveCount = spawner.spawner.maxAliveCount;
                if (maxAliveCount > 0 && CountAliveEnemies(enemies, spawnerIdentifier) >= maxAliveCount)
                {
                    continue;
                }

                if (spawner.spawnRemainingTicks > 0)
                {
                    spawner.spawnRemainingTicks--;
                }

                if (spawner.spawnRemainingTicks > 0)
                {
                    continue;
                }

                enemySpawnRequests.Enqueue(new EnemySpawnRequest
                {
                    enemyTypeIndex = spawner.enemyTypeIndex,
                    spawner = spawnerIdentifier,
                    position = GetSpawnPosition(in spawner, random),
                    facingDirection = spawner.facingDirection,
                    enemy = spawner.enemy,
                });
                spawner.spawnedCount++;
                spawner.spawnRemainingTicks = Mathf.Max(1, spawner.spawner.intervalTicks);

                int totalSpawnCount = spawner.spawner.totalSpawnCount;
                if (totalSpawnCount > 0 && spawner.spawnedCount >= totalSpawnCount)
                {
                    spawner.isAlive = false;
                    spawners.freeIndices.Add(index);
                }
            }
        }

        // 스포너가 낸 적 중 살아 있는 수를 센다.
        private static int CountAliveEnemies(EnemyStorage enemies, EntityIdentifier spawnerIdentifier)
        {
            int aliveCount = 0;
            EnemyEntity[] entities = enemies.entities;
            for (int enemyIndex = 0; enemyIndex < entities.Length; enemyIndex++)
            {
                ref EnemyEntity enemy = ref entities[enemyIndex];
                if (enemy.isAlive && enemy.spawner.index == spawnerIdentifier.index && enemy.spawner.generation == spawnerIdentifier.generation)
                {
                    aliveCount++;
                }
            }

            return aliveCount;
        }

        // 스포너 위치를 중심으로 한 XZ 평면 원 안에서 고르게 무작위 지점을 고른다.
        private static Vector3 GetSpawnPosition(in EnemySpawnerEntity spawner, System.Random random)
        {
            float radius = spawner.spawner.spawnRadius;
            if (radius <= 0f)
            {
                return spawner.position;
            }

            float angle = (float)(random.NextDouble() * 2.0 * Mathf.PI);
            float distance = radius * Mathf.Sqrt((float)random.NextDouble());
            return spawner.position + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);
        }
    }
}
