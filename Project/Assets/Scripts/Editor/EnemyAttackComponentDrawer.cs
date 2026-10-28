using Project06.Components;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 인스펙터에서 EnemyAttackComponent를 그린다. 근접이면 판정 모양만, 원거리면 투사체 수치만 보인다. 숨긴 값도 에셋에는 남는다.
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
        // 근접 공격일 때만 그리는 필드
        private const string MELEE_HIT_FIELD_NAME = nameof(EnemyAttackComponent.meleeHit);
        // 원거리 공격일 때만 그리는 필드
        private const string PROJECTILE_FIELD_NAME = nameof(EnemyAttackComponent.projectile);

        /// <summary>
        /// 접기 화살표와 이름을 그리고, 펼쳐져 있으면 공통 필드와 종류에 맞는 판정 모양 또는 투사체 수치를 그린다.
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

                DrawField(position, y, property.FindPropertyRelative(GetKindFieldName(property)));

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 접혀 있으면 한 줄, 펼쳐져 있으면 그리는 필드들의 높이 합을 구한다. 종류에 맞지 않는 필드의 높이는 빼고 센다.
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

            height += GetFieldHeight(property.FindPropertyRelative(GetKindFieldName(property)));
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

        // 공격 종류에 맞는 전용 필드 이름을 돌려준다.
        private static string GetKindFieldName(SerializedProperty property)
        {
            SerializedProperty kind = property.FindPropertyRelative(nameof(EnemyAttackComponent.kind));
            return kind.enumValueIndex == (int)EnemyAttackKind.Ranged ? PROJECTILE_FIELD_NAME : MELEE_HIT_FIELD_NAME;
        }
    }
}
