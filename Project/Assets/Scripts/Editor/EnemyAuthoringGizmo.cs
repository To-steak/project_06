using Project06.Authoring;
using Project06.Definitions;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 씬 뷰에서 EnemyAuthoring 위치에 DefinitionCatalog.enemy의 피격 반지름을 원으로 그린다.
    public static class EnemyAuthoringGizmo
    {
        private static readonly Color HIT_RADIUS_COLOR = new Color(1f, 0.3f, 0.2f, 1f);

        private const string DEFINITION_CATALOG_PATH = "Assets/Resources/Definition Catalog.asset";

        // 피격 판정은 높이를 보지 않으므로 발밑 높이의 XZ 평면에 원을 그린다. 선택하면 반지름 값도 표시한다.
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawHitRadius(EnemyAuthoring enemyAuthoring, GizmoType gizmoType)
        {
            DefinitionCatalog definitionCatalog = AssetDatabase.LoadAssetAtPath<DefinitionCatalog>(DEFINITION_CATALOG_PATH);
            if (definitionCatalog == null || definitionCatalog.enemy == null)
            {
                return;
            }

            float hitRadius = definitionCatalog.enemy.enemy.hitRadius;
            Vector3 position = enemyAuthoring.transform.position;
            Handles.color = HIT_RADIUS_COLOR;
            Handles.DrawWireDisc(position, Vector3.up, hitRadius);

            if ((gizmoType & GizmoType.Selected) != 0)
            {
                Handles.Label(position, $"피격 반지름 {hitRadius}m");
            }
        }
    }
}
