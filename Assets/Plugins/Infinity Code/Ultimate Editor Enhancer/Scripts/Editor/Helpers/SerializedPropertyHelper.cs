/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Collections;
using InfinityCode.UltimateEditorEnhancer.JSON;
using UnityEditor;
using UnityEngine;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class SerializedPropertyHelper
    {
        public static object EMPTY_VALUE = new object();

        public static object GetBoxedValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return EMPTY_VALUE;
                case SerializedPropertyType.Enum:
                    return prop.enumValueIndex;
                case SerializedPropertyType.Integer:
#if UNITY_2022_1_OR_NEWER
                    switch (prop.numericType)
                    {
                        case SerializedPropertyNumericType.Int8:
                            return (sbyte)prop.intValue;
                        case SerializedPropertyNumericType.UInt8:
                            return (byte)prop.uintValue;
                        case SerializedPropertyNumericType.Int16:
                            return (short)prop.intValue;
                        case SerializedPropertyNumericType.UInt16:
                            return (ushort)prop.uintValue;
                        case SerializedPropertyNumericType.UInt32:
                            return prop.uintValue;
                        case SerializedPropertyNumericType.Int64:
                            return prop.longValue;
                        case SerializedPropertyNumericType.UInt64:
                            return prop.ulongValue;
                        default:
                            return prop.intValue;
                    }
#else
                    return prop.intValue;
#endif
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case SerializedPropertyType.Float:
#if UNITY_2022_1_OR_NEWER
                    return prop.numericType == SerializedPropertyNumericType.Double ? prop.doubleValue : prop.floatValue;
#else
                    return prop.floatValue;
#endif
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask)prop.intValue;
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                case SerializedPropertyType.ArraySize:
                    return prop.intValue;
                case SerializedPropertyType.Character:
#if UNITY_2022_1_OR_NEWER
                    return (ushort)prop.uintValue;
#else
                    return (ushort)prop.intValue;
#endif
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                case SerializedPropertyType.Gradient:
#if UNITY_2022_1_OR_NEWER
                    return prop.gradientValue;
#else
                    return EMPTY_VALUE;
#endif
                case SerializedPropertyType.Quaternion:
                    return prop.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return prop.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return prop.intValue;
                case SerializedPropertyType.Vector2Int:
                    return prop.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return prop.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return prop.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return prop.boundsIntValue;
                case SerializedPropertyType.ManagedReference:
#if UNITY_2022_1_OR_NEWER
                    return prop.managedReferenceValue;
#else
                    return EMPTY_VALUE;
#endif

#if UNITY_2022_1_OR_NEWER
                case SerializedPropertyType.Hash128:
                    return prop.hash128Value;
#endif
                default:
                    return EMPTY_VALUE;
            }
        }

        public static void FromJson(SerializedProperty prop, JsonItem json)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    if (prop.isArray)
                    {
                        JsonArray array = json as JsonArray;
                        if (array == null) return;
                        int arraySize = array.count;
                        prop.arraySize = arraySize;
                        for (int i = 0; i < arraySize; i++)
                        {
                            FromJson(prop.GetArrayElementAtIndex(i), array[i]);
                        }
                    }
                    
                    JsonObject obj = json as JsonObject;
                    if (obj == null) return;
                    foreach (var pair in obj.table)
                    {
                        SerializedProperty child = prop.FindPropertyRelative(pair.Key);
                        if (child == null) continue;
                        FromJson(child, pair.Value);
                    }

                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = json.Value<int>();
                    break;
                case SerializedPropertyType.Integer:
#if UNITY_2022_1_OR_NEWER
                    switch (prop.numericType)
                    {
                        case SerializedPropertyNumericType.Int8:
                        case SerializedPropertyNumericType.Int16:
                            prop.intValue = json.Value<int>();
                            break;
                        case SerializedPropertyNumericType.UInt8:
                        case SerializedPropertyNumericType.UInt16:
                        case SerializedPropertyNumericType.UInt32:
                            prop.uintValue = (uint)json.Value<long>();
                            break;
                        case SerializedPropertyNumericType.Int64:
                            prop.longValue = json.Value<long>();
                            break;
                        case SerializedPropertyNumericType.UInt64:
                            prop.ulongValue = (ulong)json.Value<long>();
                            break;
                        default:
                            prop.intValue = json.Value<int>();
                            break;
                    }
#else
                    prop.intValue = json.Value<int>();
#endif
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = json.Value<bool>();
                    break;
                case SerializedPropertyType.Float:
#if UNITY_2022_1_OR_NEWER
                    if (prop.numericType == SerializedPropertyNumericType.Double)
                    {
                        prop.doubleValue = json.Value<double>();
                    }
                    else
                    {
                        prop.floatValue = json.Value<float>();
                    }
#else
                    prop.floatValue = json.Value<float>();
#endif
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = json.Value<string>();
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = json.Deserialize<Color>();
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = json.Value<int>() != 0 ? EditorUtility.InstanceIDToObject(json.Value<int>()) : null;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = json.Value<int>();
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = json.Deserialize<Vector2>();
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = json.Deserialize<Vector3>();
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = json.Deserialize<Vector4>();
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = json.Deserialize<Rect>();
                    break;
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.FixedBufferSize:
                    prop.intValue = json.Value<int>();
                    break;
                case SerializedPropertyType.Character:
#if UNITY_2022_1_OR_NEWER
                    prop.uintValue = (uint)json.Value<long>();
#else
                    prop.intValue = json.Value<int>();
#endif
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = json.Deserialize<AnimationCurve>();
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = json.Deserialize<Bounds>();
                    break;
                case SerializedPropertyType.Gradient:
#if UNITY_2022_1_OR_NEWER
                    prop.gradientValue = json.Deserialize<Gradient>();
#endif
                    break;
                case SerializedPropertyType.Quaternion:
                    prop.quaternionValue = json.Deserialize<Quaternion>();
                    break;
                case SerializedPropertyType.ExposedReference:
                    prop.exposedReferenceValue = json.Value<int>() != 0 ? EditorUtility.InstanceIDToObject(json.Value<int>()) : null;
                    break;
                case SerializedPropertyType.Vector2Int:
                    prop.vector2IntValue = json.Deserialize<Vector2Int>();
                    break;
                case SerializedPropertyType.Vector3Int:
                    prop.vector3IntValue = json.Deserialize<Vector3Int>();
                    break;
                case SerializedPropertyType.RectInt:
                    prop.rectIntValue = json.Deserialize<RectInt>();
                    break;
                case SerializedPropertyType.BoundsInt:
                    prop.boundsIntValue = json.Deserialize<BoundsInt>();
                    break;
            }
        }

        public static void SetBoxedValue(SerializedProperty prop, object value)
        {
            if (value == EMPTY_VALUE) return;

            try
            {
                switch (prop.propertyType)
                {
                    case SerializedPropertyType.Generic:
                        break;
                    case SerializedPropertyType.Enum:
                        prop.enumValueIndex = (int)value;
                        break;
                    case SerializedPropertyType.Integer:
                    case SerializedPropertyType.ArraySize:
#if UNITY_2022_1_OR_NEWER
                        if (prop.numericType == SerializedPropertyNumericType.UInt64)
                        {
                            prop.ulongValue = Convert.ToUInt64(value);
                        }
                        else prop.longValue = Convert.ToInt64(value);
#else
                        prop.intValue = Convert.ToInt32(value);
#endif
                        break;
                    case SerializedPropertyType.Boolean:
                        prop.boolValue = (bool)value;
                        break;
                    case SerializedPropertyType.Float:
#if UNITY_2022_1_OR_NEWER
                        if (prop.numericType == SerializedPropertyNumericType.Double)
                        {
                            prop.doubleValue = Convert.ToDouble(value);
                        }
                        else
                        {
                            prop.floatValue = Convert.ToSingle(value);
                        }
#else
                        prop.floatValue = Convert.ToSingle(value);
#endif
                        break;
                    case SerializedPropertyType.String:
                        prop.stringValue = (string)value;
                        break;
                    case SerializedPropertyType.Color:
                        prop.colorValue = (Color)value;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        prop.objectReferenceValue = (UnityEngine.Object)value;
                        break;
                    case SerializedPropertyType.LayerMask:
                        try
                        {
                            prop.intValue = ((LayerMask)value).value;
                            break;
                        }
                        catch (InvalidCastException)
                        {
                            prop.intValue = Convert.ToInt32(value);
                            break;
                        }
                    case SerializedPropertyType.Vector2:
                        prop.vector2Value = (Vector2)value;
                        break;
                    case SerializedPropertyType.Vector3:
                        prop.vector3Value = (Vector3)value;
                        break;
                    case SerializedPropertyType.Vector4:
                        prop.vector4Value = (Vector4)value;
                        break;
                    case SerializedPropertyType.Rect:
                        prop.rectValue = (Rect)value;
                        break;
                    case SerializedPropertyType.Character:
#if UNITY_2022_1_OR_NEWER
                        prop.uintValue = Convert.ToUInt16(value);
#endif
                        break;
                    case SerializedPropertyType.AnimationCurve:
                        prop.animationCurveValue = (AnimationCurve)value;
                        break;
                    case SerializedPropertyType.Bounds:
                        prop.boundsValue = (Bounds)value;
                        break;
                    case SerializedPropertyType.Gradient:
#if UNITY_2022_1_OR_NEWER
                        prop.gradientValue = (Gradient)value;
#endif
                        break;
                    case SerializedPropertyType.Quaternion:
                        prop.quaternionValue = (Quaternion)value;
                        break;
                    case SerializedPropertyType.ExposedReference:
                        prop.exposedReferenceValue = (UnityEngine.Object)value;
                        break;
                    case SerializedPropertyType.Vector2Int:
                        prop.vector2IntValue = (Vector2Int)value;
                        break;
                    case SerializedPropertyType.Vector3Int:
                        prop.vector3IntValue = (Vector3Int)value;
                        break;
                    case SerializedPropertyType.RectInt:
                        prop.rectIntValue = (RectInt)value;
                        break;
                    case SerializedPropertyType.BoundsInt:
                        prop.boundsIntValue = (BoundsInt)value;
                        break;
                    case SerializedPropertyType.ManagedReference:
                        prop.managedReferenceValue = value;
                        break;
#if UNITY_2022_1_OR_NEWER
                    case SerializedPropertyType.Hash128:
                        prop.hash128Value = (Hash128)value;
                        break;
#endif
                }
            }
            catch
            {

            }
        }

        public static JsonItem ToJson(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    string path = prop.propertyPath + ".";
                    SerializedProperty propCopy;
                    
                    if (prop.isArray)
                    {
                        JsonArray array = new JsonArray();
                        int arraySize = prop.arraySize;
                        
                        prop.NextVisible(true);
                        if (arraySize == 0) return array;
                        
                        propCopy = prop.Copy();
                        
                        while (propCopy.NextVisible(true))
                        {
                            if (propCopy.propertyPath.StartsWith(path))
                            {
                                prop.NextVisible(true);
                                array.Add(ToJson(prop));
                                propCopy = prop.Copy();
                            }
                            else break;
                        }

                        return array;
                    }
                    
                    JsonObject obj = new JsonObject();
                    propCopy = prop.Copy();
                    
                    while (propCopy.NextVisible(true))
                    {
                        if (propCopy.propertyPath.StartsWith(path))
                        {
                            prop.NextVisible(true);
                            string propName = prop.name;
                            obj.Add(propName, ToJson(prop));
                            propCopy = prop.Copy();
                        }
                        else break;
                    }

                    return obj;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Enum:
#if UNITY_2022_1_OR_NEWER
                    switch (prop.numericType)
                    {
                        case SerializedPropertyNumericType.Int8:
                        case SerializedPropertyNumericType.Int16:
                            return new JsonValue(prop.intValue);
                        case SerializedPropertyNumericType.UInt8:
                        case SerializedPropertyNumericType.UInt16:
                        case SerializedPropertyNumericType.UInt32:
                            return new JsonValue((long)prop.uintValue);
                        case SerializedPropertyNumericType.Int64:
                            return new JsonValue(prop.longValue);
                        case SerializedPropertyNumericType.UInt64:
                            return new JsonValue((long)prop.ulongValue);
                        default:
                            return new JsonValue(prop.intValue);
                    }
#else
                    return new JsonValue(prop.intValue);
#endif
                case SerializedPropertyType.Boolean:
                    return new JsonValue(prop.boolValue);
                case SerializedPropertyType.Float:
#if UNITY_2022_1_OR_NEWER
                    return new JsonValue(prop.numericType == SerializedPropertyNumericType.Double ? prop.doubleValue : prop.floatValue);
#else
                    return new JsonValue(prop.floatValue);
#endif
                case SerializedPropertyType.String:
                    return new JsonValue(prop.stringValue);
                case SerializedPropertyType.Color:
                    return Json.Serialize(prop.colorValue);
                case SerializedPropertyType.ObjectReference:
                    if (prop.objectReferenceValue == null) return new JsonValue(null);
                    return new JsonValue(prop.objectReferenceValue.GetInstanceID());
                case SerializedPropertyType.LayerMask:
                    return new JsonValue(prop.intValue);
                case SerializedPropertyType.Vector2:
                    return Json.Serialize(prop.vector2Value);
                case SerializedPropertyType.Vector3:
                    return Json.Serialize(prop.vector3Value);
                case SerializedPropertyType.Vector4:
                    return Json.Serialize(prop.vector4Value);
                case SerializedPropertyType.Rect:
                    return Json.Serialize(prop.rectValue);
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.FixedBufferSize:
                    return Json.Serialize(prop.intValue);
                case SerializedPropertyType.Character:
#if UNITY_2022_1_OR_NEWER
                    return new JsonValue((long)prop.uintValue);
#else
                    return new JsonValue(prop.intValue);
#endif
                case SerializedPropertyType.AnimationCurve:
                    return Json.Serialize(prop.animationCurveValue);
                case SerializedPropertyType.Bounds:
                    return Json.Serialize(prop.boundsValue);
                case SerializedPropertyType.Gradient:
#if UNITY_2022_1_OR_NEWER
                    return Json.Serialize(prop.gradientValue);
#else
                    return new JsonValue(null);
#endif
                case SerializedPropertyType.Quaternion:
                    return Json.Serialize(prop.quaternionValue);
                case SerializedPropertyType.ExposedReference:
                    if (prop.exposedReferenceValue == null) return new JsonValue(null);
                    return new JsonValue(prop.exposedReferenceValue.GetInstanceID());
                case SerializedPropertyType.Vector2Int:
                    return Json.Serialize(prop.vector2IntValue);
                case SerializedPropertyType.Vector3Int:
                    return Json.Serialize(prop.vector3IntValue);
                case SerializedPropertyType.RectInt:
                    return Json.Serialize(prop.rectIntValue);
                case SerializedPropertyType.BoundsInt:
                    return Json.Serialize(prop.boundsIntValue);
                default:
                    return new JsonValue(null);
            }
        }
    }
}