using Project06.Entities;
using UnityEngine;

namespace Project06.Presentation.Enemy.ViewSystems
{
    public static class EnemyViewSystem
    {
        /// <summary>
        /// 적마다 View를 한 번 생성해 위치와 방향을 맞춰 둔다. 판 시작 시 한 번 호출한다.
        /// </summary>
        /// <param name="enemyPrefab">적 프리팹. null이면 View를 만들지 않는다.</param>
        /// <param name="enemies">표시할 적 목록.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Initialize(GameObject enemyPrefab, EnemyEntity[] enemies, ViewContext context)
        {
            context.enemyViews = new GameObject[enemies.Length];
            if (enemyPrefab == null)
            {
                return;
            }

            for (int index = 0; index < enemies.Length; index++)
            {
                ref EnemyEntity enemy = ref enemies[index];
                Quaternion rotation = Quaternion.LookRotation(enemy.facingDirection, Vector3.up);
                context.enemyViews[index] = Object.Instantiate(enemyPrefab, enemy.position, rotation);
            }
        }

        /// <summary>
        /// 죽은 적의 View를 끈다.
        /// </summary>
        /// <param name="enemies">표시할 적 목록.</param>
        /// <param name="context">View 실행 상태.</param>
        public static void Present(EnemyEntity[] enemies, ViewContext context)
        {
            for (int index = 0; index < enemies.Length; index++)
            {
                ref EnemyEntity enemy = ref enemies[index];
                GameObject view = context.enemyViews[index];
                if (view != null && view.activeSelf != enemy.isAlive)
                {
                    view.SetActive(enemy.isAlive);
                }
            }
        }
    }
}
