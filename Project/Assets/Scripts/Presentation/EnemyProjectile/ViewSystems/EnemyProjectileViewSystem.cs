using Project06.Entities;
using UnityEngine;
using UnityEngine.Pool;

namespace Project06.Presentation.EnemyProjectile.ViewSystems
{
    public static class EnemyProjectileViewSystem
    {
        // 종류별 풀이 처음 확보하는 View 수
        private const int DEFAULT_POOL_CAPACITY = 16;

        /// <summary>
        /// 적 종류별 적 투사체 View 풀과 슬롯별 View 매핑을 만든다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="projectilePrefabs">적 종류별 투사체 프리팹. 적 종류 인덱스와 같은 순서이며 null이 섞여 있어도 된다.</param>
        /// <param name="capacity">적 투사체 저장소의 슬롯 수.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(GameObject[] projectilePrefabs, int capacity, ViewContext context)
        {
            context.enemyProjectilePools = new ObjectPool<GameObject>[projectilePrefabs.Length];
            for (int enemyTypeIndex = 0; enemyTypeIndex < projectilePrefabs.Length; enemyTypeIndex++)
            {
                GameObject prefab = projectilePrefabs[enemyTypeIndex];
                if (prefab == null)
                {
                    continue;
                }

                context.enemyProjectilePools[enemyTypeIndex] = new ObjectPool<GameObject>(
                    () => Object.Instantiate(prefab),
                    view => view.SetActive(true),
                    view => view.SetActive(false),
                    view => Object.Destroy(view),
                    false,
                    DEFAULT_POOL_CAPACITY,
                    capacity);
            }

            context.enemyProjectileViews = new GameObject[capacity];
            context.enemyProjectileViewGenerations = new int[capacity];
            context.enemyProjectileViewPoolIndices = new int[capacity];
        }

        /// <summary>
        /// 살아 있는 적 투사체마다 View를 풀에서 꺼내 직전 틱과 현재 틱 사이로 보간해 놓고(새로 꺼낸 View는 꼬리를 지우고), 소멸했거나 슬롯이 재사용된 투사체의 View는 풀에 반납한다.
        /// </summary>
        /// <param name="enemyProjectiles">표시할 적 투사체 저장소.</param>
        /// <param name="interpolation">직전 틱에서 현재 틱까지 진행한 비율(0~1).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(EnemyProjectileStorage enemyProjectiles, float interpolation, ViewContext context)
        {
            EnemyProjectileEntity[] entities = enemyProjectiles.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyProjectileEntity enemyProjectile = ref entities[index];
                GameObject view = context.enemyProjectileViews[index];

                if (view != null && (!enemyProjectile.isAlive || context.enemyProjectileViewGenerations[index] != enemyProjectile.generation))
                {
                    context.enemyProjectilePools[context.enemyProjectileViewPoolIndices[index]].Release(view);
                    context.enemyProjectileViews[index] = null;
                    view = null;
                }

                if (!enemyProjectile.isAlive)
                {
                    continue;
                }

                bool isNewView = view == null;
                if (isNewView)
                {
                    ObjectPool<GameObject> pool = GetPool(enemyProjectile.enemyTypeIndex, context);
                    if (pool == null)
                    {
                        continue;
                    }

                    view = pool.Get();
                    context.enemyProjectileViews[index] = view;
                    context.enemyProjectileViewGenerations[index] = enemyProjectile.generation;
                    context.enemyProjectileViewPoolIndices[index] = enemyProjectile.enemyTypeIndex;
                }

                view.transform.position = Vector3.Lerp(enemyProjectile.previousPosition, enemyProjectile.currentPosition, interpolation);
                if (enemyProjectile.velocity.sqrMagnitude > 0f)
                {
                    view.transform.rotation = Quaternion.LookRotation(enemyProjectile.velocity, Vector3.up);
                }

                if (isNewView)
                {
                    ClearTrails(view);
                }
            }
        }

        // 풀에서 다시 꺼낸 투사체가 이전에 날아간 자리부터 꼬리를 그리지 않도록, 출발 위치로 옮긴 뒤 꼬리를 지운다.
        private static void ClearTrails(GameObject view)
        {
            TrailRenderer trail = view.GetComponentInChildren<TrailRenderer>();
            if (trail != null)
            {
                trail.Clear();
            }
        }

        private static ObjectPool<GameObject> GetPool(int enemyTypeIndex, ViewContext context)
        {
            if (enemyTypeIndex < 0 || enemyTypeIndex >= context.enemyProjectilePools.Length)
            {
                return null;
            }

            return context.enemyProjectilePools[enemyTypeIndex];
        }
    }
}
