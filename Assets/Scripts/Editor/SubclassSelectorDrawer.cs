using System;
using System.Linq;
using Architecture;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// [SubclassSelector] 어트리뷰트가 붙은 [SerializeReference] 필드를
    /// 인스펙터에서 편하게 드롭다운으로 선택할 수 있도록 해주는 커스텀 드로어입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // 필드의 기본 타입(인터페이스 또는 부모 클래스) 획득
            Type fieldType = GetFieldType(property);
            if (fieldType == null)
            {
                EditorGUI.LabelField(position, label.text, "Type not supported");
                EditorGUI.EndProperty();
                return;
            }

            // 부모 타입(인터페이스/추상클래스)을 상속받는 모든 일반 클래스(구현체) 검색
            var derivedTypes = TypeCache.GetTypesDerivedFrom(fieldType)
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericType && t.IsDefined(typeof(SerializableAttribute), false))
                .ToArray();

            // 현재 할당된 타입 (없으면 null)
            Type currentType = null;
            if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                var parts = property.managedReferenceFullTypename.Split(' ');
                if (parts.Length == 2)
                {
                    currentType = Type.GetType($"{parts[1]}, {parts[0]}");
                }
            }

            // 드롭다운에 표시할 이름 목록
            GUIContent[] typeNames = new GUIContent[derivedTypes.Length + 1];
            typeNames[0] = new GUIContent("<Null> (비우기)");
            int currentIndex = 0;

            for (int i = 0; i < derivedTypes.Length; i++)
            {
                typeNames[i + 1] = new GUIContent(derivedTypes[i].Name);
                if (currentType == derivedTypes[i])
                {
                    currentIndex = i + 1;
                }
            }

            Rect popupRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Foldout + Popup rendering
            property.isExpanded = EditorGUI.Foldout(new Rect(popupRect.x, popupRect.y, EditorGUIUtility.labelWidth, popupRect.height), property.isExpanded, label, true);
            
            Rect dropdownRect = new Rect(popupRect.x + EditorGUIUtility.labelWidth, popupRect.y, popupRect.width - EditorGUIUtility.labelWidth, popupRect.height);
            int newIndex = EditorGUI.Popup(dropdownRect, currentIndex, typeNames);

            if (newIndex != currentIndex)
            {
                property.managedReferenceValue = newIndex == 0 ? null : Activator.CreateInstance(derivedTypes[newIndex - 1]);
                property.serializedObject.ApplyModifiedProperties();
            }

            // 내부 필드(수치 입력 등) 렌더링
            if (property.isExpanded && currentType != null)
            {
                EditorGUI.indentLevel++;
                Rect propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
                
                SerializedProperty iterator = property.Copy();
                bool enterChildren = true;
                
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    // 현재 객체의 끝에 도달하면 반복 종료
                    if (SerializedProperty.EqualContents(iterator, property.GetEndProperty()))
                        break;

                    float childHeight = EditorGUI.GetPropertyHeight(iterator, true);
                    propertyRect.height = childHeight;
                    EditorGUI.PropertyField(propertyRect, iterator, true);
                    propertyRect.y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            
            if (property.isExpanded && !string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                SerializedProperty iterator = property.Copy();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (SerializedProperty.EqualContents(iterator, property.GetEndProperty()))
                        break;
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            
            return height;
        }

        private Type GetFieldType(SerializedProperty property)
        {
            var parts = property.managedReferenceFieldTypename.Split(' ');
            if (parts.Length == 2)
            {
                return Type.GetType($"{parts[1]}, {parts[0]}");
            }
            return null;
        }
    }
}
