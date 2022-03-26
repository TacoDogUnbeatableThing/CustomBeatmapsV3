using System.Collections.Generic;

namespace CustomBeatmaps.UISystem
{
    public class ReaccStore
    {
        public readonly Dictionary<string, object> States = new Dictionary<string, object>();
        public readonly Dictionary<string, object[]> EffectDependencies = new Dictionary<string, object[]>();
    }
}
