using Project06.Components;
using Project06.Entities;

namespace Project06.Systems
{
    public static class GameResultSystem
    {
        /// <summary>
        /// 판이 진행 중이면 승패를 정한다. 살아 있는 보스 중 Dead가 된 것이 있으면 승리, 아니면 파괴된 쉘터가 있으면 패배다.
        /// 같은 틱에 둘 다 일어나면 승리로 본다. 승패가 나면 Runner가 시뮬레이션을 멈춘다.
        /// 보스와 쉘터가 데미지를 받고 파괴가 정해지는 ShelterDestroySystem 다음에 호출한다.
        /// </summary>
        /// <param name="enemies">보스가 처치되었는지 볼 적 저장소. 읽기만 한다.</param>
        /// <param name="shelters">파괴되었는지 볼 쉘터 저장소. 읽기만 한다.</param>
        /// <param name="gameResult">판의 승패.</param>
        public static void Tick(EnemyStorage enemies, ShelterStorage shelters, ref GameResult gameResult)
        {
            if (gameResult != GameResult.Playing)
            {
                return;
            }

            if (IsBossKilled(enemies))
            {
                gameResult = GameResult.Victory;
            }
            else if (IsShelterDestroyed(shelters))
            {
                gameResult = GameResult.Defeat;
            }
        }

        // 살아 있는 보스 중 Dead인 것이 있는지 확인한다.
        private static bool IsBossKilled(EnemyStorage enemies)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                if (enemy.isAlive && enemy.isBoss && enemy.state == EnemyState.Dead)
                {
                    return true;
                }
            }

            return false;
        }

        // 살아 있는 쉘터 중 파괴된 것이 있는지 확인한다.
        private static bool IsShelterDestroyed(ShelterStorage shelters)
        {
            ShelterEntity[] entities = shelters.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref ShelterEntity shelter = ref entities[index];
                if (shelter.isAlive && shelter.isDestroyed)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
