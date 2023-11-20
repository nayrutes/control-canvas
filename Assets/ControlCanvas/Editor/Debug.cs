
#define DEBUG_LEVEL_1
#define DEBUG_LEVEL_2
#define DEBUG_LEVEL_3


namespace ControlCanvas.Editor
{
    public static class Debug
    {
        public static void Log(object s)
        {
#if DEBUG_LEVEL_1
            UnityEngine.Debug.Log(s);
#endif
        }

        public static void LogWarning(object s)
        {
#if DEBUG_LEVEL_2
            UnityEngine.Debug.LogWarning(s);
#endif
        }
        public static void LogError(object s)
        {
#if DEBUG_LEVEL_3
            UnityEngine.Debug.LogError(s);
#endif
        }

    }
}