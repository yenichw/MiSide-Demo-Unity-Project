/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.Windows
{
    public partial class CreateBrowser
    {
        public class PrefabItem : Item
        {
            public string path;
            [NonSerialized]
            private bool previewLoaded;
            private GameObject asset;

            public PrefabItem(string label, string path)
            {
                if (label.Length < 8) return;

                int lastDot = 1;

                for (int i = label.Length - 1; i >= 2; i--)
                {
                    if (label[i] == '.')
                    {
                        lastDot = i;
                        break;
                    }
                }

                this.label = label.Substring(0, lastDot);
                this.path = path;
            }

            public override void Dispose()
            {
                base.Dispose();

                path = null;
                asset = null;
            }

            public override void Draw()
            {
                if (!Prefs.createBrowserPreviewIcons)
                {
                    if (content.image == null) content.image = EditorResources.prefabTexture;
                }
                else if (content.image == null || !previewLoaded)
                {
                    content.image = EditorResources.prefabTexture;
                    previewLoaded = false;

                    if (asset == null && EditorApplication.timeSinceStartup - loadTimer < 0.3)
                    {
                        try
                        {
                            if (File.Exists(path)) asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            else
                            {
                                content.image = EditorResources.prefabTexture;
                                previewLoaded = true;
                            }
                        }
                        catch
                        {

                        }
                    }
                    else GUI.changed = true; 

                    if (asset != null)
                    {
                        if (AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
                        {
                            content.image = EditorResources.prefabTexture;
                            GUI.changed = true;
                        }
                        else
                        {
                            content.image = AssetPreview.GetAssetPreview(asset);
                            if (!AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
                            {
                                if (content.image == null)
                                {
                                    content.image = EditorResources.prefabTexture;
                                    previewLoaded = true;
                                    GUI.changed = true;
                                }
                                else if (content.image.name != "Prefab Icon" && content.image.name != "d_Prefab Icon")
                                {
                                    previewLoaded = true;
                                    GUI.changed = true;
                                }
                            }
                            else
                            {
                                content.image = EditorResources.prefabTexture;
                                GUI.changed = true;
                            }
                        }
                    }
                }

                base.Draw();
            }

            public void DrawPreview()
            {
                if (previewPrefab != this && previewEditor != null)
                {
                    DestroyImmediate(previewEditor);
                }

                if (previewEditor == null)
                {
                    previewPrefab = this;
                    previewEditor = Editor.CreateEditor(AssetDatabase.LoadAssetAtPath<GameObject>(path));
                }

                if (previewEditor != null) previewEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(1, PreviewHeight), Styles.grayRow);
            }

            protected override void InitContent()
            {
                _content = new GUIContent(label, path);
            }

            public override void OnClick()
            {
                if (instance.OnSelectPrefab != null) instance.OnSelectPrefab(path);
                instance.Close();
            }
        }
    }
}