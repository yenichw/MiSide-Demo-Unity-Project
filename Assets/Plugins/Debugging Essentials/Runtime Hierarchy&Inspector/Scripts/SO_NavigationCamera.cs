using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebuggingEssentials
{
    // [CreateAssetMenu(fileName = "NavigationCamera_Data", menuName = "ScriptableObjects/DebuggingEssentials/NavigationCamera_Data", order = 1)]
    public class SO_NavigationCamera : ScriptableObject
    {
        public FloatInputField mouseSensitity = new FloatInputField(1.66f);

        public FloatInputField accelMulti = new FloatInputField(1);
        public FloatInputField decelMulti = new FloatInputField(15);
        public FloatInputField speedSlow = new FloatInputField(1);
        public FloatInputField speedNormal = new FloatInputField(10);
        public FloatInputField speedFast = new FloatInputField(25);
        public FloatInputField mouseScrollWheelMulti = new FloatInputField(25);
        public FloatInputField mouseStrafeMulti = new FloatInputField(0.5f);
    }
} 
