using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomBeatmaps.UISystem
{
    /**
     * I'm really stupid for trying this
     * but dammit if they're gonna render UI in code,
     * I'm gonna try shoving my own system into this
     */
    public static class Reacc
    {

        private static ReaccStore _currentStore = new ReaccStore();

        private static string GetNewKeyNow(int lineNumber)
        {
            return Environment.StackTrace + $"->{lineNumber}";
        }

        public static void SetStore(ReaccStore store)
        {
            _currentStore = store;
        }

        public static int GetUniqueId([CallerLineNumber] int lineNumber = 0)
        {
            return GetNewKeyNow(lineNumber).GetHashCode();
        }

        public static (T, Action<T>) UseState<T>(T defaultValue, [CallerLineNumber] int lineNumber = 0)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            return UseState(() => defaultValue, lineNumber);
        }

        public static (T, Action<T>) UseState<T>(Func<T> getDefaultValue, [CallerLineNumber] int lineNumber = 0)
        {
            var key = GetNewKeyNow(lineNumber);

            if (!_currentStore.States.ContainsKey(key)) _currentStore.States[key] = getDefaultValue.Invoke();

            var result = (T) _currentStore.States[key];
            void Setter(T t) => _currentStore.States[key] = t;

            return (result, Setter);
        }

        public static void UseEffect(Action onChange, object[] dependencies = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (dependencies == null)
            {
                dependencies = Array.Empty<object>();
            }

            var key = GetNewKeyNow(lineNumber);

            if (!_currentStore.EffectDependencies.ContainsKey(key))
            {
                _currentStore.EffectDependencies[key] = dependencies;
                onChange.Invoke();
            }
            else
            {
                var oldDeps = _currentStore.EffectDependencies[key];
                if (oldDeps.Length != dependencies.Length)
                    onChange.Invoke();
                else
                    for (var i = 0; i < oldDeps.Length; ++i)
                        if (!Equals(oldDeps[i], dependencies[i]))
                        {
                            onChange.Invoke();
                            break;
                        }

                _currentStore.EffectDependencies[key] = dependencies;
            }
        }
    }
}