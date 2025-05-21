using UnityEngine;
using UnityEditor;


namespace Rowlan.AnimationPreview
{
    [ExecuteInEditMode]
    public class MenuExtension : MonoBehaviour
    {
        [MenuItem("GameObject/Animation Preview/Add Animation Preview", false, 10)]
        static void AddPrefab(MenuCommand menuCommand)
        {
            // create new gameobject
            GameObject go = new GameObject();

            // settings
            go.name = "Animation Preview";

            // add the animation preview gameobject
            go.AddComponent<AnimationPreview>();

            // ensure gameobject gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            Selection.activeObject = go;

        }
    }
}