using System.Collections.Generic;
using Project06.Components;
using Project06.Entities;
using Project06.Presentation.Enemy.Views;
using Project06.Records;
using UnityEngine;
using UnityEngine.Pool;

namespace Project06.Presentation.Enemy.ViewSystems
{
    public static class EnemyViewSystem
    {
        private static readonly int SPEED_PARAMETER_HASH = Animator.StringToHash(SPEED_PARAMETER);
        private static readonly int IS_DEAD_PARAMETER_HASH = Animator.StringToHash(IS_DEAD_PARAMETER);
        private static readonly int GET_HIT_PARAMETER_HASH = Animator.StringToHash(GET_HIT_PARAMETER);

        // 종류별 풀이 처음 확보하는 View 수
        private const int DEFAULT_POOL_CAPACITY = 16;
        // 이동 속력(Float). 0은 정지, 1은 걷기 속도, 2는 달리기 속도가 되도록 정규화한다.
        private const string SPEED_PARAMETER = "Speed";
        // 사망했는지 여부(Bool). 체력이 0 이하가 되면 true가 되고, 사망 시간이 끝나 소멸할 때까지 유지된다.
        private const string IS_DEAD_PARAMETER = "IsDead";
        // 경직에 걸린 프레임에 켜는 Trigger
        private const string GET_HIT_PARAMETER = "GetHit";
        // 공격 Trigger 이름이 비어 있음을 나타내는 해시
        private const int NO_TRIGGER_HASH = 0;

        /// <summary>
        /// 종류별 적 View 풀, 공격 Trigger 해시, 슬롯별 View 매핑을 만든다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="enemyPrefabs">종류별 적 프리팹. 적 종류 인덱스와 같은 순서이며 null이 섞여 있어도 된다. 루트에 EnemyView가 있어야 한다.</param>
        /// <param name="attackTriggerNames">종류별 공격 Trigger 이름 목록. 적 종류 인덱스와 같은 순서이며, 안쪽은 공격 목록과 같은 순서다. null이나 빈 이름이 섞여 있어도 된다.</param>
        /// <param name="capacity">적 저장소의 슬롯 수.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(GameObject[] enemyPrefabs, string[][] attackTriggerNames, int capacity, ViewContext context)
        {
            context.enemyAttackTriggerHashes = new int[attackTriggerNames.Length][];
            for (int enemyTypeIndex = 0; enemyTypeIndex < attackTriggerNames.Length; enemyTypeIndex++)
            {
                string[] triggerNames = attackTriggerNames[enemyTypeIndex] ?? new string[0];
                int[] triggerHashes = new int[triggerNames.Length];
                for (int attackIndex = 0; attackIndex < triggerNames.Length; attackIndex++)
                {
                    triggerHashes[attackIndex] = string.IsNullOrEmpty(triggerNames[attackIndex]) ? NO_TRIGGER_HASH : Animator.StringToHash(triggerNames[attackIndex]);
                }

                context.enemyAttackTriggerHashes[enemyTypeIndex] = triggerHashes;
            }

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
        /// 살아 있는 적마다 View를 풀에서 꺼내 위치와 방향을 맞추고 이동 속력과 사망 여부를 Animator에 넘긴다. 소멸했거나 슬롯이 재사용된 적의 View는 풀에 반납한다.
        /// 이번 프레임에 공격을 시작한 기록이 있으면 그 적의 공격 애니메이션을, 경직에 걸린 기록이 있으면 피격 애니메이션을 한 번 재생한다.
        /// </summary>
        /// <param name="enemies">표시할 적 저장소.</param>
        /// <param name="enemyAttackStartedRecords">이번 프레임에 쌓인 적 공격 시작 기록.</param>
        /// <param name="enemyHitStunnedRecords">이번 프레임에 쌓인 적 경직 기록.</param>
        /// <param name="interpolation">직전 틱에서 현재 틱까지 진행한 비율(0~1).</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(EnemyStorage enemies, List<EnemyAttackStartedRecord> enemyAttackStartedRecords, List<EnemyHitStunnedRecord> enemyHitStunnedRecords, float interpolation, ViewContext context)
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
                    view.animator.SetBool(IS_DEAD_PARAMETER_HASH, enemy.state == EnemyState.Dead);
                }
            }

            PresentAttackStarts(enemyAttackStartedRecords, context);
            PresentHitStuns(enemyHitStunnedRecords, context);
        }

        // 경직에 걸린 적의 View가 아직 그 적을 표시하고 있으면 피격 Trigger를 켠다. 같은 프레임에 공격 Trigger가 켜졌다면 끈다.
        private static void PresentHitStuns(List<EnemyHitStunnedRecord> enemyHitStunnedRecords, ViewContext context)
        {
            for (int recordIndex = 0; recordIndex < enemyHitStunnedRecords.Count; recordIndex++)
            {
                EnemyHitStunnedRecord record = enemyHitStunnedRecords[recordIndex];
                int index = record.enemy.index;
                EnemyView view = context.enemyViews[index];
                if (view == null || view.animator == null || context.enemyViewGenerations[index] != record.enemy.generation)
                {
                    continue;
                }

                int[] triggerHashes = context.enemyAttackTriggerHashes[context.enemyViewPoolIndices[index]];
                for (int attackIndex = 0; attackIndex < triggerHashes.Length; attackIndex++)
                {
                    if (triggerHashes[attackIndex] != NO_TRIGGER_HASH)
                    {
                        view.animator.ResetTrigger(triggerHashes[attackIndex]);
                    }
                }

                view.animator.SetTrigger(GET_HIT_PARAMETER_HASH);
            }
        }

        // 공격을 시작한 적의 View가 아직 그 적을 표시하고 있으면 그 공격의 Trigger를 켠다. Trigger 이름이 없는 공격은 건너뛴다.
        private static void PresentAttackStarts(List<EnemyAttackStartedRecord> enemyAttackStartedRecords, ViewContext context)
        {
            for (int recordIndex = 0; recordIndex < enemyAttackStartedRecords.Count; recordIndex++)
            {
                EnemyAttackStartedRecord record = enemyAttackStartedRecords[recordIndex];
                int index = record.enemy.index;
                EnemyView view = context.enemyViews[index];
                if (view == null || view.animator == null || context.enemyViewGenerations[index] != record.enemy.generation)
                {
                    continue;
                }

                int[] triggerHashes = context.enemyAttackTriggerHashes[context.enemyViewPoolIndices[index]];
                if (record.attackIndex < 0 || record.attackIndex >= triggerHashes.Length || triggerHashes[record.attackIndex] == NO_TRIGGER_HASH)
                {
                    continue;
                }

                view.animator.SetTrigger(triggerHashes[record.attackIndex]);
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
