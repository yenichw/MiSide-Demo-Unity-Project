// based on the original game.Yen Chezky(yenichw)
using UnityEngine;

namespace MagicaCloth
{
    /// <summary>
    /// ???????
    /// </summary>
    public class StopWatch
    {
        private float startTime;
        private float endTime;

        public StopWatch Start()
        {
            startTime = Time.realtimeSinceStartup;
            return this;
        }

        public StopWatch Stop()
        {
            endTime = Time.realtimeSinceStartup;
            return this;
        }

        public float ElapsedSeconds
        {
            get
            {
                return (endTime - startTime);
            }
        }

        public float ElapsedMilliseconds
        {
            get
            {
                return (endTime - startTime) * 1000.0f;
            }
        }
    }
}
