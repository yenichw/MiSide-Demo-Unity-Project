/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.ProjectTools
{
    [InitializeOnLoad]
    public static class ProjectPlayAudio
    {
        private static AudioClip clip;

        static ProjectPlayAudio()
        {
            ProjectItemDrawer.Register("PLAY_AUDIO", DrawButton, ProjectToolOrder.PlayAudio);
        }

        private static void DrawButton(ProjectItem item)
        {
            if (!Prefs.projectPlayAudio) return;
            Object asset = item.asset;
            if (asset == null) return;
            if (!(asset is AudioClip)) return;

            bool isPlaying = clip == asset;

            if (isPlaying)
            {
                if (!AudioUtilsRef.IsClipPlaying(clip))
                {
                    isPlaying = false;
                    clip = null;
                }
            }

            if (!item.hovered && !isPlaying) return;

            Rect r = item.rect;
            r.xMin = r.xMax - 18;
            r.height = 16;

            item.rect.xMax -= 18;

            Texture icon;
            string tooltip;
            if (isPlaying)
            {
                icon = EditorIconContents.preAudioPlayOn.image;
                tooltip = "Stop Audio";
            }
            else
            {
                icon = EditorIconContents.playButtonOn.image;
                tooltip = "Play Audio";
            }
            
            ButtonEvent be = GUILayoutUtils.Button(r, TempContent.Get(icon, tooltip), GUIStyle.none);
            if (be == ButtonEvent.click) ProcessClick(asset, isPlaying);
        }

        private static void ProcessClick(Object asset, bool isPlaying)
        {
            Event e = Event.current;
            if (e.button != 0) return;
            
            if (isPlaying)
            {
                AudioUtilsRef.StopAllClips();
                clip = null;
            }
            else
            {
                AudioUtilsRef.StopAllClips();
                clip = asset as AudioClip;
                AudioUtilsRef.PlayClip(clip);
            }
            e.Use();
        }
    }
}

    