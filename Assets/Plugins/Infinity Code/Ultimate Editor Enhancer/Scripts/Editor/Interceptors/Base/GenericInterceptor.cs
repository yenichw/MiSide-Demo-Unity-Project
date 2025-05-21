/*           INFINITY CODE          */
/*     https://infinity-code.com    */

namespace InfinityCode.UltimateEditorEnhancer.Interceptors
{
    public abstract class GenericInterceptor<T>: Interceptor where T : GenericInterceptor<T>
    {
        private static T _instance;

        protected static T instance
        {
            get => _instance;
        }

        protected override void Init()
        {
            _instance = (T)this;

            base.Init();
        }
    }
}