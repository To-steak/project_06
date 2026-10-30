using Project06.Components;
using UnityEditor;
using UnityEngine;

namespace Project06.Editor
{
    // 인스펙터에서 WeaponComponent를 그린다. 원거리면 연사·투사체·탄창 수치만, 근접이면 근접 수치만 보인다. 숨긴 값도 에셋에는 남는다.
    [CustomPropertyDrawer(typeof(WeaponComponent))]
    public class WeaponComponentDrawer : PropertyDrawer
    {
        // 원거리 무기일 때만 그리는 필드
        private static readonly string[] RANGED_FIELD_NAMES =
        {
            nameof(WeaponComponent.fireIntervalTicks),
            nameof(WeaponComponent.projectileSpeed),
            nameof(WeaponComponent.projectileRange),
            nameof(WeaponComponent.damage),
            nameof(WeaponComponent.magazineCapacity),
            nameof(WeaponComponent.reloadTicks),
        };
        // 근접 무기일 때만 그리는 필드
        private static readonly string[] MELEE_FIELD_NAMES =
        {
            nameof(WeaponComponent.melee),
        };
        // 종류와 상관없이 그리는 필드
        private const string KIND_FIELD_NAME = nameof(WeaponComponent.kind);

        /// <summary>
        /// 접기 화살표와 이름을 그리고, 펼쳐져 있으면 종류와 종류에 맞는 필드를 그린다.
        /// </summary>
        /// <param name="position">그릴 영역.</param>
        /// <param name="property">그릴 WeaponComponent 속성.</param>
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
                y = DrawField(position, y, property.FindPropertyRelative(KIND_FIELD_NAME));
                string[] fieldNames = GetKindFieldNames(property);
                for (int fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
                {
                    y = DrawField(position, y, property.FindPropertyRelative(fieldNames[fieldIndex]));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 접혀 있으면 한 줄, 펼쳐져 있으면 그리는 필드들의 높이 합을 구한다. 종류에 맞지 않는 필드의 높이는 빼고 센다.
        /// </summary>
        /// <param name="property">그릴 WeaponComponent 속성.</param>
        /// <param name="label">인스펙터가 넘긴 이름.</param>
        /// <returns>인스펙터에서 차지할 높이(픽셀).</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return height;
            }

            height += GetFieldHeight(property.FindPropertyRelative(KIND_FIELD_NAME));
            string[] fieldNames = GetKindFieldNames(property);
            for (int fieldIndex = 0; fieldIndex < fieldNames.Length; fieldIndex++)
            {
                height += GetFieldHeight(property.FindPropertyRelative(fieldNames[fieldIndex]));
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

        // 무기 종류에 맞는 전용 필드 이름 목록을 돌려준다.
        private static string[] GetKindFieldNames(SerializedProperty property)
        {
            SerializedProperty kind = property.FindPropertyRelative(KIND_FIELD_NAME);
            return kind.enumValueIndex == (int)WeaponKind.Melee ? MELEE_FIELD_NAMES : RANGED_FIELD_NAMES;
        }
    }
}
