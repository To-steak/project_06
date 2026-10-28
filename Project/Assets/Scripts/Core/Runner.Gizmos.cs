#if UNITY_EDITOR
using Project06.Components;
using Project06.Entities;
using UnityEditor;
using UnityEngine;

namespace Project06.Core
{
    // 씬 뷰 디버그 기즈모. 에디터에서만 컴파일된다.
    public partial class Runner
    {
        // 적 디버그 기즈모 색
        private static readonly Color ENEMY_DETECTION_RADIUS_COLOR = new Color(1f, 0.9f, 0.2f, 1f);
        private static readonly Color ENEMY_LOSE_RADIUS_COLOR = new Color(0.7f, 0.4f, 1f, 1f);
        private static readonly Color ENEMY_STOP_DISTANCE_COLOR = new Color(0.3f, 1f, 0.4f, 1f);
        private static readonly Color ENEMY_WANDER_RADIUS_COLOR = new Color(1f, 1f, 1f, 0.4f);
        private static readonly Color ENEMY_PATH_COLOR = new Color(0.2f, 0.8f, 1f, 1f);

        // 디버그용으로 살아 있고 사망 중이 아닌 적마다 자기 위치를 중심으로 판정 원을 XZ 평면에 그리고, 남은 NavMesh 경로를 선으로 그린다.
        // 추격 중이 아니면 감지 원과 생성 위치 중심의 배회 원을, 추격 중이면 해제 원과 정지 원을 그린다.
        private void OnDrawGizmos()
        {
            if (_world == null)
            {
                return;
            }

            EnemyEntity[] enemies = _world.enemies.entities;
            for (int index = 0; index < enemies.Length; index++)
            {
                ref EnemyEntity enemy = ref enemies[index];
                if (!enemy.isAlive || enemy.state == EnemyState.Dead)
                {
                    continue;
                }

                Vector3 position = enemy.currentPosition;
                if (enemy.state == EnemyState.Chase)
                {
                    Handles.color = ENEMY_LOSE_RADIUS_COLOR;
                    Handles.DrawWireDisc(position, Vector3.up, enemy.enemy.loseRadius);
                    Handles.color = ENEMY_STOP_DISTANCE_COLOR;
                    Handles.DrawWireDisc(position, Vector3.up, enemy.enemy.stopDistance);
                }
                else
                {
                    Handles.color = ENEMY_DETECTION_RADIUS_COLOR;
                    Handles.DrawWireDisc(position, Vector3.up, enemy.enemy.detectionRadius);
                    Handles.color = ENEMY_WANDER_RADIUS_COLOR;
                    Handles.DrawWireDisc(enemy.spawnPosition, Vector3.up, enemy.enemy.wanderRadius);
                }

                Handles.color = ENEMY_PATH_COLOR;
                Vector3 lineStart = position;
                for (int cornerIndex = enemy.pathCornerIndex; cornerIndex < enemy.pathCornerCount; cornerIndex++)
                {
                    Vector3 corner = enemy.pathCorners[cornerIndex];
                    Handles.DrawLine(lineStart, corner);
                    lineStart = corner;
                }
            }
        }
    }
}
#endif
