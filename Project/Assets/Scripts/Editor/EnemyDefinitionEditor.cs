using Project06.Components;
using Project06.Definitions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Project06.Editor
{
    // 인스펙터에서 EnemyDefinition을 그린다. enemy.attacks와 attackTriggerNames를 +/- 버튼과 끌어 옮기기가 되는 하나의 공격 목록으로 합쳐 보여 주고,
    // 추가·삭제·순서 변경을 두 배열에 함께 적용해 길이와 순서를 항상 같게 맞춘다. 나머지 필드는 기본 모양으로 그린다.
    [CustomEditor(typeof(EnemyDefinition))]
    public class EnemyDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty _attacks;
        private SerializedProperty _attackTriggerNames;
        private ReorderableList _attackList;

        private const string SCRIPT_PROPERTY_NAME = "m_Script";
        private const string ENEMY_PROPERTY_NAME = nameof(EnemyDefinition.enemy);
        private const string ATTACKS_PROPERTY_NAME = nameof(EnemyComponent.attacks);
        private const string ATTACK_TRIGGER_NAMES_PROPERTY_NAME = nameof(EnemyDefinition.attackTriggerNames);
        // 목록 항목 안의 접기 화살표가 끌기 손잡이와 겹치지 않도록 띄우는 폭(픽셀)
        private const float ELEMENT_FOLDOUT_OFFSET = 12f;

        /// <summary>
        /// EnemyDefinition의 필드를 순서대로 그리되, enemy 안의 attacks는 빼고 enemy 바로 아래에 Trigger 이름과 합친 공격 목록으로 그린다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _attackTriggerNames.arraySize = _attacks.arraySize;

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == SCRIPT_PROPERTY_NAME)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(iterator);
                    }
                }
                else if (iterator.name == ENEMY_PROPERTY_NAME)
                {
                    DrawEnemyWithoutAttacks(iterator.Copy());
                    _attackList.DoLayoutList();
                }
                else if (iterator.name != ATTACK_TRIGGER_NAMES_PROPERTY_NAME)
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _attacks = serializedObject.FindProperty(ENEMY_PROPERTY_NAME).FindPropertyRelative(ATTACKS_PROPERTY_NAME);
            _attackTriggerNames = serializedObject.FindProperty(ATTACK_TRIGGER_NAMES_PROPERTY_NAME);
            _attackList = new ReorderableList(serializedObject, _attacks, true, true, true, true)
            {
                drawHeaderCallback = DrawAttackListHeader,
                drawElementCallback = DrawAttack,
                elementHeightCallback = GetAttackHeight,
                onAddCallback = AddAttack,
                onRemoveCallback = RemoveAttack,
                onReorderCallbackWithDetails = ReorderAttackTriggerName,
            };
        }

        // enemy를 접기 항목으로 그리고, 펼쳐져 있으면 attacks를 뺀 하위 필드를 그린다.
        private static void DrawEnemyWithoutAttacks(SerializedProperty enemy)
        {
            enemy.isExpanded = EditorGUILayout.Foldout(enemy.isExpanded, enemy.displayName, true);
            if (!enemy.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            SerializedProperty child = enemy.Copy();
            SerializedProperty end = enemy.GetEndProperty();
            bool enterChildren = true;
            while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                if (child.name != ATTACKS_PROPERTY_NAME)
                {
                    EditorGUILayout.PropertyField(child, true);
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawAttackListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, _attacks.displayName);
        }

        // 공격 수치(EnemyAttackComponentDrawer)를 그리고, 펼쳐져 있으면 그 아래에 Trigger 이름을 그린다.
        private void DrawAttack(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.x += ELEMENT_FOLDOUT_OFFSET;
            rect.width -= ELEMENT_FOLDOUT_OFFSET;
            rect.y += EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty attack = _attacks.GetArrayElementAtIndex(index);
            float attackHeight = EditorGUI.GetPropertyHeight(attack, true);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, attackHeight), attack, new GUIContent($"Attack {index}"), true);
            if (!attack.isExpanded)
            {
                return;
            }

            Rect triggerRect = new Rect(rect.x, rect.y + attackHeight + EditorGUIUtility.standardVerticalSpacing, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(triggerRect, _attackTriggerNames.GetArrayElementAtIndex(index), new GUIContent("Animation Trigger"));
            EditorGUI.indentLevel--;
        }

        private float GetAttackHeight(int index)
        {
            SerializedProperty attack = _attacks.GetArrayElementAtIndex(index);
            float height = EditorGUI.GetPropertyHeight(attack, true) + EditorGUIUtility.standardVerticalSpacing * 2f;
            if (attack.isExpanded)
            {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }

        // 두 배열 끝에 항목을 하나씩 늘리고 새 항목을 선택한다. 새 공격은 직전 마지막 공격의 값을 복사해 시작한다.
        private void AddAttack(ReorderableList list)
        {
            _attacks.arraySize++;
            _attackTriggerNames.arraySize = _attacks.arraySize;
            list.index = _attacks.arraySize - 1;
        }

        // 선택한 항목을(없으면 마지막 항목을) 두 배열에서 함께 지운다.
        private void RemoveAttack(ReorderableList list)
        {
            int index = list.index >= 0 && list.index < _attacks.arraySize ? list.index : _attacks.arraySize - 1;
            if (index < 0)
            {
                return;
            }

            _attacks.DeleteArrayElementAtIndex(index);
            _attackTriggerNames.DeleteArrayElementAtIndex(index);
            list.index = Mathf.Min(index, _attacks.arraySize - 1);
        }

        // 목록이 attacks의 순서를 옮긴 뒤 불린다. Trigger 이름도 같은 자리로 옮긴다.
        private void ReorderAttackTriggerName(ReorderableList list, int oldIndex, int newIndex)
        {
            _attackTriggerNames.MoveArrayElement(oldIndex, newIndex);
        }
    }
}
