/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InfinityCode.UltimateEditorEnhancer.SceneTools.QuickAccessActions
{
    [Title("Save Scenes")]
    public class SaveAction: QuickAccessAction
    {
        private GUIContent _activeContent;
        private GUIContent _content;

        public override GUIContent content
        {
            get
            {
                bool isDirty = false;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    if (SceneManager.GetSceneAt(i).isDirty)
                    {
                        isDirty = true;
                        break;
                    }
                }

                if (isDirty)
                {
                    if (_activeContent == null)
                    {
                        _activeContent = new GUIContent(Icons.saveActive, "Save Scene(s)");
                    }

                    return _activeContent;
                }
                else
                {
                    if (_content == null)
                    {
                        _content = new GUIContent(Icons.save, "Save Scene(s)");
                    }

                    return _content;
                }
            }
        }

        public override void OnClick()
        {
            if (Event.current.button == 0)
            {
                EditorApplication.ExecuteMenuItem("File/Save");
            }
            else
            {
                GenericMenuEx menu = GenericMenuEx.Start();
                menu.Add("Save", () => EditorApplication.ExecuteMenuItem("File/Save"));
                menu.Add("Save As", () => EditorApplication.ExecuteMenuItem("File/Save As..."));

                menu.AddSeparator();

                menu.Add("Autosave/On Entering Playmode", Prefs.saveScenesWhenEnteringPlaymode, () =>
                {
                    Prefs.saveScenesWhenEnteringPlaymode = !Prefs.saveScenesWhenEnteringPlaymode;
                    Prefs.Save();
                });
                menu.Add("Autosave/By Timer", Prefs.saveScenesByTimer, () =>
                {
                    Prefs.saveScenesByTimer = !Prefs.saveScenesByTimer;
                    Prefs.Save();
                });
                menu.AddSeparator("Autosave/");
                menu.Add("Autosave/Settings", Settings.OpenSettings);
                menu.Show();
            }

            Event.current.Use();
        }

        public override void ResetContent()
        {
            _activeContent = null;
            _content = null;
        }
    }
}