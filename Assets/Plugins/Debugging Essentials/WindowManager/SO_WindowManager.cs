using DebuggingEssentials;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "WindowManager", menuName = "ScriptableObjects/DebuggingEssentials/WindowManager_Data", order = 1)]
public class SO_WindowManager : ScriptableObject
{
    public FloatInputField guiScale = new FloatInputField(1);
}
