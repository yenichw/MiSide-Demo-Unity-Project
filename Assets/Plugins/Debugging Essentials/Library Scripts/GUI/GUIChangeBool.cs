using System;
using UnityEngine;

namespace DebuggingEssentials
{
    [Serializable]
    public class GUIChangeBool
    {
        static FastList<GUIChangeBool> applyList = new FastList<GUIChangeBool>();

        [SerializeField] bool value;
        [NonSerialized] bool newValue;
        [NonSerialized] bool hasChanged;

        public GUIChangeBool() { }
        public GUIChangeBool(bool value)
        {
            this.value = newValue = value;
        }

        public static void ResetStatic()
        {
            applyList.Clear();
        }

        public bool Value
        {
            get
            {
                return value;
            }
            set
            {
                // Debug.Log("Set value " + value);
                if (newValue == value && hasChanged) return;

                newValue = value;

                if (!hasChanged)
                {
                    applyList.Add(this);
                    hasChanged = true;
                }
            }
        }

        public bool RealValue
        {
            get
            {
                return !hasChanged ? value : newValue;
            }
        }

        public void Update()
        {
            value = newValue;
            hasChanged = false;
        }

        public static void ApplyUpdates()
        {
            for (int i = 0; i < applyList.Count; i++)
            {
                applyList.items[i].Update();
            }
            applyList.Clear();
        }
    }
}