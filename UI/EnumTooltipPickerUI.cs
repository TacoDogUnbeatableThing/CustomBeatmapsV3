using System;
using System.Linq;
using UnityEngine;

namespace CustomBeatmaps.UI
{
    public static class EnumTooltipPickerUI
    {
        public static void Render<T>(T tab, Action<T> setTab, Func<T, string> toString) where T : Enum
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

            int index = Toolbar.Render((int) (object) tab, names);
            if (index != -1)
            {
                T newOnline = (T) typeof(T).GetEnumValues().GetValue(index);
                if (!newOnline.Equals(tab))
                {
                    setTab(newOnline);
                }
            }
        }

        public static void Render<T>(T tab, Action<T> setTab) where T : Enum
        {
            Render(tab, setTab, null);
        }
    }
}