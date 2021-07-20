using System;
using System.Diagnostics;
using System.IO;
using SolidSpace.Editor.Utilities;
using SolidSpace.Entities.Health;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace SolidSpace.Entities.Splitting.Editor
{
    public class ShapeFillWindow : EditorWindow
    {
        private Texture2D _inputTexture;
        private string _outputTexturePath;
        private JobMemoryAllocator _jobMemory;
        private Stopwatch _stopwatch;
        
        private void OnGUI()
        {
            _jobMemory ??= new JobMemoryAllocator();
            _stopwatch ??= new Stopwatch();
            
            _inputTexture = (Texture2D) EditorGUILayout.ObjectField("Input Texture", _inputTexture, typeof(Texture2D), false);
            _outputTexturePath = EditorGUILayout.TextField("Output path", _outputTexturePath);

            if (GUILayout.Button("Test Fill"))
            {
                try
                {
                    FillTexture();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    _jobMemory.DisposeAllocations();
                }
            }
        }

        private void TimerBegin()
        {
            _stopwatch.Restart();
        }

        private void TimerEnd(string operationName)
        {
            _stopwatch.Stop();
            var elapsedMs = _stopwatch.ElapsedTicks / (float) Stopwatch.Frequency * 1000;
            Debug.Log($"{operationName}: {elapsedMs}ms");
        }

        private void FillTexture()
        {
            ConsoleUtil.ClearLog();
            
            TimerBegin();
            var pixels = new NativeArray<Color32>(_inputTexture.GetPixels32(), Allocator.TempJob);
            _jobMemory.AddAllocation(pixels);
            
            var textureWidth = _inputTexture.width;
            var textureHeight = _inputTexture.height;

            var frameBits = _jobMemory.CreateNativeArray<byte>(HealthFrameBitsUtil.GetRequiredByteCount(textureWidth, textureHeight));
            HealthFrameBitsUtil.TextureToFrameBits(pixels, textureWidth, textureHeight, frameBits);
            _jobMemory.AddAllocation(frameBits);
            TimerEnd("Convert to bit array");

            var seedJob = new ShapeSeedJob
            {
                inFrameBits = frameBits,
                inFrameSize = new int2(textureWidth, textureHeight),
                outResultCode = _jobMemory.CreateNativeReference<EShapeSeedResult>(),
                outConnections = _jobMemory.CreateNativeArray<byte2>(256),
                outConnectionCount = _jobMemory.CreateNativeReference<int>(),
                outSeedBounds = _jobMemory.CreateNativeArray<ByteBounds>(256),
                outSeedCount = _jobMemory.CreateNativeReference<int>(),
                outSeedMask = _jobMemory.CreateNativeArray<byte>(textureWidth * textureHeight)
            };
            
            TimerBegin();
            seedJob.Schedule().Complete();
            TimerEnd("ShapeFillJob");

            if (seedJob.outResultCode.Value != EShapeSeedResult.Normal)
            {
                Debug.LogError($"ShapeFillJob ended with '{seedJob.outResultCode.Value}'");
                return;
            }
            
            Debug.Log("Seed count: " + seedJob.outSeedCount.Value);
            Debug.Log("Connection count: " + seedJob.outConnectionCount.Value);

            var shapeReadJob = new ShapeReadJob
            {
                inConnections = seedJob.outConnections,
                inOutBounds = seedJob.outSeedBounds,
                inConnectionCount = seedJob.outConnectionCount.Value,
                inSeedCount = seedJob.outSeedCount.Value,
                outShapeCount = _jobMemory.CreateNativeReference<int>(),
                outShapeRootSeeds = _jobMemory.CreateNativeArray<byte>(256),
            };
            
            TimerBegin();
            shapeReadJob.Schedule().Complete();
            TimerEnd("ShapeReadJob");

            var shapeCount = shapeReadJob.outShapeCount.Value;
            Debug.Log("Shape count: " + shapeCount);
            for (var i = 0; i < shapeCount; i++)
            {
                var bounds = shapeReadJob.inOutBounds[i];
                var pos = bounds.min;
                var width = bounds.max.x - bounds.min.x + 1;
                var height = bounds.max.y - bounds.min.y + 1;
                Debug.Log($"Shape at ({pos.x}, {pos.y}) with size ({width}, {height})");
            }

            var exportTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
            var textureSize = textureWidth * textureHeight;
            var exportTextureRaw = exportTexture.GetRawTextureData<ColorRGB24>();
            var colors = new ColorRGB24[]
            {
                new ColorRGB24 {r = 0, g = 0, b = 255},
                new ColorRGB24 {r = 0, g = 255, b = 0},
                new ColorRGB24 {r = 0, g = 255, b = 255},
                new ColorRGB24 {r = 255, g = 0, b = 0},
                new ColorRGB24 {r = 255, g = 0, b = 255},
                new ColorRGB24 {r = 255, g = 255, b = 0},
                new ColorRGB24 {r = 255, g = 255, b = 255},
            };
            for (var i = 0; i < textureSize; i++)
            {
                var maskColor = seedJob.outSeedMask[i];
                exportTextureRaw[i] = maskColor == 0 ? default : colors[maskColor % colors.Length];
            }
            exportTexture.Apply();

            File.WriteAllBytes(_outputTexturePath, exportTexture.EncodeToPNG());

            DestroyImmediate(exportTexture);
        }
    }
}