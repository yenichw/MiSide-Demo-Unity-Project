// Copyright (C) 2018 KAMGAM e.U. - All rights reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace kamgam.editor
{
    /// <summary>
    /// Remember that the coroutines will be destroyed with every EditorPlayModeState change.
    /// </summary>
    [InitializeOnLoad]
    public class Coroutines
    {
        static List<IEnumerator> _coroutines;

        static Coroutines()
        {
            _coroutines = new List<IEnumerator>();
            EditorApplication.update += advanceCoroutines;
        }

        /// <summary>
        /// Update loop for the coroutines.
        /// </summary>
        static void advanceCoroutines()
        {
            for (int i = _coroutines.Count() - 1; i >= 0; --i)
            {
                if (_coroutines[i] != null && _coroutines[i].MoveNext())
                {
                    _coroutines[i].MoveNext();
                    _coroutines.RemoveAt(i);
                }
            }
        }

        /// <param name="routine"></param>
        public static void StartCoroutine( IEnumerator routine )
        {
            _coroutines.Add( routine );
        }

        public static void StopCoroutine(IEnumerator routine)
        {
            _coroutines.Remove(routine);
        }

    }
}
#endif
