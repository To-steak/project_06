using Project06.Authoring;
using Project06.Definitions;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 씬 뷰에서 ShelterAuthoring 위치에 그 Authoring이 가리키는 ShelterDefinition의 피격 반지름과 정신력 안전 반지름을 원으로 그린다.
    public static class ShelterAuthoringGizmo
    {
        private static readonly Color HIT_RADIUS_COLOR = new Color(1f, 1f, 1f, 1f);
        private static readonly Color MENTALITY_SAFE_RADIUS_COLOR = new Color(0.4f, 1f, 1f, 1f);

        // 판정은 높이를 보지 않으므로 발밑 높이의 XZ 평면에 원을 그린다. 선택하면 반지름 값도 표시한다.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawRadii(ShelterAuthoring shelterAuthoring, GizmoType gizmoType)
        {
            ShelterDefinition definition = shelterAuthoring.definition;
            if (definition == null)
            {
                return;
            }

            float hitRadius = definition.shelter.hitRadius;
            float mentalitySafeRadius = definition.shelter.mentalitySafeRadius;
            Vector3 position = shelterAuthoring.transform.position;
            Handles.color = HIT_RADIUS_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, hitRadius);
            Handles.color = MENTALITY_SAFE_RADIUS_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, mentalitySafeRadius);

            if ((gizmoType & GizmoType.Selected) != 0)
            {
                Handles.Label(position, $"피격 반지름 {hitRadius}m\n정신력 안전 반지름 {mentalitySafeRadius}m");
            }
        }
    }
}
