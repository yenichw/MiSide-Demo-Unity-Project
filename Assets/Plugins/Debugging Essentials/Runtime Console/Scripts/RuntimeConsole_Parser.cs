using System;
using UnityEngine;

namespace DebuggingEssentials
{
    public partial class RuntimeConsole
    {
        static bool TryParse(Type t, FastQueue<string> paramQueue, out object result)
        {
            bool valid = true;

            if (t == typeof(Vector2))
            {
                var v = new Vector2();
                for (int i = 0; i < 2; i++)
                {
                    object r = ChangeType(typeof(float), paramQueue, ref valid);
                    if (r != null) v[i] = (float)r; else { result = null; return false; }
                }
                result = v;
            }
            else if (t == typeof(Vector3))
            {
                var v = new Vector3();
                for (int i = 0; i < 3; i++)
                {
                    object r = ChangeType(typeof(float), paramQueue, ref valid);
                    if (r != null) v[i] = (float)r; else { result = null; return false; }
                }
                result = v;
            }
            else if (t == typeof(Vector4))
            {
                var v = new Vector4();
                for (int i = 0; i < 4; i++)
                {
                    object r = ChangeType(typeof(float), paramQueue, ref valid);
                    if (r != null) v[i] = (float)r; else { result = null; return false; }
                }
                result = v;
            }
            else if (t == typeof(Quaternion))
            {
                var v = new Quaternion();
                for (int i = 0; i < 4; i++)
                {
                    object r = ChangeType(typeof(float), paramQueue, ref valid);
                    if (r != null) v[i] = (float)r; else { result = null; return false; }
                }
                result = v;
            }
            else result = ChangeType(t, paramQueue, ref valid);

            return valid;
        }

        static object ChangeType(Type t, FastQueue<string> paramQueue, ref bool valid)
        {
            if (paramQueue.Count == 0)
            {
                LogResultError("Not enough parameters");
                valid = false;
                return null;
            }

            string value = paramQueue.Dequeue();
            value = value.Trim();

            // Debug.Log("TryParse: " + value);

            if (t == typeof(string)) return value;
            else if (t == typeof(bool)) { bool result; bool.TryParse(value, out result); return result; }
            else if (t == typeof(byte)) { byte result; byte.TryParse(value, out result); return result; }
            else if (t == typeof(sbyte)) { sbyte result; sbyte.TryParse(value, out result); return result; }
            else if (t == typeof(char)) { char result; char.TryParse(value, out result); return result; }
            else if (t == typeof(decimal)) { decimal result; decimal.TryParse(value, out result); return result; }
            else if (t == typeof(double)) { double result; double.TryParse(value, out result); return result; }
            else if (t == typeof(float)) { float result; float.TryParse(value, out result); return result; }
            else if (t == typeof(int)) { int result; int.TryParse(value, out result); return result; }
            else if (t == typeof(uint)) { uint result; uint.TryParse(value, out result); return result; }
            else if (t == typeof(long)) { long result; long.TryParse(value, out result); return result; }
            else if (t == typeof(ulong)) { ulong result; ulong.TryParse(value, out result); return result; }
            else if (t == typeof(short)) { short result; short.TryParse(value, out result); return result; }
            else if (t == typeof(ushort)) { ushort result; ushort.TryParse(value, out result); return result; }
            else if (t.IsEnum)
            {
                try
                {
                    return Enum.Parse(t, value, true);
                }
                catch (Exception)
                {
                    LogResultError("Cannot find '" + value + "'");
                }
            }

            valid = false;
            return null;
        }
    }
}
