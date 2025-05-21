using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebuggingEssentials
{
    public static class Parser
    {
        static public bool TryParse(Type t, FastQueue<string> paramQueue, out object result)
        {
            bool valid = true;

            if (t == typeof(Vector2))
            {
                var v = new Vector2();
                for (int i = 0; i < 2; i++)
                {
                    valid = ChangeType(typeof(float), paramQueue, out result);
                    if (valid) v[i] = (float)result; else return false;
                }
                result = v;
            }
            else if (t == typeof(Vector3))
            {
                var v = new Vector3();
                for (int i = 0; i < 3; i++)
                {
                    valid = ChangeType(typeof(float), paramQueue, out result);
                    if (valid) v[i] = (float)result; else return false;
                }
                result = v;
            }
            else if (t == typeof(Vector4))
            {
                var v = new Vector4();
                for (int i = 0; i < 4; i++)
                {
                    valid = ChangeType(typeof(float), paramQueue, out result);
                    if (valid) v[i] = (float)result; else return false;
                }
                result = v;
            }
            else if (t == typeof(Quaternion))
            {
                var v = new Quaternion();
                for (int i = 0; i < 4; i++)
                {
                    valid = ChangeType(typeof(float), paramQueue, out result);
                    if (valid) v[i] = (float)result; else return false;
                }
                result = v;
            }
            else valid = ChangeType(t, paramQueue, out result);

            return valid;
        }

        static bool ChangeType(Type t, FastQueue<string> paramQueue, out object result)
        {
            if (paramQueue.Count == 0)
            {
                RuntimeConsole.LogResultError("Not enough parameters");
                result = null;
                return false;
            }

            string value = paramQueue.Dequeue();
            return ChangeType(t, value, out result);
        }

        public static bool ChangeType(Type t, string value, out object result, bool logError = true)
        {
            value = value.Trim();

            // Debug.Log("TryParse: " + value);
            if (t == typeof(string)) { result = value; return true; }
            else if (t.IsEnum)
            {
                try
                {
                    result = Enum.Parse(t, value, true);
                    return true;
                }
                catch (Exception)
                {
                    if (logError) RuntimeConsole.LogResultError("Cannot find '" + value + "'");
                }
            }
            else
            {
                try
                {
                    result = Convert.ChangeType(value, t);
                    return true;
                }
                catch (Exception)
                {
                    if (logError) RuntimeConsole.LogResultError("Cannot parse " + value);
                }
            }

            result = null;
            return false;
        }
    }
}
