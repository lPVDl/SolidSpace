using System;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine.Profiling;

namespace SolidSpace.Profiling
{
    public partial class ProfilingManager : IProfilingHandler, IProfilingManager, IController
    {
        public ProfilingTreeReader Reader => new ProfilingTreeReader(_profilingTree);

        private const int MaxRecordCount = (1 << 17) - 2;
        private const int JobStackSize = 64;
        private const string RootNodeName = "_root";
        
        public EControllerType ControllerType => EControllerType.Profiling;

        private readonly ProfilingConfig _config;

        private bool _enableSolidProfiling;
        private bool _enableUnityProfiling;
        private NativeArray<ProfilingRecord> _records;
        private int _recordCount;
        private int _nameCount;
        private string[] _namesActive;
        private string[] _namesPassive;
        private Stopwatch _stopwatch;
        private Stopwatch _buildTreeJobStopwatch;
        private Processor _processor;
        private ProfilingTree _profilingTree;

        public ProfilingManager(ProfilingConfig config)
        {
            _config = config;
            _processor = new Processor { owner = this };
        }

        public void Initialize() => _processor.Initialize();

        public void Update() => _processor.Update();

        public ProfilingHandle GetHandle(object owner)
        {
            if (owner is null) throw new ArgumentNullException(nameof(owner));

            return new ProfilingHandle(this);
        }

        public void OnBeginSample(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            if (_enableUnityProfiling)
            {
                Profiler.BeginSample(name);
            }

            if (!_enableSolidProfiling || _recordCount >= MaxRecordCount)
            {
                return;
            }

            var record = new ProfilingRecord();
            record.Write((int) _stopwatch.ElapsedTicks, true);
            _records[_recordCount++] = record;
            _namesActive[_nameCount++] = name;
        }

        public void OnEndSample()
        {
            if (_enableUnityProfiling)
            {
                Profiler.EndSample();
            }
            
            if (!_enableSolidProfiling || _recordCount >= MaxRecordCount)
            {
                return;
            }

            var record = new ProfilingRecord();
            record.Write((int) _stopwatch.ElapsedTicks, false);
            _records[_recordCount++] = record;
        }

        public void FinalizeObject() => _processor.FinalizeObject();
    }
}