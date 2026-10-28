using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class EnemyProjectileMoveSystem
    {
        /// <summary>
        /// 살아 있는 적 투사체를 한 틱만큼 옮기고, 사거리를 다 날아간 투사체를 소멸시킨다.
        /// </summary>
        /// <param name="enemyProjectiles">옮길 적 투사체 저장소.</param>
        public static void Tick(EnemyProjectileStorage enemyProjectiles)
        {
            EnemyProjectileEntity[] entities = enemyProjectiles.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyProjectileEntity enemyProjectile = ref entities[index];
                if (!enemyProjectile.isAlive)
                {
                    continue;
                }

                Vector3 step = enemyProjectile.velocity * Time.fixedDeltaTime;
                enemyProjectile.previousPosition = enemyProjectile.currentPosition;
                enemyProjectile.currentPosition += step;
                enemyProjectile.remainingDistance -= step.magnitude;

                if (enemyProjectile.remainingDistance <= 0f)
                {
                    enemyProjectile.isAlive = false;
                    enemyProjectiles.freeIndices.Add(index);
                }
            }
        }
    }
}
