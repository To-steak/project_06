using Project06.Entities;

namespace Project06.Systems
{
    public static class ShelterDestroySystem
    {
        /// <summary>
        /// 체력이 0 이하가 된 쉘터를 파괴 상태로 바꾼다. 파괴된 쉘터는 다시 회복하지 않는다.
        /// 쉘터가 데미지를 받는 EnemyAttackSystem과 EnemyProjectileHitSystem 다음에 호출한다.
        /// </summary>
        /// <param name="shelters">파괴를 처리할 쉘터 저장소.</param>
        public static void Tick(ShelterStorage shelters)
        {
            ShelterEntity[] entities = shelters.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref ShelterEntity shelter = ref entities[index];
                if (!shelter.isAlive || shelter.isDestroyed || shelter.health > 0f)
                {
                    continue;
                }

                shelter.isDestroyed = true;
                shelter.health = 0f;
            }
        }
    }
}
