using System;
using System.Linq;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class EnumTooltipPickerUI
    {
        public static void Render<T>(T tab, Action<T> setTab, Func<T, string> toString, params GUILayoutOption[] layoutOptions) where T : Enum
        {
            string[] names;
            if (toString == null)
            {
                names = typeof(T).GetEnumNames();
            }
            else
            {
                var vals = typeof(T).GetEnumValues();
                names = new string[vals.Length];
                for (int i = 0; i < names.Length; ++i)
                    names[i] = toString((T)vals.GetValue(i));
            }
            T newOnline = (T) typeof(T).GetEnumValues().GetValue(GUILayout.Toolbar((int)(object)tab, names, layoutOptions));
            if (!newOnline.Equals(tab))
            {
                setTab(newOnline);
            }
        }

        public static void Render<T>(T tab, Action<T> setTab, params GUILayoutOption[] layoutOptions) where T : Enum
        {
            Render(tab, setTab, null, layoutOptions);
        }
    }
}