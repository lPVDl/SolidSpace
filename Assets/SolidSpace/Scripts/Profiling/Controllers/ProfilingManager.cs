using System;
using System.Diagnostics;
using SolidSpace.DebugUtils;
using SolidSpace.Profiling.Enums;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace SolidSpace.Profiling
{
    public partial class ProfilingManager : IProfilingHandler, IProfilingManager, IController
    {
        public ProfilingTreeReader Reader => new ProfilingTreeReader(_profilingTree);

        private const int JobStackSize = 64;
        private const string RootNodeName = "_root";

        public EControllerType ControllerType => EControllerType.Profiling;

        private readonly ProfilingConfig _config;

        private bool _enableSolidProfiling;
        private bool _enableUnityProfiling;
        private NativeArray<ProfilingRecord> _records;
        private int _recordCount;
        private int _maxRecordCount;
        private int _nameCount;
        private string[] _namesActive;
        private string[] _namesPassive;
        private Stopwatch _stopwatch;
        private Stopwatch _buildTreeJobStopwatch;
        private ProfilingTree _profilingTree;
        private NativeArrayUtil _arrayUtil;
        private ErrorHandler _errorHandler;

        public ProfilingManager(ProfilingConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            _enableSolidProfiling = _config.EnableSolidProfiling;
            _enableUnityProfiling = _config.EnableUnityProfiling;
            _buildTreeJobStopwatch = new Stopwatch();
            _maxRecordCount = _config.MaxRecordCount;
            _records = new NativeArray<ProfilingRecord>(_maxRecordCount, Allocator.Persistent);
            _recordCount = 0;
            _nameCount = 1;
            _namesActive = new string[_maxRecordCount + 1];
            _namesPassive = new string[_maxRecordCount + 1];
            _namesActive[0] = RootNodeName;
            _namesPassive[0] = RootNodeName;
            _stopwatch = new Stopwatch();
            _profilingTree = new ProfilingTree
            {
                childs = _arrayUtil.CreateTempJobArray<ushort>(1),
                names = _arrayUtil.CreateTempJobArray<ushort>(1),
                siblings = _arrayUtil.CreateTempJobArray<ushort>(1),
                times = _arrayUtil.CreateTempJobArray<float>(1),
                text = _namesActive
            };
            _profilingTree.times[0] = 0;
            _profilingTree.childs[0] = 0;
            _profilingTree.names[0] = 0;
            _profilingTree.siblings[0] = 0;
        }

        public void Update()
        {
            _profilingTree.Dispose();

            var nodeCount = _recordCount / 2 + _recordCount % 2 + 1;

            var job = new ProfilingBuildTreeJob
            {
                inRecords = _records,
                inRecordCount = _recordCount,
                inFrequency = Stopwatch.Frequency,
                outChilds = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                outNames = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                outSiblings = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                outTimes = _arrayUtil.CreateTempJobArray<float>(nodeCount),
                outStatus = _arrayUtil.CreateTempJobArray<EProfilingBuildTreeStatus>(1),
                outStackLast = _arrayUtil.CreateTempJobArray<int>(1),
                parentStack = _arrayUtil.CreateTempJobArray<ushort>(JobStackSize),
                siblingStack = _arrayUtil.CreateTempJobArray<ushort>(JobStackSize),
                timeStack = _arrayUtil.CreateTempJobArray<int>(JobStackSize)
            };

            var timer = _buildTreeJobStopwatch;
            Profiler.BeginSample("ProfilingManager.BuildTreeJob");
            timer.Reset();
            timer.Start();
            job.Schedule().Complete();
            timer.Stop();
            Profiler.EndSample();

            SpaceDebug.LogState("BuildTreeJob ms", timer.ElapsedTicks / (float) Stopwatch.Frequency * 1000);

            _recordCount = 0;

            try
            {
                _errorHandler.HandleJobState(_namesActive, job);
            }
            finally
            {
                job.parentStack.Dispose();
                job.siblingStack.Dispose();
                job.timeStack.Dispose();
                job.outStatus.Dispose();
                job.outStackLast.Dispose();

                _profilingTree = new ProfilingTree
                {
                    childs = job.outChilds,
                    names = job.outNames,
                    siblings = job.outSiblings,
                    times = job.outTimes,
                    text = _namesActive
                };
                
                Swap(ref _namesActive, ref _namesPassive);
                _nameCount = 1;

                _stopwatch.Reset();
                _stopwatch.Start();
            }
        }

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

            if (!_enableSolidProfiling)
            {
                return;
            }

            if (_recordCount >= _maxRecordCount)
            {
                var message = $"Too many records ({_maxRecordCount}). Try adjusting record count in the config.";
                throw new OutOfMemoryException(message);
            }

            var record = new ProfilingRecord();
            record.Write((int) _stopwatch.ElapsedTicks, true);
            _records[_recordCount++] = record;
            _namesActive[_nameCount++] = name;
        }

        public void OnEndSample(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            if (_enableUnityProfiling)
            {
                Profiler.EndSample();
            }

            if (!_enableSolidProfiling)
            {
                return;
            }

            if (_recordCount >= _maxRecordCount)
            {
                var message = $"Too many records ({_maxRecordCount}). Adjust record count in the config.";
                throw new OutOfMemoryException(message);
            }

            var record = new ProfilingRecord();
            record.Write((int) _stopwatch.ElapsedTicks, false);
            _records[_recordCount++] = record;
        }

        public void FinalizeObject()
        {
            _records.Dispose();
            _profilingTree.Dispose();
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            var t = a;
            a = b;
            b = t;
        }
    }
}