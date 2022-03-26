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
    public static class ScheduleHelper
    {
        private static readonly List<Action> ToInvoke = new List<Action>();
        private static GlobalScheduleUpdater _updater;
        private static bool _iterating;

        public static void SafeLog(object log)
        {
            SafeInvoke(() => Debug.Log(log));
        }
        /// <summary>
        /// Invokes when it is safe to do so within Unity's single thread.
        /// </summary>
        public static void SafeInvoke(Action toInvoke)
        {
            lock (ToInvoke)
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

                ToInvoke.Add(toInvoke);
            }
        }

        private static void RunSafeInvokes()
        {
            // Skip over, this is being taken care of elsewhere.
            if (_iterating)
                return;

            lock (ToInvoke)
            {
                _iterating = true;
                foreach (var toInvoke in ToInvoke)
                {
                    toInvoke?.Invoke();
                }
                ToInvoke.Clear();
                _iterating = false;
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
