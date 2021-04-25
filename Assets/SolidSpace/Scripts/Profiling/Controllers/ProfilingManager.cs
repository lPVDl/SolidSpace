using System;
using System.Collections.Generic;
using SolidSpace.Profiling.Data;
using SolidSpace.Profiling.Interfaces;
using Unity.Collections;

namespace SolidSpace.Profiling.Controllers
{
    // TODO: Rename profiling to profiler
    public partial class ProfilingManager : IProfilingHandler, IProfilingManager, IInitializable, IFinalazable, IUpdatable
    {
        // TODO: Make editor window search container, add funcs to container to resolve, editor only, remove this field
        public static IProfilingManager Instance { get; private set; }

        // TODO: Rename profiling result to tree
        public ProfilingResultReadOnly Result => new ProfilingResultReadOnly(_profilingResult);
        
        private const int MaxRecordCount = 1 << 17 - 2;
        private const int SamplesPerSecond = 1 << 24;
        private const int JobStackSize = 64;
        private const string RootNodeName = "_root";
        
        public EControllerType ControllerType => EControllerType.Profiling;

        private readonly ProfilingConfig _config;

        private bool _enableSolidProfiling;
        private bool _enableUnityProfiling;
        private NativeArray<ProfilingRecord> _records;
        private int _recordCount;
        private List<string> _namesActive;
        private List<string> _namesPassive;
        private double _frameStartTime;
        private Handler _handler;
        private Processor _processor;
        private ProfilingResult _profilingResult;

        public ProfilingManager(ProfilingConfig config)
        {
            _config = config;
            _handler = new Handler { owner = this };
            _processor = new Processor { owner = this };
        }

        public void Initialize() => _processor.Initialize();

        public void Update() => _processor.Update();

        public ProfilingHandle GetHandle(object owner)
        {
            if (owner is null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            return new ProfilingHandle(this);
        }

        public void OnBeginSample(string name) => _handler.OnBeginSample(name);

        public void OnEndSample(string name) => _handler.OnEndSample(name);

        public void FinalizeObject() => _processor.FinalizeObject();
    }
}