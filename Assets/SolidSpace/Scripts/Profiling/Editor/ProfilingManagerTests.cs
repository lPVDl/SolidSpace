using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SolidSpace.Profiling.Editor
{
    public class ProfilingManagerTests
    {
        private const string DummySampleName = "TestSample";
        
        private ProfilingManager _manager;
        private ProfilingConfig _config;
        private List<ProfilingNodeFriendly> _nodes;
        private int _totalNodeCount;

        [SetUp]
        public void SetUp()
        {
            _nodes = new List<ProfilingNodeFriendly>();
            _config = new ProfilingConfig(true, true);
            _manager = new ProfilingManager(_config);
            _totalNodeCount = 0;
            _manager.Initialize();
        }
        
        [Test]
        public void GetHandle_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.GetHandle(null));
        }

        [Test]
        public void OnBeginSample_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.OnBeginSample(null));
        }

        [Test]
        public void OnEndSample_WithNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.OnEndSample(null));
        }

        [Test]
        public void OnUpdate_WithoutSamples_HasRootOnly()
        {
            UpdateManager_ReadResults();
            Assert.That(_totalNodeCount, Is.EqualTo(1));
        }
        
        [Test]
        public void OnUpdate_WithoutSamples_RootDeepIsZero()
        {
            UpdateManager_ReadResults();
            Assert.That(_nodes[0].deep, Is.EqualTo(0));
        }
        
        [Test]
        public void OnUpdate_WithoutSamples_RootNameIsCorrect()
        {
            UpdateManager_ReadResults(); 
            Assert.That(_nodes[0].name, Is.EqualTo("_root"));
        }

        [Test]
        public void OnUpdate_WithDummy_HasRootAndDummyOnly()
        {
            BeginEndDummySample_UpdateManager_ReadResults(); 
            Assert.That(_totalNodeCount, Is.EqualTo(2));
        }
        
        [Test]
        public void OnUpdate_WithDummy_DummyDeepIsOne()
        {
            BeginEndDummySample_UpdateManager_ReadResults(); 
            Assert.That(_nodes[1].deep, Is.EqualTo(1));
        }
        
        [Test]
        public void OnUpdate_WithDummy_DummyNameIsCorrect()
        {
            BeginEndDummySample_UpdateManager_ReadResults(); 
            Assert.That(_nodes[1].name, Is.EqualTo(DummySampleName));
        }

        private void BeginEndDummySample_UpdateManager_ReadResults()
        {
            _manager.OnBeginSample(DummySampleName);
            _manager.OnEndSample(DummySampleName);
            _manager.Update();

            _manager.Reader.Read(0, 2, _nodes, out _totalNodeCount);
        }

        private void UpdateManager_ReadResults()
        {
            _manager.Update();
            
            _manager.Reader.Read(0, 1, _nodes, out _totalNodeCount);
        }

        [TearDown]
        public void TearDown()
        {
            _manager.FinalizeObject();
        }
    }
}