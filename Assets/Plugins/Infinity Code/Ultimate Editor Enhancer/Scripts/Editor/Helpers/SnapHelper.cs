/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using InfinityCode.UltimateEditorEnhancer.Integration;
using InfinityCode.UltimateEditorEnhancer.UnityTypes;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class SnapHelper
    {
        public static bool enabled
        {
            get { return ProGrids.snapEnabled || EditorSnapSettings.gridSnapEnabled; }
        }

        public static float value
        {
            get
            {
                if (ProGrids.snapEnabled) return ProGrids.snapValueInUnityUnits;
                return GridSettingsRef.GetSize().x;
            }
            set
            {
                if (ProGrids.snapEnabled) ProGrids.snapValueInUnityUnits = value;
                else GridSettingsRef.SetSize(new Vector3(value, value, value));
            }
        }

        public static void Snap(Transform transform, SnapAxis axis = SnapAxis.All)
        {
            if (ProGrids.isEnabled) ProGrids.SnapToGrid(transform);
            else transform.position = Snapping.Snap(transform.position, GridSettingsRef.GetSize(), axis);
        }

        public static Vector3 Snap(Vector3 position, SnapAxis axis = SnapAxis.All)
        {
            if (ProGrids.isEnabled) return ProGrids.SnapToGrid(position);
            return Snapping.Snap(position, GridSettingsRef.GetSize(), axis);
        }
    }
}