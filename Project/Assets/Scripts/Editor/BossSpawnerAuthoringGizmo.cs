using Project06.Authoring;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 씬 뷰에서 BossSpawnerAuthoring 위치에 구체와 바라보는 방향을 그린다.
    public static class BossSpawnerAuthoringGizmo
    {
        private static readonly Color CENTER_COLOR = new Color(0.2f, 0.4f, 1f, 1f);
        private static readonly Color DIRECTION_COLOR = new Color(1f, 0.2f, 0.8f, 1f);
        private const float CENTER_RADIUS = 0.3f;

        // 보스가 생길 위치에 파란 구체를 그린다. 선택하면 라벨도 표시한다.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawCenter(BossSpawnerAuthoring bossSpawnerAuthoring, GizmoType gizmoType)
        {
            Vector3 position = bossSpawnerAuthoring.transform.position;
            Gizmos.color = CENTER_COLOR;
            Gizmos.DrawSphere(position, CENTER_RADIUS);
            Handles.color = DIRECTION_COLOR;
            Handles.DrawLine(position, position + bossSpawnerAuthoring.transform.forward);

            if ((gizmoType & GizmoType.Selected) != 0)
            {
                Handles.Label(position, "보스 · 마지막 날 밤");
            }
        }
    }
}
