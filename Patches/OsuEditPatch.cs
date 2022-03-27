using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CustomBeatmaps.UI.OSUEditMode;
using CustomBeatmaps.Util;
using FMOD.Studio;
using HarmonyLib;
using Rhythm;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomBeatmaps.Patches
{
    /// <summary>
    /// Grant cheats when playing a beatmap (from OSU directory) for editing/testing purposes.
    /// </summary>
    public static class OsuEditorPatch
    {
        private static bool _editMode;
        private static string _editPath;
        private static bool _enableCountdown;

        private static Rhythm.RhythmController _rhythmController;

        private static bool EditMode => _editMode;
        private static bool _paused;

        public static bool AutoReload = true;

        public static int SongTimeMS =>
            _rhythmController != null ? (int)_rhythmController.songTracker.TimelinePosition : 0;

        public static int SongTotalMS { get; private set; }

        private static FileSystemWatcher _fileWatcher;
        
        public static void SetEditMode(bool editMode, bool enableCountdown=true, string path=null)
        {
            ScheduleHelper.SafeLog($"EDIT MODE {editMode}");
            _editMode = editMode;
            _enableCountdown = enableCountdown;
            _editPath = path;
        }

        public static bool IsPaused()
        {
            return _paused;
        }

        public static void SetPaused(bool paused)
        {
            _paused = paused;
            if (!_paused)
            {
                Time.timeScale = 1;
            }
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Awake")]
        [HarmonyPrefix]
        private static void RhythmControllerInit(Rhythm.RhythmController __instance)
        {
            _rhythmController = __instance;

            if (EditMode)
            {
                // Clear previous filewatcher
                if (_fileWatcher != null)
                {
                    _fileWatcher.Dispose();
                    _fileWatcher = null;
                }

                // Add OSU UI
                new GameObject().AddComponent<OSUEditUIBehaviour>();

                // Watch our file for changes/hot swap
                _fileWatcher = FileWatchHelper.WatchFile(_editPath, () =>
                {
                    if (AutoReload)
                    {
                        ScheduleHelper.SafeInvoke(HotReloadBeatmap);
                    }
                });
            }
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Start")]
        [HarmonyPostfix]
        private static void RhythmControllerPostStart()
        {
            SongTotalMS = GetSongDurationEstimate();
        }

        [HarmonyPatch(typeof(Rhythm.RhythmController), "OnDestroy")]
        [HarmonyPostfix]
        private static void RhythmControllerDestroy()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }
        }
        
        // PAUSING

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Update")]
        [HarmonyPrefix]
        private static void RhythmControllerPauseUpdatePre(Rhythm.RhythmController __instance)
        {
            if (!EditMode)
                return;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                bool paused = IsPaused();
                ScheduleHelper.SafeLog($"TOGGLE PAUSE {!paused}");
                SetPaused(!paused);
            }
        }

        [HarmonyPatch(typeof(JeffBezosController), "Update")]
        [HarmonyPrefix]
        private static void JeffBezosPreUpdate()
        {
            if (!EditMode)
                return;
            if (IsPaused())
            {
                Time.timeScale = 0;
                JeffBezosController.approachTimeScale = 0;
                JeffBezosController.atsSpeed = 0;
            }
        }

        [HarmonyPatch(typeof(JeffBezosController), "SetTimeScale")]
        [HarmonyPrefix]
        private static void JeffBezosSetTimeScale(ref bool __runOriginal)
        {
            // Don't set time scale if we're paused, we do that ourselves.
            if (EditMode && IsPaused())
            {
                __runOriginal = false;
            }
        }

        // COUNTDOWN OVERRIDE
        
        [HarmonyPatch(typeof(Rhythm.RhythmController), "Start")]
        [HarmonyPrefix]
        private static void RhythmControllerDisableCountdown(Rhythm.RhythmController __instance)
        {
            // Externally enable/disable countdown
            __instance.enableCountdown = _enableCountdown;
        }

        // INVINCIBILITY

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Miss")]
        [HarmonyPrefix]
        private static void ProcessMiss(ref bool __runOriginal)
        {
            // Don't miss
            if (EditMode)
            {
                __runOriginal = false;
            }
        }

        // RELOAD

        [HarmonyPatch(typeof(Rhythm.RhythmController), "Update")]
        [HarmonyPrefix]
        private static void RhythmControllerReloadUpdatePre(Rhythm.RhythmController __instance)
        {
            if (!EditMode)
                return;
            if (Input.GetKeyDown(KeyCode.R))
            {
                ScheduleHelper.SafeLog("RELOAD");
                HotReloadBeatmap();
            }
        }

        public static void HotReloadBeatmap()
        {
            HotReloadBeatmapInfo();
            SongTotalMS = GetSongDurationEstimate();
            HotReloadAdjustNotesToCurrentTime();
        }

        private static readonly FieldInfo FlipsField =
            typeof(Rhythm.RhythmController).GetField("flips", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo NotesField =
            typeof(Rhythm.RhythmController).GetField("notes", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo CommandsField =
            typeof(Rhythm.RhythmController).GetField("commands", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo NoteGroupField =
            typeof(Rhythm.RhythmController).GetField("noteGroup", BindingFlags.Instance | BindingFlags.NonPublic);
        private static void HotReloadBeatmapInfo()
        {
            // Update base beatmap info after reloading our beatmap
            CustomBeatmapLoadingOverridePatch.SetOverrideBeatmap(CustomPackageHelper.LoadLocalBeatmap(_editPath));
            _rhythmController.parser.beatmapPath = _editPath;
            _rhythmController.parser.ParseBeatmap();
            _rhythmController.beatmap = _rhythmController.parser.beatmap;

            // These are also locally stored based on the beatmap.
            FlipsField.SetValue(_rhythmController, new Queue<FlipInfo>(_rhythmController.beatmap.flips));
            NotesField.SetValue(_rhythmController, new Queue<NoteInfo>(_rhythmController.beatmap.notes));
            CommandsField.SetValue(_rhythmController, new Queue<CommandInfo>(_rhythmController.beatmap.commands));
        }

        private static void HotReloadAdjustNotesToCurrentTime()
        {
            GameObject noteGroup = (GameObject)NoteGroupField.GetValue(_rhythmController);

            RecalculateFlipsAndCamCenter((Queue<FlipInfo>)FlipsField.GetValue(_rhythmController));

            // Kill all notes on screen
            foreach (BaseNote note in noteGroup.GetComponentsInChildren<BaseNote>())
            {
                // Kinda dangerous? Test to make sure this doesn't break something.
                Object.DestroyImmediate(note.gameObject);
            }

            // Force a fixed update to reload
            RhythmFixedUpdate(_rhythmController);
        }

        private static void RecalculateFlipsAndCamCenter(Queue<FlipInfo> flips)
        {
            // Default to right side
            _rhythmController.player.ChangeSide(Side.Right);
            _rhythmController.cameraObject.SetTargetPoint(_rhythmController.rightCameraTargetPoint);
            _rhythmController.cameraIsCentered = false;
            // Parse all flips until we arrive at our current position.
            while (flips.Count > 0 && _rhythmController.songTracker.Position >= (float) flips.Peek().time)
            {
                RhythmUpdateFlips(_rhythmController);
            }
        }

        private static int GetSongDurationEstimate()
        {
            var beatmap = _rhythmController.beatmap;
            if (beatmap.notes.Count != 0)
            {
                return (int)beatmap.notes.Last().time + 1000;
            }
            return 0;
        }
        
        // Exposed methods
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Rhythm.RhythmController), "UpdateFlips")]
        private  static void RhythmUpdateFlips(object instance) => throw new InvalidOperationException("Stub");

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(Rhythm.RhythmController), "FixedUpdate")]
        private static void RhythmFixedUpdate(object instance) => throw new InvalidOperationException("Stub");
    }
}
