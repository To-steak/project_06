using Project06.Authoring;
using Project06.Definitions;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 씬 뷰에서 EnemyAuthoring 위치에 그 Authoring이 가리키는 EnemyDefinition의 피격 반지름, 감지 반지름, 추격 해제 반지름, 정지 거리를 원으로 그린다.
    public static class EnemyAuthoringGizmo
    {
        private static readonly Color HIT_RADIUS_COLOR = new Color(1f, 0.3f, 0.2f, 1f);
        private static readonly Color DETECTION_RADIUS_COLOR = new Color(1f, 0.9f, 0.2f, 1f);
        private static readonly Color LOSE_RADIUS_COLOR = new Color(0.7f, 0.4f, 1f, 1f);
        private static readonly Color STOP_DISTANCE_COLOR = new Color(0.3f, 1f, 0.4f, 1f);

        // 판정은 높이를 보지 않으므로 발밑 높이의 XZ 평면에 원을 그린다. 선택하면 반지름 값도 표시한다.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawRadii(EnemyAuthoring enemyAuthoring, GizmoType gizmoType)
        {
            EnemyDefinition definition = enemyAuthoring.definition;
            if (definition == null)
            {
                return;
            }

            float hitRadius = definition.enemy.hitRadius;
            float detectionRadius = definition.enemy.detectionRadius;
            float loseRadius = definition.enemy.loseRadius;
            float stopDistance = definition.enemy.stopDistance;
            Vector3 position = enemyAuthoring.transform.position;
            Handles.color = HIT_RADIUS_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, hitRadius);
            Handles.color = DETECTION_RADIUS_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, detectionRadius);
            Handles.color = LOSE_RADIUS_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, loseRadius);
            Handles.color = STOP_DISTANCE_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, stopDistance);

            if ((gizmoType & GizmoType.Selected) != 0)
            {
                Handles.Label(position, $"피격 반지름 {hitRadius}m\n감지 반지름 {detectionRadius}m\n해제 반지름 {loseRadius}m\n정지 거리 {stopDistance}m");
            }
        }
    }
}
