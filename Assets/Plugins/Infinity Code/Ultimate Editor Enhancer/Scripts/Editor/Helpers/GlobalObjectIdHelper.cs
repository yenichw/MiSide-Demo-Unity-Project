/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class GlobalObjectIdHelper
    {
        public static GlobalObjectId GetGlobalObjectIdSlow(Object targetObject)
        {
            bool logEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(targetObject);
            Debug.unityLogger.logEnabled = logEnabled;
            return id;
        }

        public static Object GlobalObjectIdentifierToObjectSlow(GlobalObjectId id)
        {
            bool logEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            Object result = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
            Debug.unityLogger.logEnabled = logEnabled;
            return result;
        }

        public static bool TryParse(string stringValue, out GlobalObjectId id)
        {
            bool logEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;
            bool result = GlobalObjectId.TryParse(stringValue, out id);
            Debug.unityLogger.logEnabled = logEnabled;
            return result;
        }
    }
}