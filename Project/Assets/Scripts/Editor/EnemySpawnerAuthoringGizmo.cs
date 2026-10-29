using Project06.Authoring;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 씬 뷰에서 EnemySpawnerAuthoring 위치에 구체와 생성 반지름, 바라보는 방향을 그린다.
    public static class EnemySpawnerAuthoringGizmo
    {
        private static readonly Color CENTER_COLOR = new Color(0.2f, 0.4f, 1f, 1f);
        private static readonly Color SPAWN_RADIUS_COLOR = new Color(0.2f, 0.8f, 1f, 1f);
        private const float CENTER_RADIUS = 0.3f;

        // 생성 판정은 높이를 보지 않으므로 발밑 높이의 XZ 평면에 원을 그린다. 선택하면 반지름 값도 표시한다.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawRadii(EnemySpawnerAuthoring spawnerAuthoring, GizmoType gizmoType)
        {
            Vector3 position = spawnerAuthoring.transform.position;
            Gizmos.color = CENTER_COLOR;
            Gizmos.DrawSphere(position, CENTER_RADIUS);

            float spawnRadius = spawnerAuthoring.spawner.spawnRadius;
            Handles.color = SPAWN_RADIUS_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, spawnRadius);
            Handles.DrawLine(position, position + spawnerAuthoring.transform.forward);

            if ((gizmoType & GizmoType.Selected) != 0)
            {
                Handles.Label(position, $"생성 반지름 {spawnRadius}m");
            }
        }
    }
}
