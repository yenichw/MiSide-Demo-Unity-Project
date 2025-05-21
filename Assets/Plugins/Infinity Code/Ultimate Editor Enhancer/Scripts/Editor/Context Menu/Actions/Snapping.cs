/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.Integration;
using InfinityCode.UltimateEditorEnhancer.Windows;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.EditorMenus.Actions
{
    public class Snapping : ActionItem
    {
        protected override bool closeOnSelect
        {
            get { return false; }
        }

        public override float order
        {
            get { return -990; }
        }

        private void AlignSelection(SnapAxis axis)
        {
            foreach (GameObject gameObject in Selection.gameObjects)
            {
                SnapHelper.Snap(gameObject.transform, axis);
            }

            EditorMenu.Close();
        }

        protected override void Init()
        {
            guiContent = new GUIContent(Icons.proGridsWhite, "Snapping");
        }

        public override void Invoke()
        {
            GenericMenuEx menu = GenericMenuEx.Start();

            if (!ProGrids.snapEnabled)
            {
                menu.Add("Snap To Grid", EditorSnapSettings.gridSnapEnabled, () => EditorSnapSettings.gridSnapEnabled = !EditorSnapSettings.gridSnapEnabled);
                menu.AddSeparator(string.Empty);
            }

            if (SelectionBoundsManager.hasBounds)
            {
                if (!ProGrids.snapEnabled)
                {
                    menu.Add("Align Selection To Grid/All Axis", () => AlignSelection(SnapAxis.All));
                    menu.Add("Align Selection To Grid/X", () => AlignSelection(SnapAxis.X));
                    menu.Add("Align Selection To Grid/Y", () => AlignSelection(SnapAxis.Y));
                    menu.Add("Align Selection To Grid/Z", () => AlignSelection(SnapAxis.Z));
                }

                Vector3 boundsSize = SelectionBoundsManager.bounds.size;
                boundsSize.x = Mathf.Round(boundsSize.x * 1000) / 1000;
                boundsSize.y = Mathf.Round(boundsSize.y * 1000) / 1000;
                boundsSize.z = Mathf.Round(boundsSize.z * 1000) / 1000;

                menu.Add("Selection/X (" + boundsSize.x + ")", () => SetSnapValue(boundsSize.x));
                menu.Add("Selection/Y (" + boundsSize.y + ")", () => SetSnapValue(boundsSize.y));
                menu.Add("Selection/Z (" + boundsSize.z + ")", () => SetSnapValue(boundsSize.z));
            }

            float[] values = {0.1f, 0.25f, 0.5f, 1, 1.5f, 2, 3, 4, 8};
            float snapValue = SnapHelper.value;
            foreach (float v in values)
            {
                menu.Add(v.ToString(), Math.Abs(snapValue - v) < float.Epsilon, () => SetSnapValue(v));
            }

            menu.Add("Set Custom Value", () =>
            {
                InputDialog.Show("Enter snapping value", snapValue.ToString(), OnSnapValueEntered);
            });

            menu.Show();
        }

        private void OnSnapValueEntered(string s)
        {
            float v;
            if (float.TryParse(s, out v)) SetSnapValue(v);
        }

        private void SetSnapValue(float value)
        {
            SnapHelper.value = value;
            EditorMenu.Close();
        }
    }
}