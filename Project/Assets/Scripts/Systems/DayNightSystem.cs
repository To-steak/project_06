using Project06.Components;
using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class DayNightSystem
    {
        /// <summary>
        /// 살아 있는 낮밤마다 지금 시간대의 남은 시간을 한 틱 줄이고, 끝나면 낮과 밤을 바꾼다.
        /// 밤이 되면 살아 있는 스포너마다 대기 시간을 첫 적 대기 시간으로, 낸 적의 수를 0으로 되돌린다. 대기 시간은 밤이 시작된 틱부터 센다.
        /// 낮이 되면 살아 있는 적을 모두 Dead로 바꾼다. 이미 날아가는 적 투사체는 그대로 둔다.
        /// 마지막 날의 밤은 끝나지 않는다. 한밤(밤의 절반)까지 흐른 뒤 그대로 멈춘다.
        /// FixedUpdate 맨 앞에 호출한다.
        /// </summary>
        /// <param name="dayNights">낮밤 저장소.</param>
        /// <param name="spawners">밤이 될 때 대기 시간을 되돌릴 적 스포너 저장소.</param>
        /// <param name="enemies">낮이 될 때 사망시킬 적 저장소.</param>
        public static void Tick(DayNightStorage dayNights, EnemySpawnerStorage spawners, EnemyStorage enemies)
        {
            DayNightEntity[] entities = dayNights.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref DayNightEntity dayNight = ref entities[index];
                if (!dayNight.isAlive)
                {
                    continue;
                }

                if (IsFinalNight(in dayNight) && dayNight.remainingTicks <= GetMidnightRemainingTicks(in dayNight))
                {
                    continue;
                }

                dayNight.remainingTicks--;
                if (dayNight.remainingTicks > 0)
                {
                    continue;
                }

                dayNight.isNight = !dayNight.isNight;
                if (dayNight.isNight)
                {
                    dayNight.remainingTicks = Mathf.Max(1, dayNight.dayNight.nightTicks);
                    ResetSpawnerTimers(spawners);
                }
                else
                {
                    dayNight.remainingTicks = Mathf.Max(1, dayNight.dayNight.dayTicks);
                    dayNight.dayCount++;
                    KillAllEnemies(enemies);
                }
            }
        }

        /// <summary>
        /// 낮밤이 지금 마지막 날의 밤이라 끝나지 않는지 확인한다.
        /// </summary>
        /// <param name="dayNight">확인할 낮밤.</param>
        /// <returns>마지막 날이 있고, 지금이 그 날의 밤이면 true.</returns>
        public static bool IsFinalNight(in DayNightEntity dayNight)
        {
            int finalDay = dayNight.dayNight.finalDay;
            return dayNight.isNight && finalDay >= 1 && dayNight.dayCount == finalDay;
        }

        // 한밤일 때 밤이 끝날 때까지 남은 틱. 끝나지 않는 밤은 남은 틱이 여기까지 줄면 멈춘다.
        private static int GetMidnightRemainingTicks(in DayNightEntity dayNight)
        {
            return Mathf.Max(1, Mathf.Max(1, dayNight.dayNight.nightTicks) / 2);
        }

        // 살아 있는 스포너마다 다음 적을 낼 때까지 남은 틱을 첫 적 대기 시간으로, 낸 적의 수를 0으로 되돌린다.
        private static void ResetSpawnerTimers(EnemySpawnerStorage spawners)
        {
            EnemySpawnerEntity[] entities = spawners.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemySpawnerEntity spawner = ref entities[index];
                if (spawner.isAlive)
                {
                    spawner.spawnRemainingTicks = spawner.spawner.firstSpawnDelayTicks;
                    spawner.spawnedCount = 0;
                }
            }
        }

        // 살아 있고 아직 Dead가 아닌 적을 모두 Dead로 바꾼다.
        private static void KillAllEnemies(EnemyStorage enemies)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                if (!enemy.isAlive || enemy.state == EnemyState.Dead)
                {
                    continue;
                }

                enemy.health = 0f;
                enemy.state = EnemyState.Dead;
                enemy.deathRemainingTicks = enemy.enemy.deathDurationTicks;
                enemy.velocity = Vector3.zero;
            }
        }
    }
}
