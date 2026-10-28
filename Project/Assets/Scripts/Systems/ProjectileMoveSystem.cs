using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class ProjectileMoveSystem
    {
        /// <summary>
        /// 살아 있는 투사체를 한 틱만큼 옮기고, 사거리를 다 날아간 투사체를 소멸시킨다.
        /// </summary>
        /// <param name="projectiles">옮길 투사체 저장소.</param>
        public static void Tick(ProjectileStorage projectiles)
        {
            ProjectileEntity[] entities = projectiles.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref ProjectileEntity projectile = ref entities[index];
                if (!projectile.isAlive)
                {
                    continue;
                }

                Vector3 step = projectile.velocity * Time.fixedDeltaTime;
                projectile.previousPosition = projectile.currentPosition;
                projectile.currentPosition += step;
                projectile.remainingDistance -= step.magnitude;

                if (projectile.remainingDistance <= 0f)
                {
                    projectile.isAlive = false;
                    projectiles.freeIndices.Add(index);
                }
            }
        }
    }
}
