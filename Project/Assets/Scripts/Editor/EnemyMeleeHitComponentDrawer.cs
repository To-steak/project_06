using Project06.Components;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 인스펙터에서 EnemyMeleeHitComponent를 그린다. 원이면 캡슐 전용인 길이를 숨긴다. 숨긴 값도 에셋에는 남는다.
    [CustomPropertyDrawer(typeof(EnemyMeleeHitComponent))]
    public class EnemyMeleeHitComponentDrawer : PropertyDrawer
    {
        // 모양과 상관없이 그리는 필드
        private static readonly string[] COMMON_FIELD_NAMES =
        {
            nameof(EnemyMeleeHitComponent.shape),
            nameof(EnemyMeleeHitComponent.offset),
            nameof(EnemyMeleeHitComponent.radius),
        };
        // 캡슐일 때만 그리는 필드
        private const string LENGTH_FIELD_NAME = nameof(EnemyMeleeHitComponent.length);
        // 공통 필드 뒤에 그리는 필드
        private const string ACTIVE_TICKS_FIELD_NAME = nameof(EnemyMeleeHitComponent.activeTicks);

        /// <summary>
        /// 접기 화살표와 이름을 그리고, 펼쳐져 있으면 공통 필드와 캡슐일 때의 길이, 타격 구간을 그린다.
        /// </summary>
        /// <param name="position">그릴 영역.</param>
        /// <param name="property">그릴 EnemyMeleeHitComponent 속성.</param>
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
                    y = DrawLine(position, y, property.FindPropertyRelative(COMMON_FIELD_NAMES[fieldIndex]));
                }

                if (IsCapsule(property))
                {
                    y = DrawLine(position, y, property.FindPropertyRelative(LENGTH_FIELD_NAME));
                }

                DrawLine(position, y, property.FindPropertyRelative(ACTIVE_TICKS_FIELD_NAME));
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 접혀 있으면 한 줄, 펼쳐져 있으면 그리는 필드 수만큼의 높이를 구한다. 원이면 길이 줄을 빼고 센다.
        /// </summary>
        /// <param name="property">그릴 EnemyMeleeHitComponent 속성.</param>
        /// <param name="label">인스펙터가 넘긴 이름.</param>
        /// <returns>인스펙터에서 차지할 높이(픽셀).</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            // 접기 줄 + 공통 필드 + 타격 구간, 캡슐이면 길이 한 줄 더
            int lineCount = 1 + COMMON_FIELD_NAMES.Length + 1 + (IsCapsule(property) ? 1 : 0);
            return lineCount * EditorGUIUtility.singleLineHeight + (lineCount - 1) * EditorGUIUtility.standardVerticalSpacing;
        }

        // y 위치에 한 줄짜리 필드를 그리고 다음 줄의 y 위치를 돌려준다.
        private static float DrawLine(Rect position, float y, SerializedProperty field)
        {
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), field);
            return y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private static bool IsCapsule(SerializedProperty property)
        {
            SerializedProperty shape = property.FindPropertyRelative(nameof(EnemyMeleeHitComponent.shape));
            return shape.enumValueIndex == (int)EnemyMeleeHitShape.Capsule;
        }
    }
}
