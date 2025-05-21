/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InfinityCode.UltimateEditorEnhancer.UnityTypes
{
    public static class EditorGUIRef
    {
        private static FieldInfo _activeEditorField;
        private static MethodInfo _doNumberFieldMethod;
        private static MethodInfo _doObjectFieldMethod;
        private static MethodInfo _doObjectFoldoutMethod;
        private static MethodInfo _doPopupMethod;
        private static MethodInfo _doTextFieldMethod;
        private static MethodInfo _dragNumberValueMethod;
        private static MethodInfo _getInspectorTitleBarObjectFoldoutRenderRectMethod;
        private static MethodInfo _hasKeyboardFocusMethod;
        private static MethodInfo _helpIconButtonMethod;
        private static MethodInfo _isEditingTextFieldMethod;
        private static FieldInfo _recycledEditorField;
        private static MethodInfo _scrollableTextAreaInternalMethod;

        private static FieldInfo activeEditorField
        {
            get
            {
                if (_activeEditorField == null) _activeEditorField = type.GetField("activeEditor", Reflection.StaticLookup);
                return _activeEditorField;
            }
        }

        public static MethodInfo doNumberFieldMethod
        {
            get
            {
                if (_doNumberFieldMethod == null)
                {
                    Type[] parameters = {
                        RecycledTextEditorRef.type,
                        typeof(Rect),
                        typeof(Rect),
                        typeof(int),
#if !UNITY_2021_2_OR_NEWER
                        typeof(bool),
                        typeof(double).MakeByRefType(),
                        typeof(long).MakeByRefType(),
#else
                        EditorGUINumberFieldValueRef.type.MakeByRefType(),
#endif
                        typeof(string),
                        typeof(GUIStyle),
                        typeof(bool),
                        typeof(double)
                    };

                    _doNumberFieldMethod = type.GetMethod("DoNumberField", Reflection.StaticLookup, null, parameters, null);
                }

                return _doNumberFieldMethod;
            }
        }

        public static MethodInfo doObjectFieldMethod
        {
            get
            {
                if (_doObjectFieldMethod == null)
                {
                    _doObjectFieldMethod = type.GetMethods(Reflection.StaticLookup)
                        .Where(m => m.Name == "DoObjectField")
                        .OrderByDescending(m => m.GetParameters().Length)
                        .FirstOrDefault();
                }
                
                return _doObjectFieldMethod;
            }
        }

        private static MethodInfo doObjectFoldoutMethod
        {
            get
            {
                if (_doObjectFoldoutMethod == null)
                {
                    _doObjectFoldoutMethod = Reflection.GetMethod(type, "DoObjectFoldout", new[]
                    {
                        typeof(bool), typeof(Rect), typeof(Rect), typeof(Object[]), typeof(int)
                    }, Reflection.StaticLookup);
                }

                return _doObjectFoldoutMethod;
            }
        }

        public static MethodInfo doPopupMethod
        {
            get
            {
                if (_doPopupMethod == null)
                {
                    Type[] parameters = {
                        typeof(Rect),
                        typeof(int),
                        typeof(int),
                        typeof(GUIContent[]),
                        typeof(Func<int, bool>),
                        typeof(GUIStyle)
                    };
                    _doPopupMethod = type.GetMethod("DoPopup", Reflection.StaticLookup, null, parameters, null);
                }

                return _doPopupMethod;
            }
        }

        public static MethodInfo doTextFieldMethod
        {
            get
            {
                if (_doTextFieldMethod == null)
                {
                    _doTextFieldMethod = type.GetMethod(
                        "DoTextField",
                        Reflection.StaticLookup,
                        null,
                        new[]
                        {
                            RecycledTextEditorRef.type, // editor
                            typeof(int), // id
                            typeof(Rect), // position
                            typeof(string), // text
                            typeof(GUIStyle), // style
                            typeof(string), // allowedletters
                            typeof(bool).MakeByRefType(), // changed
                            typeof(bool), // reset
                            typeof(bool), // multiline
                            typeof(bool) // passwordField
#if UNITY_2021_2_OR_NEWER
                            , typeof(GUIStyle), // cancelButtonStyle
                            typeof(bool) // checkTextLimit
#endif
                        },
                        null
                    );
                }
                return _doTextFieldMethod;
            }
        }

        private static MethodInfo dragNumberValueMethod
        {
            get
            {
                if (_dragNumberValueMethod == null)
                {
                    _dragNumberValueMethod = type.GetMethod(
                        "DragNumberValue",
                        Reflection.StaticLookup,
                        null,
                        new[]
                        {
                            typeof(Rect),
                            typeof(int),
                            typeof(bool),
                            typeof(double).MakeByRefType(),
                            typeof(long).MakeByRefType(),
                            typeof(double)
                        },
                        null
                    );
                }
                return _dragNumberValueMethod;
            }
        }

        private static MethodInfo getInspectorTitleBarObjectFoldoutRenderRectMethod
        {
            get
            {
                if (_getInspectorTitleBarObjectFoldoutRenderRectMethod == null) _getInspectorTitleBarObjectFoldoutRenderRectMethod = type.GetMethod("GetInspectorTitleBarObjectFoldoutRenderRect", Reflection.StaticLookup, null, new[] { typeof(Rect), typeof(GUIStyle) }, null);
                return _getInspectorTitleBarObjectFoldoutRenderRectMethod;
            }
        }

        private static MethodInfo hasKeyboardFocusMethod
        {
            get
            {
                if (_hasKeyboardFocusMethod == null)
                {
                    _hasKeyboardFocusMethod = type.GetMethod(
                        "HasKeyboardFocus",
                        Reflection.StaticLookup,
                        null,
                        new[] { typeof(int) },
                        null);
                }
                return _hasKeyboardFocusMethod;
            }
        }

        public static MethodInfo helpIconButtonMethod
        {
            get
            {
                if (_helpIconButtonMethod == null)
                {
                    Type[] parameters = {
                        typeof(Rect),
                        typeof(Object[])
                    };
                    _helpIconButtonMethod = type.GetMethod("HelpIconButton", Reflection.StaticLookup, null, parameters, null);
                }

                return _helpIconButtonMethod;
            }
        }

        private static MethodInfo isEditingTextFieldMethod
        {
            get
            {
                if (_isEditingTextFieldMethod == null)
                {
                    _isEditingTextFieldMethod = Reflection.GetMethod(type, "IsEditingTextField", Reflection.StaticLookup);
                }

                return _isEditingTextFieldMethod;
            }
        }

        private static FieldInfo recycledEditorField
        {
            get
            {
#if UNITY_6000_0_OR_NEWER
                _recycledEditorField = type.GetField("s_RecycledEditorInternal", Reflection.StaticLookup);
#else
                _recycledEditorField = type.GetField("s_RecycledEditor", Reflection.StaticLookup);
#endif

                return _recycledEditorField;
            }
        }

        public static MethodInfo scrollableTextAreaInternalMethod
        {
            get
            {
                if (_scrollableTextAreaInternalMethod == null)
                {
                    _scrollableTextAreaInternalMethod = Reflection.GetMethod(type, "ScrollableTextAreaInternal", new[]
                    {
                        typeof(Rect), 
                        typeof(string), 
                        typeof(Vector2).MakeByRefType(), 
                        typeof(GUIStyle)
                    }, Reflection.StaticLookup);
                }
                
                return _scrollableTextAreaInternalMethod;
            }
        }

        private static Type type => typeof(EditorGUI);

        public static object GetRecycledEditor()
        {
            return recycledEditorField.GetValue(null);
        }


        public static bool IsEditingTextField()
        {
            return (bool)isEditingTextFieldMethod.Invoke(null, new object[0]);
        }
        
        public static string ScrollableTextAreaInternal(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style)
        {
            object[] parameters = new object[]
            {
                position, text, scrollPosition, style
            };
            string result = (string)scrollableTextAreaInternalMethod.Invoke(null, parameters);
            scrollPosition = (Vector2) parameters[2];
            return result;
        }

        public struct NumberFieldValue
        {
            public bool isDouble;
            public double doubleVal;
            public long longVal;
            public object expression;
            public bool success;

            public NumberFieldValue(double v)
            {
                this.isDouble = true;
                this.doubleVal = v;
                this.longVal = 0L;
                this.expression = null;
                this.success = false;
            }

            public NumberFieldValue(long v)
            {
                this.isDouble = false;
                this.doubleVal = 0.0;
                this.longVal = v;
                this.expression = null;
                this.success = false;
            }

            public bool hasResult
            {
                get
                {
                    return this.success || this.expression != null;
                }
            }
        }
    }
}