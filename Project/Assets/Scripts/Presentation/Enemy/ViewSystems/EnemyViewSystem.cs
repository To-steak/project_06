using Project06.Entities;
using UnityEngine;
using UnityEngine.Pool;

namespace Project06.Presentation.Enemy.ViewSystems
{
    public static class EnemyViewSystem
    {
        // 종류별 풀이 처음 확보하는 View 수
        private const int DEFAULT_POOL_CAPACITY = 16;

        /// <summary>
        /// 종류별 적 View 풀과 슬롯별 View 매핑을 만든다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="enemyPrefabs">종류별 적 프리팹. 적 종류 인덱스와 같은 순서이며 null이 섞여 있어도 된다.</param>
        /// <param name="capacity">적 저장소의 슬롯 수.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(GameObject[] enemyPrefabs, int capacity, ViewContext context)
        {
            context.enemyPools = new ObjectPool<GameObject>[enemyPrefabs.Length];
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyPrefabs.Length; enemyTypeIndex++)
            {
                GameObject prefab = enemyPrefabs[enemyTypeIndex];
                if (prefab == null)
                {
                    continue;
                }

                context.enemyPools[enemyTypeIndex] = new ObjectPool<GameObject>(
                    () => Object.Instantiate(prefab),
                    view => view.SetActive(true),
                    view => view.SetActive(false),
                    view => Object.Destroy(view),
                    false,
                    DEFAULT_POOL_CAPACITY,
                    capacity);
            }

            context.enemyViews = new GameObject[capacity];
            context.enemyViewGenerations = new int[capacity];
            context.enemyViewPoolIndices = new int[capacity];
        }

        /// <summary>
        /// 살아 있는 적마다 View를 풀에서 꺼내 위치와 방향을 맞추고, 소멸했거나 슬롯이 재사용된 적의 View는 풀에 반납한다.
        /// </summary>
        /// <param name="enemies">표시할 적 저장소.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(EnemyStorage enemies, ViewContext context)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                GameObject view = context.enemyViews[index];

                if (view != null && (!enemy.isAlive || context.enemyViewGenerations[index] != enemy.generation))
                {
                    context.enemyPools[context.enemyViewPoolIndices[index]].Release(view);
                    context.enemyViews[index] = null;
                    view = null;
                }

                if (!enemy.isAlive)
                {
                    continue;
                }

                if (view == null)
                {
                    ObjectPool<GameObject> pool = GetPool(enemy.enemyTypeIndex, context);
                    if (pool == null)
                    {
                        continue;
                    }

                    view = pool.Get();
                    context.enemyViews[index] = view;
                    context.enemyViewGenerations[index] = enemy.generation;
                    context.enemyViewPoolIndices[index] = enemy.enemyTypeIndex;
                }

                view.transform.position = enemy.position;
                if (enemy.facingDirection.sqrMagnitude > 0f)
                {
                    view.transform.rotation = Quaternion.LookRotation(enemy.facingDirection, Vector3.up);
                }
            }
        }

        private static ObjectPool<GameObject> GetPool(int enemyTypeIndex, ViewContext context)
        {
            if (enemyTypeIndex < 0 || enemyTypeIndex >= context.enemyPools.Length)
            {
                return null;
            }

            return context.enemyPools[enemyTypeIndex];
        }
    }
}
