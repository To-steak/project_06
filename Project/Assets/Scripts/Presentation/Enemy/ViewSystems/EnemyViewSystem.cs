using Project06.Entities;
using Project06.Presentation.Enemy.Views;
using UnityEngine;
using UnityEngine.Pool;

namespace Project06.Presentation.Enemy.ViewSystems
{
    public static class EnemyViewSystem
    {
        private static readonly int SPEED_PARAMETER_HASH = Animator.StringToHash(SPEED_PARAMETER);

        // 종류별 풀이 처음 확보하는 View 수
        private const int DEFAULT_POOL_CAPACITY = 16;
        // 이동 속력(Float). 0은 정지, 1은 걷기 속도, 2는 달리기 속도가 되도록 정규화한다.
        private const string SPEED_PARAMETER = "Speed";

        /// <summary>
        /// 종류별 적 View 풀과 슬롯별 View 매핑을 만든다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="enemyPrefabs">종류별 적 프리팹. 적 종류 인덱스와 같은 순서이며 null이 섞여 있어도 된다. 루트에 EnemyView가 있어야 한다.</param>
        /// <param name="capacity">적 저장소의 슬롯 수.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(GameObject[] enemyPrefabs, int capacity, ViewContext context)
        {
            context.enemyPools = new ObjectPool<EnemyView>[enemyPrefabs.Length];
            for (int enemyTypeIndex = 0; enemyTypeIndex < enemyPrefabs.Length; enemyTypeIndex++)
            {
                GameObject prefab = enemyPrefabs[enemyTypeIndex];
                if (prefab == null)
                {
                    continue;
                }

                EnemyView prefabView = prefab.GetComponent<EnemyView>();
                if (prefabView == null)
                {
                    Debug.LogError($"적 프리팹 '{prefab.name}'의 루트에 EnemyView가 없다. 이 종류의 적은 표시하지 않는다.", prefab);
                    continue;
                }

                context.enemyPools[enemyTypeIndex] = new ObjectPool<EnemyView>(
                    () => Object.Instantiate(prefabView),
                    view => view.gameObject.SetActive(true),
                    view => view.gameObject.SetActive(false),
                    view => Object.Destroy(view.gameObject),
                    false,
                    DEFAULT_POOL_CAPACITY,
                    capacity);
            }

            context.enemyViews = new EnemyView[capacity];
            context.enemyViewGenerations = new int[capacity];
            context.enemyViewPoolIndices = new int[capacity];
        }

        /// <summary>
        /// 살아 있는 적마다 View를 풀에서 꺼내 위치와 방향을 맞추고 이동 속력을 Animator에 넘긴다. 소멸했거나 슬롯이 재사용된 적의 View는 풀에 반납한다.
        /// </summary>
        /// <param name="enemies">표시할 적 저장소.</param>
        /// <param name="interpolation">직전 틱에서 현재 틱까지 진행한 비율(0~1).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(EnemyStorage enemies, float interpolation, ViewContext context)
        {
            EnemyEntity[] entities = enemies.entities;
            for (int index = 0; index < entities.Length; index++)
            {
                ref EnemyEntity enemy = ref entities[index];
                EnemyView view = context.enemyViews[index];

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
                    ObjectPool<EnemyView> pool = GetPool(enemy.enemyTypeIndex, context);
                    if (pool == null)
                    {
                        continue;
                    }

                    view = pool.Get();
                    context.enemyViews[index] = view;
                    context.enemyViewGenerations[index] = enemy.generation;
                    context.enemyViewPoolIndices[index] = enemy.enemyTypeIndex;
                }

                view.transform.position = Vector3.Lerp(enemy.previousPosition, enemy.currentPosition, interpolation);
                if (enemy.facingDirection.sqrMagnitude > 0f)
                {
                    view.transform.rotation = Quaternion.LookRotation(enemy.facingDirection, Vector3.up);
                }

                if (view.animator != null)
                {
                    view.animator.SetFloat(SPEED_PARAMETER_HASH, GetBlendSpeed(in enemy));
                }
            }
        }

        private static ObjectPool<EnemyView> GetPool(int enemyTypeIndex, ViewContext context)
        {
            if (enemyTypeIndex < 0 || enemyTypeIndex >= context.enemyPools.Length)
            {
                return null;
            }

            return context.enemyPools[enemyTypeIndex];
        }

        // 속력을 걷기 속도에서 1, 달리기 속도에서 2인 값으로 바꾼다.
        private static float GetBlendSpeed(in EnemyEntity enemy)
        {
            float speed = enemy.velocity.magnitude;
            float walkSpeed = enemy.enemy.walkSpeed;
            float runSpeed = enemy.enemy.runSpeed;
            return speed <= walkSpeed
                ? Mathf.InverseLerp(0f, walkSpeed, speed)
                : 1f + Mathf.InverseLerp(walkSpeed, runSpeed, speed);
        }
    }
}
