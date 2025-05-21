/*           INFINITY CODE          */
/*     https://infinity-code.com    */

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public abstract class StatedInterceptor<T> : GenericInterceptor<T> where T: StatedInterceptor<T>
    {
        public abstract bool state { get; }

        protected override void Patch()
        {
            if (!state) return;
            base.Patch();
        }

        public static void Refresh()
        {
            if (instance.state) instance.Patch();
            else instance.Unpatch();
        }
    }
}