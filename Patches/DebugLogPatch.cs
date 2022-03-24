using System;
using HarmonyLib;

namespace CustomBeatmaps.Patches
{
    public static class DebugLogPatch
    {
        public static Action SomethingLogged;
        private static bool _logLock;

        private static readonly object LogMutex = new object();

        [HarmonyPatch("UnityEngine.DebugLogHandler, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "Internal_Log")]
        [HarmonyPrefix]
        private static void OnLogged()
        {
            // Prevent stackoverflow/infinite recursion of we log within "SomethingLogged"
            // Might just want to invoke anyway but pass a boolean for whether this is invoked "within another log invoke" or something
            lock (LogMutex)
            {
                if (_logLock)
                    return;
                _logLock = true;
                SomethingLogged?.Invoke();
                _logLock = false;
            }
        }
        
        
    }
}