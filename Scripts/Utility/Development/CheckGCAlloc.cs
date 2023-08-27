using System;
using Unity.Profiling;
using UnityEngine;

namespace Game.Utility.Development
{

    public sealed class CheckGCAllocScope : IDisposable
    {
#if ENABLE_RELEASE
    public void Dispose()
    {
    }

    public static CheckGCAllocScope Create( string name )
    {
        return default;
    }

#else
        private readonly string m_name;
        private readonly long m_startValue;

        private static ProfilerRecorder m_profilerRecorder;

        private CheckGCAllocScope(string name)
        {
            m_name = name;
            m_startValue = m_profilerRecorder.CurrentValue;
        }

        public void Dispose()
        {
            var endValue = m_profilerRecorder.CurrentValue;
            var value = endValue - m_startValue;

            Debug.Log($"[GC Alloc] {m_name}: {value}");
        }

        public static CheckGCAllocScope Create(string name)
        {
            return new(name);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoadMethod()
        {
            m_profilerRecorder.Dispose();
            m_profilerRecorder = ProfilerRecorder.StartNew
            (
                category: ProfilerCategory.Memory,
                statName: "GC Allocated In Frame"
            );
        }

#endif
    }
}