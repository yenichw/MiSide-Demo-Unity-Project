// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEditor;
using UnityEditorInternal;

namespace MagicaCloth
{
    /// <summary>
    /// ??????????????????????
    /// </summary>
    public class CustomReorderableList : ReorderableList
    {
        public CustomReorderableList(SerializedObject serializedObject, SerializedProperty elements, string title)
            : base(serializedObject, elements, true, false, true, true)
        {
            // ?????????????
            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = elements.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                EditorGUI.PropertyField(rect, element);
            };

            // ??????
            drawHeaderCallback = (rect) =>
                     EditorGUI.LabelField(rect, title);
            //EditorGUI.LabelField(rect, elements.displayName);

            // +??????????
            onAddCallback += (list) =>
            {
                //?????
                elements.arraySize++;

                //?????????????
                list.index = elements.arraySize - 1;

                //???????null?????
                var element = elements.GetArrayElementAtIndex(list.index);
                element.objectReferenceValue = null;
            };
        }
    }
}
