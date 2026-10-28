using Project06.Entities;
using UnityEngine;

namespace Project06.Systems
{
    public static class ProjectileHitSystem
    {
        /// <summary>
        /// 살아 있는 투사체마다 이번 틱에 날아갈 선분이 살아 있는 적의 피격 원(XZ 평면)과 만나는지 검사한다. ProjectileMoveSystem보다 먼저 호출한다.
        /// 가장 먼저 만나는 적에게 데미지를 주고 투사체를 소멸시키며, 체력이 0 이하가 된 적은 죽인다. 높이(Y)는 판정에 쓰지 않는다.
        /// </summary>
        /// <param name="projectiles">검사할 투사체 저장소.</param>
        /// <param name="enemies">맞을 수 있는 적 목록.</param>
        public static void Tick(ProjectileStorage projectiles, EnemyEntity[] enemies)
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
                float stepLength = step.magnitude;
                if (stepLength > projectile.remainingDistance && stepLength > 0f)
                {
                    step *= projectile.remainingDistance / stepLength;
                }

                int hitEnemyIndex = -1;
                float nearestHitRatio = float.MaxValue;
                for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++)
                {
                    ref EnemyEntity enemy = ref enemies[enemyIndex];
                    if (!enemy.isAlive)
                    {
                        continue;
                    }

                    if (TryGetHitRatio(projectile.currentPosition, step, enemy.position, enemy.enemy.hitRadius, out float hitRatio) && hitRatio < nearestHitRatio)
                    {
                        nearestHitRatio = hitRatio;
                        hitEnemyIndex = enemyIndex;
                    }
                }

                if (hitEnemyIndex < 0)
                {
                    continue;
                }

                ref EnemyEntity hitEnemy = ref enemies[hitEnemyIndex];
                hitEnemy.health -= projectile.damage;
                if (hitEnemy.health <= 0f)
                {
                    hitEnemy.isAlive = false;
                }

                projectile.isAlive = false;
                projectiles.freeIndices.Add(index);
            }
        }

        // XZ 평면에서 start부터 start + step까지의 선분이 center를 중심으로 한 반지름 radius의 원과 처음 만나는 비율(0~1)을 구한다. 시작점이 원 안이면 0이다.
        private static bool TryGetHitRatio(Vector3 start, Vector3 step, Vector3 center, float radius, out float hitRatio)
        {
            hitRatio = 0f;
            Vector2 offset = new Vector2(start.x - center.x, start.z - center.z);
            Vector2 direction = new Vector2(step.x, step.z);

            float c = Vector2.Dot(offset, offset) - radius * radius;
            if (c <= 0f)
            {
                return true;
            }

            float a = Vector2.Dot(direction, direction);
            if (a <= 0f)
            {
                return false;
            }

            float b = 2f * Vector2.Dot(offset, direction);
            float discriminant = b * b - 4f * a * c;
            if (discriminant < 0f)
            {
                return false;
            }

            hitRatio = (-b - Mathf.Sqrt(discriminant)) / (2f * a);
            return hitRatio >= 0f && hitRatio <= 1f;
        }
    }
}
