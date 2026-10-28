using Project06.Components;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 인스펙터에서 EnemyAttackComponent를 그린다. 근접 공격이면 원거리 전용인 투사체 수치를 숨긴다. 숨긴 값도 에셋에는 남는다.
    [CustomPropertyDrawer(typeof(EnemyAttackComponent))]
    public class EnemyAttackComponentDrawer : PropertyDrawer
    {
        // 종류와 상관없이 그리는 필드
        private static readonly string[] COMMON_FIELD_NAMES =
        {
            nameof(EnemyAttackComponent.kind),
            nameof(EnemyAttackComponent.range),
            nameof(EnemyAttackComponent.damage),
            nameof(EnemyAttackComponent.windupTicks),
            nameof(EnemyAttackComponent.recoveryTicks),
            nameof(EnemyAttackComponent.cooldownTicks),
        };
        // 원거리 공격일 때만 그리는 필드
        private const string PROJECTILE_FIELD_NAME = nameof(EnemyAttackComponent.projectile);

        /// <summary>
        /// 접기 화살표와 이름을 그리고, 펼쳐져 있으면 공통 필드와 원거리일 때의 투사체 수치를 그린다.
        /// </summary>
        /// <param name="position">그릴 영역.</param>
        /// <param name="property">그릴 EnemyAttackComponent 속성.</param>
        /// <param name="label">인스펙터가 넘긴 이름.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(lineRect, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = lineRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                for (int fieldIndex = 0; fieldIndex < COMMON_FIELD_NAMES.Length; fieldIndex++)
                {
                    y = DrawField(position, y, property.FindPropertyRelative(COMMON_FIELD_NAMES[fieldIndex]));
                }

                if (IsRanged(property))
                {
                    DrawField(position, y, property.FindPropertyRelative(PROJECTILE_FIELD_NAME));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 접혀 있으면 한 줄, 펼쳐져 있으면 그리는 필드들의 높이 합을 구한다. 근접이면 투사체 수치의 높이를 빼고 센다.
        /// </summary>
        /// <param name="property">그릴 EnemyAttackComponent 속성.</param>
        /// <param name="label">인스펙터가 넘긴 이름.</param>
        /// <returns>인스펙터에서 차지할 높이(픽셀).</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return height;
            }

            for (int fieldIndex = 0; fieldIndex < COMMON_FIELD_NAMES.Length; fieldIndex++)
            {
                height += GetFieldHeight(property.FindPropertyRelative(COMMON_FIELD_NAMES[fieldIndex]));
            }

            if (IsRanged(property))
            {
                height += GetFieldHeight(property.FindPropertyRelative(PROJECTILE_FIELD_NAME));
            }

            return height;
        }

        // y 위치에 필드를 그리고 다음 필드의 y 위치를 돌려준다.
        private static float DrawField(Rect position, float y, SerializedProperty field)
        {
            float fieldHeight = EditorGUI.GetPropertyHeight(field, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, fieldHeight), field, true);
            return y + fieldHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private static float GetFieldHeight(SerializedProperty field)
        {
            return EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        private static bool IsRanged(SerializedProperty property)
        {
            SerializedProperty kind = property.FindPropertyRelative(nameof(EnemyAttackComponent.kind));
            return kind.enumValueIndex == (int)EnemyAttackKind.Ranged;
        }
    }
}
