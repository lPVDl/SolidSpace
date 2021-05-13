using System;
using System.Diagnostics;
using SolidSpace.Debugging;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace SolidSpace.Profiling
{
    public partial class ProfilingManager : IProfilingProcessor, IProfilingManager
    {
        public ProfilingTreeReader Reader => new ProfilingTreeReader(_profilingTree);
        
        private readonly ProfilingConfig _config;

        private bool _enableSolidProfiling;
        private bool _enableUnityProfiling;
        private NativeArray<ProfilingRecord> _records;
        private int _recordCount;
        private int _maxRecordCount;
        private int _nameCount;
        private string[] _namesActive;
        private string[] _namesPassive;
        private Stopwatch _samplesTimer;
        private Stopwatch _buildTreeJobTimer;
        private ProfilingTree _profilingTree;
        private ExceptionHandler _exceptionHandler;

        public ProfilingManager(ProfilingConfig config)
        {
            _config = config;
        }

        public void Initialize()
        {
            _enableSolidProfiling = _config.EnableSolidProfiling;
            _enableUnityProfiling = _config.EnableUnityProfiling;
            _buildTreeJobTimer = new Stopwatch();
            _maxRecordCount = _config.MaxRecordCount;
            _records = new NativeArray<ProfilingRecord>(_maxRecordCount, Allocator.Persistent);
            _recordCount = 0;
            _nameCount = 1;
            _namesActive = new string[_maxRecordCount + 1];
            _namesPassive = new string[_maxRecordCount + 1];
            _namesActive[0] = "_root";
            _namesPassive[0] = "_root";
            _samplesTimer = new Stopwatch();
            _samplesTimer.Start();
            _profilingTree = new ProfilingTree
            {
                childs = NativeArrayUtil.CreateTempJobArray<ushort>(1),
                names = NativeArrayUtil.CreateTempJobArray<ushort>(1),
                siblings = NativeArrayUtil.CreateTempJobArray<ushort>(1),
                times = NativeArrayUtil.CreateTempJobArray<float>(1),
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
                outChilds = NativeArrayUtil.CreateTempJobArray<ushort>(nodeCount),
                outNames = NativeArrayUtil.CreateTempJobArray<ushort>(nodeCount),
                outSiblings = NativeArrayUtil.CreateTempJobArray<ushort>(nodeCount),
                outTimes = NativeArrayUtil.CreateTempJobArray<float>(nodeCount),
                outState = NativeArrayUtil.CreateTempJobArray<TreeBuildState>(1),
                parentStack = NativeArrayUtil.CreateTempJobArray<ushort>(_config.StackSize),
                siblingStack = NativeArrayUtil.CreateTempJobArray<ushort>(_config.StackSize),
                timeStack = NativeArrayUtil.CreateTempJobArray<int>(_config.StackSize),
                nameHashStack = NativeArrayUtil.CreateTempJobArray<int>(_config.StackSize)
            };
            
            Profiler.BeginSample("ProfilingManager.BuildTreeJob");
            _buildTreeJobTimer.Restart();
            job.Schedule().Complete();
            _buildTreeJobTimer.Stop();
            Profiler.EndSample();

            SpaceDebug.LogState("BuildTreeJob ms", _buildTreeJobTimer.ElapsedTicks / (float) Stopwatch.Frequency * 1000);

            _recordCount = 0;

            try
            {
                _exceptionHandler.HandleJobState(_namesActive, job);
            }
            finally
            {
                job.parentStack.Dispose();
                job.siblingStack.Dispose();
                job.timeStack.Dispose();
                job.nameHashStack.Dispose();
                job.outState.Dispose();

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

                _samplesTimer.Restart();
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
            record.Write((int) _samplesTimer.ElapsedTicks, true, name.GetHashCode());
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
            record.Write((int) _samplesTimer.ElapsedTicks, false, name.GetHashCode());
            _records[_recordCount++] = record;
            _namesActive[_nameCount++] = name;
        }

        public void FinalizeObject()
        {
            _records.Dispose();
            _profilingTree.Dispose();
        }

        private void Swap<T>(ref T a, ref T b)
        {
            var t = a;
            a = b;
            b = t;
        }
    }
}