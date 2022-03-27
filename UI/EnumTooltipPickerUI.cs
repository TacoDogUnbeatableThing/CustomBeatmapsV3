﻿using System;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class EnumTooltipPickerUI
    {
        public static void Render<T>(T tab, Action<T> setTab) where T : Enum
        {
            T newOnline = (T) typeof(T).GetEnumValues().GetValue(GUILayout.Toolbar((int)(object)tab, typeof(T).GetEnumNames()));
            if (!newOnline.Equals(tab))
            {
                setTab(newOnline);
            }
        }
    }
}