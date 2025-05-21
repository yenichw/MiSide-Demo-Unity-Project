/*           INFINITY CODE          */
/*     https://infinity-code.com    */

using System;
using System.Linq;

namespace InfinityCode.UltimateEditorEnhancer
{
    public static class PackageLocator
    {
        private static bool? _hasBurst;
        private static bool? _hasEntities;
        private static bool? _hasZenject;
        
        public static bool hasBurst
        {
            get
            {
                if (_hasBurst.HasValue) return _hasBurst.Value;
                _hasBurst = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("Unity.Burst"));
                return _hasBurst.Value;
            }
        }
        
        public static bool hasZenject
        {
            get
            {
                if (_hasZenject.HasValue) return _hasZenject.Value;
                _hasZenject = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("Zenject"));
                return _hasZenject.Value;
            }
        }

        public static bool hasEntities
        {
            get
            {
                if (_hasEntities.HasValue) return _hasEntities.Value;
                _hasEntities = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith("Unity.Entities"));
                return _hasEntities.Value;
            }
        }
    }
}