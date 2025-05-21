/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer.HierarchyTools
{
    [Serializable]
    public class BackgroundRule : ISerializationCallbackReceiver
    {
        public bool enabled = true;
        public BackgroundCondition condition = BackgroundCondition.nameEqual;
        public string value;
        public Color color = Color.gray;
        
        [SerializeField, HideInInspector]
        private bool _serialized;

        public JsonObject json
        {
            get
            {
                return Json.Serialize(this) as JsonObject;
            }
        }

        public bool Validate(GameObject go)
        {
            if (!enabled) return false;

            string name = go.name;
            switch (condition)
            {
                case BackgroundCondition.nameStarts:
                    return StringHelper.StartsWith(name, value);
                case BackgroundCondition.nameContains:
                    return StringHelper.Contains(name, value);
                case BackgroundCondition.nameEqual:
                    return name == value;
                default:
                    return false;
            }
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            if (_serialized) return;
            
            _serialized = true;
            
            enabled = true;
            condition = BackgroundCondition.nameStarts;
            value = "";
            color = Color.gray;
        }
    }
}