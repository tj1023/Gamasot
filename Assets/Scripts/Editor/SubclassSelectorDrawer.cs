using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Core;

namespace Editor
{
    /// <summary>
    /// [SubclassSelector] 어트리뷰트가 붙은 [SerializeReference] 필드를
    /// 인스펙터에서 편하게 드롭다운으로 선택할 수 있도록 해주는 커스텀 드로어입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        // ── 타입별 캐시 ─────────────────────────────────
        private static readonly Dictionary<Type, (Type[] types, GUIContent[] labels)> STypeCache = new();

        /// <summary>
        /// 도메인 리로드 시 캐시 자동 초기화
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ClearCacheOnReload() => STypeCache.Clear();

        // ── OnGUI ───────────────────────────────────────
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Type fieldType = GetFieldType(property);
            if (fieldType == null)
            {
                EditorGUI.LabelField(position, label.text, "Type not supported");
                EditorGUI.EndProperty();
                return;
            }

            // 캐시에서 파생 타입 목록 및 라벨 가져오기
            var (derivedTypes, typeLabels) = GetOrBuildCache(fieldType);

            // 현재 할당된 타입 확인
            Type currentType = ResolveCurrentType(property);
            int currentIndex = 0;

            // Missing type 감지
            bool isMissing = currentType == null 
                             && !string.IsNullOrEmpty(property.managedReferenceFullTypename);

            if (currentType != null)
            {
                for (int i = 0; i < derivedTypes.Length; i++)
                {
                    if (currentType == derivedTypes[i])
                    {
                        currentIndex = i + 1;
                        break;
                    }
                }
            }

            // ── Foldout + Popup ──
            Rect popupRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(
                new Rect(popupRect.x, popupRect.y, EditorGUIUtility.labelWidth, popupRect.height),
                property.isExpanded, label, true);

            Rect dropdownRect = new Rect(
                popupRect.x + EditorGUIUtility.labelWidth, popupRect.y,
                popupRect.width - EditorGUIUtility.labelWidth, popupRect.height);

            // Missing type 경고 표시
            if (isMissing)
            {
                var warningStyle = new GUIStyle(EditorStyles.popup)
                {
                    normal =
                    {
                        textColor = Color.red
                    }
                };
                EditorGUI.LabelField(dropdownRect, $"⚠ Missing: {property.managedReferenceFullTypename}", warningStyle);
            }
            else
            {
                int newIndex = EditorGUI.Popup(dropdownRect, currentIndex, typeLabels);
                if (newIndex != currentIndex)
                {
                    property.managedReferenceValue = newIndex == 0
                        ? null
                        : Activator.CreateInstance(derivedTypes[newIndex - 1]);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // ── 자식 프로퍼티 렌더링 ──
            if (property.isExpanded && currentType != null && property.hasVisibleChildren)
            {
                EditorGUI.indentLevel++;
                Rect childRect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    EditorGUIUtility.singleLineHeight);

                ForEachDirectChild(property, child =>
                {
                    float childHeight = EditorGUI.GetPropertyHeight(child, true);
                    childRect.height = childHeight;
                    EditorGUI.PropertyField(childRect, child, true);
                    childRect.y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                });
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        // ── GetPropertyHeight ───────────────────────────
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded
                && !string.IsNullOrEmpty(property.managedReferenceFullTypename)
                && property.hasVisibleChildren)
            {
                ForEachDirectChild(property, child =>
                {
                    height += EditorGUI.GetPropertyHeight(child, true)
                              + EditorGUIUtility.standardVerticalSpacing;
                });
            }

            return height;
        }

        // ── 유틸리티 ────────────────────────────────────

        /// <summary>
        /// fieldType에서 파생된 타입 목록과 드롭다운 라벨을 캐시하여 반환합니다.
        /// </summary>
        private static (Type[] types, GUIContent[] labels) GetOrBuildCache(Type fieldType)
        {
            if (STypeCache.TryGetValue(fieldType, out var cached))
                return cached;

            var derivedTypes = TypeCache.GetTypesDerivedFrom(fieldType)
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericType)
                .OrderBy(t => t.Name)
                .ToArray();

            var labels = new GUIContent[derivedTypes.Length + 1];
            labels[0] = new GUIContent("<Null> (비우기)");
            for (int i = 0; i < derivedTypes.Length; i++)
            {
                labels[i + 1] = new GUIContent(derivedTypes[i].Name);
            }

            STypeCache[fieldType] = (derivedTypes, labels);
            return (derivedTypes, labels);
        }

        /// <summary>
        /// 현재 SerializeReference 필드에 할당된 타입을 해석합니다.
        /// Type.GetType 실패 시 AppDomain 어셈블리 전체를 검색합니다.
        /// </summary>
        private static Type ResolveCurrentType(SerializedProperty property)
        {
            string fullTypeName = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypeName)) return null;

            var parts = fullTypeName.Split(' ');
            if (parts.Length != 2) return null;

            string assemblyName = parts[0];
            string typeName = parts[1];

            // 빠른 경로
            Type result = Type.GetType($"{typeName}, {assemblyName}");
            if (result != null) return result;

            // Fallback: asmdef 경계를 넘는 경우
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == assemblyName)
                {
                    result = asm.GetType(typeName);
                    if (result != null) return result;
                }
            }

            return null;
        }

        /// <summary>
        /// property의 직계 자식 프로퍼티만 순회합니다.
        /// OnGUI/GetPropertyHeight 양쪽에서 재사용됩니다.
        /// </summary>
        private static void ForEachDirectChild(SerializedProperty property, Action<SerializedProperty> action)
        {
            string parentPath = property.propertyPath + ".";
            SerializedProperty iterator = property.Copy();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!iterator.propertyPath.StartsWith(parentPath))
                    break;
                action(iterator);
            }
        }

        /// <summary>
        /// SerializedProperty에서 필드의 기본 타입(인터페이스/부모 클래스)을 추출합니다.
        /// </summary>
        private static Type GetFieldType(SerializedProperty property)
        {
            string fullTypeName = property.managedReferenceFieldTypename;
            if (string.IsNullOrEmpty(fullTypeName)) return null;

            var parts = fullTypeName.Split(' ');
            if (parts.Length != 2) return null;

            Type result = Type.GetType($"{parts[1]}, {parts[0]}");
            if (result != null) return result;

            // Fallback
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == parts[0])
                    return asm.GetType(parts[1]);
            }

            return null;
        }
    }
}
