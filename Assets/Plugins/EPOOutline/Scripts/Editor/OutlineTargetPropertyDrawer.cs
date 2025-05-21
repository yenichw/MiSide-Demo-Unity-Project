using UnityEditor;
using UnityEngine;


namespace EPOOutline
{
    [CustomPropertyDrawer(typeof(OutlineTarget))]
    public class OutlineTargetPropertyDrawer : PropertyDrawer
    {
        private void Shift(ref Rect rect, bool right)
        {
            if (right)
                rect.x += EditorGUIUtility.singleLineHeight;

            rect.width -= EditorGUIUtility.singleLineHeight * (right ? 1.0f : 0.5f);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var initialPosition = position;

            var labelWidth = EditorGUIUtility.labelWidth;
            position.y += EditorGUIUtility.singleLineHeight * 0.2f;

            position.height = EditorGUIUtility.singleLineHeight;
            var rendererPosition = position;
            rendererPosition.width = position.width / 2;
            Shift(ref rendererPosition, false);
            var renderer = property.FindPropertyRelative("renderer");
            EditorGUI.PropertyField(rendererPosition, renderer, GUIContent.none);

            var menu = new GenericMenu();
            var textureNameProperty = property.FindPropertyRelative("cutoutTextureName");

            var cutoutIsInUse = !string.IsNullOrEmpty(textureNameProperty.stringValue);
            menu.AddItem(new GUIContent("none"), string.IsNullOrEmpty(textureNameProperty.stringValue), () =>
                {
                    textureNameProperty.stringValue = string.Empty;
                    textureNameProperty.serializedObject.ApplyModifiedProperties();
                });
            
            var rendererReference = renderer.objectReferenceValue as Renderer;
            var referenceName = "none";
            var usingCutout = cutoutIsInUse && rendererReference != null;
            if (rendererReference != null)
            {
                var materialSubMeshIndex = property.FindPropertyRelative("SubmeshIndex").intValue;
                var materials = rendererReference.sharedMaterials;
                if (materials.Length > 0)
                {
                    var material = materials[materialSubMeshIndex % materials.Length];
                    if (material != null)
                    {
                        var propertiesCount = ShaderUtil.GetPropertyCount(material.shader);
                        for (var index = 0; index < propertiesCount; index++)
                        {
                            var propertyType = ShaderUtil.GetPropertyType(material.shader, index);
                            if (propertyType != ShaderUtil.ShaderPropertyType.TexEnv)
                                continue;

                            var propertyName = ShaderUtil.GetPropertyName(material.shader, index);
                            var equals = propertyName == textureNameProperty.stringValue;
                            if (equals)
                                referenceName = ShaderUtil.GetPropertyDescription(material.shader, index) + " '" +
                                                propertyName + "'";

                            menu.AddItem(
                                new GUIContent(ShaderUtil.GetPropertyDescription(material.shader, index) + " '" +
                                               propertyName + "'"), equals && usingCutout, () =>
                                {
                                    textureNameProperty.stringValue = propertyName;
                                    textureNameProperty.serializedObject.ApplyModifiedProperties();
                                });
                        }
                    }
                }
            }

            var cutoutPosition = position;
            cutoutPosition.x = rendererPosition.x + rendererPosition.width;
            cutoutPosition.width -= rendererPosition.width;
            Shift(ref cutoutPosition, true);

            var sourceLable = usingCutout ? referenceName : "none";

            var cutoutSourceLabel = "Cutout source: " + sourceLable; 

            if (EditorGUI.DropdownButton(cutoutPosition, new GUIContent(cutoutSourceLabel), FocusType.Passive))
                menu.ShowAsContext();

            var drawingPosition = position;

            EditorGUIUtility.labelWidth = 80;
            drawingPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var boundsModePosition = initialPosition;
            boundsModePosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 3.0f;
            boundsModePosition.width = cutoutPosition.width / 2.0f;
            boundsModePosition.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.LabelField(boundsModePosition, new GUIContent("Bounds mode"));
            boundsModePosition.x += boundsModePosition.width;
            EditorGUI.PropertyField(boundsModePosition, property.FindPropertyRelative("BoundsMode"), GUIContent.none);

            boundsModePosition.x += boundsModePosition.width;

            EditorGUIUtility.labelWidth = 90;
            boundsModePosition.width = drawingPosition.width / 2.0f;
            Shift(ref boundsModePosition, true);
            
            if (usingCutout)
                EditorGUI.PropertyField(boundsModePosition, property.FindPropertyRelative("cutoutTextureIndex"), new GUIContent("Texture index"));

            if (property.FindPropertyRelative("BoundsMode").intValue == (int)BoundsMode.Manual)
            {
                drawingPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(drawingPosition, property.FindPropertyRelative("Bounds"), GUIContent.none);
                drawingPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUIUtility.labelWidth = labelWidth;

            if (usingCutout || rendererReference is SpriteRenderer)
            {
                drawingPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(drawingPosition, property.FindPropertyRelative("CutoutThreshold"));

                drawingPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(drawingPosition, property.FindPropertyRelative("CutoutMask"));
            }

            drawingPosition.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            var linePosition = drawingPosition;
            linePosition.width /= 2;
            Shift(ref linePosition, false);

            var cullPosition = linePosition;
            cullPosition.width /= 2;
            EditorGUI.LabelField(cullPosition, new GUIContent("Cull mode"));
            cullPosition.x += cullPosition.width;

            EditorGUI.PropertyField(cullPosition, property.FindPropertyRelative("CullMode"), GUIContent.none);

            linePosition.x += linePosition.width;
            Shift(ref linePosition, true);

            var submeshIndex = property.FindPropertyRelative("SubmeshIndex");

            EditorGUIUtility.labelWidth = 90;

            EditorGUI.PropertyField(linePosition, submeshIndex);
            if (submeshIndex.intValue < 0)
                submeshIndex.intValue = 0;

            EditorGUIUtility.labelWidth = labelWidth;

            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var renderer = property.FindPropertyRelative("renderer");

            var rendererReference = renderer.objectReferenceValue as Renderer;
            var textureNameProperty = property.FindPropertyRelative("cutoutTextureName");

            var usingCutout = !string.IsNullOrEmpty(textureNameProperty.stringValue);

            var linesCount = 3.0f;

            if (usingCutout || rendererReference is SpriteRenderer)
                linesCount += 2.0f;

            if (property.FindPropertyRelative("BoundsMode").intValue == (int)BoundsMode.Manual)
                linesCount += 2.0f;

            return (EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight) * (linesCount + 0.5f);
        }
    }
}