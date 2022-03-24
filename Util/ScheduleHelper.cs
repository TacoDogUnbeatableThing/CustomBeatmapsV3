using System;
using System.Collections.Generic;
using CustomBeatmaps.Patches;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomBeatmaps.Util
{
    /// <summary>
    /// Lets us schedule asynchronous things in Unity
    /// </summary>
    public class ScheduleHelper
    {
        private static readonly List<Action> _toInvoke = new List<Action>();
        private static GlobalScheduleUpdater _updater;

        public static void SafeLog(object log)
        {
            SafeInvoke(() => Debug.Log(log));
        }
        /// <summary>
        /// Invokes on the next Unity update call
        /// </summary>
        public static void SafeInvoke(Action toInvoke)
        {
            // Make sure we have an updater
            if (_updater == null)
            {
                GameObject updaterObject = new GameObject("Global Updater (BepinEx Mod)");
                _updater = updaterObject.AddComponent<GlobalScheduleUpdater>();
                Object.DontDestroyOnLoad(updaterObject);
                // Hook up to receive static logging, to make invoking SYNC UP with Debug.Log
                DebugLogPatch.SomethingLogged += RunSafeInvokes;
            }

            // Kinda redundant but whatever
            if (!_updater.enabled)
                _updater.enabled = true;
            if (!_updater.gameObject.activeSelf)
                _updater.gameObject.SetActive(true);

            lock (_toInvoke)
            {
                _toInvoke.Add(toInvoke);
            }
        }

        private static void RunSafeInvokes()
        {
            lock (_toInvoke)
            {
                foreach (var toInvoke in _toInvoke)
                {
                    toInvoke?.Invoke();
                }
                _toInvoke.Clear();
            }
        }

        private class GlobalScheduleUpdater : MonoBehaviour
        {
            private void Awake() => RunSafeInvokes();
            private void Start() => RunSafeInvokes();
            private void Update() => RunSafeInvokes();
            private void FixedUpdate() => RunSafeInvokes();
            private void LateUpdate() => RunSafeInvokes();
            private void OnGUI() => RunSafeInvokes();
        }
    }
}
