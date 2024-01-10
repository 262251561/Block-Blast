using UnityEngine;
using System.Collections;
using System;

namespace TH
{
    public static class GLog
    {
#if UNITY_EDITOR
        public static bool s_isLog = true;
#else
        public static bool s_isLog = true;
#endif

        public static void Log(object msg)
        {
            if (s_isLog)
                Debug.Log(msg.ToString());
        }

        public static void LogError(string msg)
        {
            if (s_isLog)
                Debug.LogError(msg);
        }

        public static void LogWarning(object msg)
        {
            if (s_isLog)
                Debug.LogWarning(msg.ToString());
        }

        public static void LogException(Exception msg)
        {
            if (s_isLog)
                Debug.LogException(msg);
        }

        public static void LogException(object msg)
        {
            LogException(new Exception(msg.ToString()));
        }

        public static void LogFormat(string text, params object[] args)
        {
            if (s_isLog)
                Debug.LogFormat(text, args);
        }
        public static void LogWarningFormat(string text, params object[] args)
        {
            if (s_isLog)
                Debug.LogWarningFormat(text, args);
        }
        public static void LogErrorFormat(string text, params object[] args)
        {
            if (s_isLog)
                Debug.LogErrorFormat(text, args);
        }
    }
}