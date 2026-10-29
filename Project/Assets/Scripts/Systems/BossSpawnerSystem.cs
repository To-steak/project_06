using System.Collections.Generic;
using Project06.Entities;
using Project06.Requests;

namespace Project06.Systems
{
    public static class BossSpawnerSystem
    {
        /// <summary>
        /// 아직 보스를 내지 않은 보스 스포너마다, 살아 있는 낮밤 중 하나가 마지막 날의 밤이면 대기 시간을 한 틱 줄이고 대기가 끝나면 보스 생성을 요청한다.
        /// 보스는 스포너 위치에 스포너가 바라보는 방향으로 생기며, 한 스포너는 보스를 한 번만 낸다. 낮밤이 없거나 마지막 날이 없으면 내지 않는다.
        /// SpawnSystem 바로 앞에 호출한다.
        /// </summary>
        /// <param name="dayNights">지금이 마지막 날의 밤인지 볼 낮밤 저장소. 읽기만 한다.</param>
        /// <param name="bossSpawners">보스 스포너 저장소.</param>
        /// <param name="enemySpawnRequests">적 생성 요청 큐.</param>
        public static void Tick(DayNightStorage dayNights, BossSpawnerStorage bossSpawners, Queue<EnemySpawnRequest> enemySpawnRequests)
        {
            BossSpawnerEntity[] entities = bossSpawners.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref BossSpawnerEntity bossSpawner = ref entities[index];
                if (!bossSpawner.isAlive || bossSpawner.hasSpawned || !IsFinalNight(dayNights))
                {
                    continue;
                }

                if (bossSpawner.spawnRemainingTicks > 0)
                {
                    bossSpawner.spawnRemainingTicks--;
                }

                if (bossSpawner.spawnRemainingTicks > 0)
                {
                    continue;
                }

                enemySpawnRequests.Enqueue(new EnemySpawnRequest
                {
                    enemyTypeIndex = bossSpawner.enemyTypeIndex,
                    // 적 스포너가 낸 적이 아니므로 어떤 적 스포너도 가리키지 않는다.
                    spawner = new EntityIdentifier { index = -1, generation = 0 },
                    position = bossSpawner.position,
                    facingDirection = bossSpawner.facingDirection,
                    enemy = bossSpawner.enemy,
                    isBoss = true,
                });
                bossSpawner.hasSpawned = true;
            }
        }

        // 살아 있는 낮밤 중 지금이 마지막 날의 밤인 것이 있는지 확인한다.
        private static bool IsFinalNight(DayNightStorage dayNights)
        {
            DayNightEntity[] entities = dayNights.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref DayNightEntity dayNight = ref entities[index];
                if (dayNight.isAlive && DayNightSystem.IsFinalNight(in dayNight))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
