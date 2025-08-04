// based on the original game.Yen Chezky(yenichw)
// Magica Cloth.
// Copyright (c) MagicaSoft, 2020-2022.
// https://magicasoft.jp
using UnityEditor;

namespace MagicaCloth
{
    /// <summary>
    /// MagicaBoneSpring??????
    /// </summary>
    public class MagicaBoneSpringGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Active)]
        //[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmo(MagicaBoneSpring scr, GizmoType gizmoType)
        {
            bool selected = (gizmoType & GizmoType.Selected) != 0 || (ClothMonitorMenu.Monitor != null && ClothMonitorMenu.Monitor.UI.AlwaysClothShow);

            if (scr.VerifyData() != Define.Error.None)
            {
                //DrawRootLine(scr);
                return;
            }

            if (PointSelector.EditEnable)
            {
                //DrawRootLine(scr);
                return;
            }

            if (ClothMonitorMenu.Monitor == null)
                return;

            if (selected == false)
                return;


            // ?????????
            DeformerGizmoDrawer.DrawDeformerGizmo(scr, scr, 0.015f);

            if (ClothMonitorMenu.Monitor.UI.DrawCloth)
            {
                // ??????
                ClothGizmoDrawer.DrawClothGizmo(
                    scr,
                    scr.ClothData,
                    scr.Params,
                    scr.Setup,
                    scr,
                    scr
                    );
            }
            //else
            //{
            //    DrawRootLine(scr);
            //}

        }

        //=========================================================================================
#if false
        static void DrawRootLine(BoneSpring scr)
        {
            for (int i = 0; i < scr.ClothTarget.RootCount; i++)
            {
                var root = scr.ClothTarget.GetRoot(i);
                if (root == null)
                    continue;

                DrawTransformLine(root, root);
            }
        }

        static void DrawTransformLine(Transform t, Transform root)
        {
            if (t == null)
                return;

            // ??????????
            if (PointSelector.EditEnable == false)
            {
                Gizmos.color = (t == root) ? GizmoUtility.ColorKinematic : GizmoUtility.ColorDynamic;
                GizmoUtility.DrawWireCube(t.position, t.rotation, Vector3.one * 0.01f);
            }

            int cnt = t.childCount;
            for (int i = 0; i < cnt; i++)
            {
                Transform ct = t.GetChild(i);

                // ???
                Gizmos.color = GizmoUtility.ColorRotationLine;
                Gizmos.DrawLine(t.position, ct.position);

                DrawTransformLine(ct, root);
            }
        }
#endif
    }
}
