using System;
using CustomBeatmaps.UISystem;
using UnityEngine;

namespace CustomBeatmaps.UI.OSUEditMode
{
    public class OSUEditUIBehaviour : MonoBehaviour
    {
        private readonly ReaccStore _store = new ReaccStore();

        private void OnGUI()
        {
            Reacc.SetStore(_store);
            OSUEditUI.Render();
        }
    }
}