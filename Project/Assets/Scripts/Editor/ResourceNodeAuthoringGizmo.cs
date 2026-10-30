using Project06.Authoring;
using Project06.Definitions;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 씬 뷰에서 ResourceNodeAuthoring 위치에 그 Authoring이 가리키는 ResourceNodeDefinition의 피격 반지름을 원으로 그린다.
    public static class ResourceNodeAuthoringGizmo
    {
        private static readonly Color HIT_RADIUS_COLOR = new Color(1f, 1f, 1f, 1f);

        // 판정은 높이를 보지 않으므로 발밑 높이의 XZ 평면에 원을 그린다. 선택하면 종류, 체력, 자원량도 표시한다.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawRadius(ResourceNodeAuthoring resourceNodeAuthoring, GizmoType gizmoType)
        {
            ResourceNodeDefinition definition = resourceNodeAuthoring.definition;
            if (definition == null)
            {
                return;
            }

            Vector3 position = resourceNodeAuthoring.transform.position;
            Handles.color = HIT_RADIUS_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, definition.node.hitRadius);

            if ((gizmoType & GizmoType.Selected) != 0)
            {
                Handles.Label(position, $"{definition.node.kind}\n피격 반지름 {definition.node.hitRadius}m\n체력 {definition.node.maxHealth}\n몫당 자원 {definition.node.resourcePerShare}");
            }
        }
    }
}
