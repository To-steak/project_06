using Project06.Entities;
using UnityEngine;
using UnityEngine.Pool;

namespace Project06.Presentation.Projectile.ViewSystems
{
    public static class ProjectileViewSystem
    {
        // 무기별 풀이 처음 확보하는 View 수
        private const int DEFAULT_POOL_CAPACITY = 32;

        /// <summary>
        /// 무기별 투사체 View 풀과 슬롯별 View 매핑을 만든다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="projectilePrefabs">무기별 투사체 프리팹. DefinitionCatalog.weapons와 같은 순서이며 null이 섞여 있어도 된다.</param>
        /// <param name="capacity">투사체 저장소의 슬롯 수.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(GameObject[] projectilePrefabs, int capacity, ViewContext context)
        {
            context.projectilePools = new ObjectPool<GameObject>[projectilePrefabs.Length];
            for (int weaponIndex = 0; weaponIndex < projectilePrefabs.Length; weaponIndex++)
            {
                GameObject prefab = projectilePrefabs[weaponIndex];
                if (prefab == null)
                {
                    continue;
                }

                context.projectilePools[weaponIndex] = new ObjectPool<GameObject>(
                    () => Object.Instantiate(prefab),
                    view => view.SetActive(true),
                    view => view.SetActive(false),
                    view => Object.Destroy(view),
                    false,
                    DEFAULT_POOL_CAPACITY,
                    capacity);
            }

            context.projectileViews = new GameObject[capacity];
            context.projectileViewGenerations = new int[capacity];
            context.projectileViewPoolIndices = new int[capacity];
        }

        /// <summary>
        /// 살아 있는 투사체마다 View를 풀에서 꺼내 직전 틱과 현재 틱 사이로 보간해 놓고(새로 꺼낸 View는 꼬리를 지우고), 소멸했거나 슬롯이 재사용된 투사체의 View는 풀에 반납한다.
        /// </summary>
        /// <param name="projectiles">표시할 투사체 저장소.</param>
        /// <param name="interpolation">직전 틱에서 현재 틱까지 진행한 비율(0~1).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(ProjectileStorage projectiles, float interpolation, ViewContext context)
        {
            ProjectileEntity[] entities = projectiles.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref ProjectileEntity projectile = ref entities[index];
                GameObject view = context.projectileViews[index];

                if (view != null && (!projectile.isAlive || context.projectileViewGenerations[index] != projectile.generation))
                {
                    context.projectilePools[context.projectileViewPoolIndices[index]].Release(view);
                    context.projectileViews[index] = null;
                    view = null;
                }

                if (!projectile.isAlive)
                {
                    continue;
                }

                bool isNewView = view == null;
                if (isNewView)
                {
                    ObjectPool<GameObject> pool = GetPool(projectile.weaponIndex, context);
                    if (pool == null)
                    {
                        continue;
                    }

                    view = pool.Get();
                    context.projectileViews[index] = view;
                    context.projectileViewGenerations[index] = projectile.generation;
                    context.projectileViewPoolIndices[index] = projectile.weaponIndex;
                }

                view.transform.position = Vector3.Lerp(projectile.previousPosition, projectile.currentPosition, interpolation);
                if (projectile.velocity.sqrMagnitude > 0f)
                {
                    view.transform.rotation = Quaternion.LookRotation(projectile.velocity, Vector3.up);
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

        private static ObjectPool<GameObject> GetPool(int weaponIndex, ViewContext context)
        {
            if (weaponIndex < 0 || weaponIndex >= context.projectilePools.Length)
            {
                return null;
            }

            return context.projectilePools[weaponIndex];
        }
    }
}
