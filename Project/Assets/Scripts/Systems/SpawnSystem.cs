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
        /// <param name="projectileSpawnRequests">투사체 생성 요청 큐.</param>
        /// <param name="projectiles">투사체를 넣을 저장소.</param>
        public static void Tick(Queue<ProjectileSpawnRequest> projectileSpawnRequests, ProjectileStorage projectiles)
        {
            while (projectileSpawnRequests.Count > 0)
            {
                ProjectileSpawnRequest request = projectileSpawnRequests.Dequeue();
                List<int> freeIndices = projectiles.freeIndices;
                if (freeIndices.Count == 0)
                {
                    continue;
                }

                int index = freeIndices[freeIndices.Count - 1];
                freeIndices.RemoveAt(freeIndices.Count - 1);

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
    }
}
