using System;
using System.Diagnostics;
using System.IO;
using SolidSpace.Editor.Utilities;
using SolidSpace.Entities.Splitting.Enums;
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
            var textureBits = ConvertToBitArray(_inputTexture);
            TimerEnd("Convert to bit array");

            var textureWidth = _inputTexture.width;
            var textureHeight = _inputTexture.height;

            var job = new ShapeFillJob
            {
                _inFrameBits = textureBits,
                _inFrameSize = new int2(textureWidth, textureHeight),
                _outResultCode = _jobMemory.CreateNativeReference<EShapeFillResult>(),
                _outShapeConnections = _jobMemory.CreateNativeArray<byte>(256),
                _outShapeConnectionCount = _jobMemory.CreateNativeReference<int>(),
                _outShapesInfo = _jobMemory.CreateNativeArray<ShapeInfo>(256),
                _outShapeCount = _jobMemory.CreateNativeReference<int>(),
                _outShapesMask = _jobMemory.CreateNativeArray<byte>(_inputTexture.width * _inputTexture.height)
            };
            
            TimerBegin();
            job.Schedule().Complete();
            TimerEnd("ShapeFillJob");

            if (job._outResultCode.Value != EShapeFillResult.Normal)
            {
                Debug.LogError($"ShapeFillJob ended with '{job._outResultCode.Value}'");
                return;
            }
            
            Debug.Log("ShapeCount: " + job._outShapeCount.Value);
            Debug.Log("ConnectionCount: " + job._outShapeConnectionCount.Value);

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
                var maskColor = job._outShapesMask[i];
                exportTextureRaw[i] = maskColor == 0 ? default : colors[maskColor % colors.Length];
            }
            exportTexture.Apply();

            File.WriteAllBytes(_outputTexturePath, exportTexture.EncodeToPNG());

            DestroyImmediate(exportTexture);
        }

        private NativeArray<byte> ConvertToBitArray(Texture2D texture)
        {
            var textureHeight = texture.height;
            var textureWidth = texture.width;
            var bytesPerLine = (int) Math.Ceiling(textureWidth / 8f);
            var textureRaw = texture.GetRawTextureData<ColorRGB24>();
            var bits = _jobMemory.CreateNativeArray<byte>(bytesPerLine * textureHeight);

            for (var y = 0; y < textureHeight; y++)
            {
                var textureOffset = textureWidth * y;
                var bitsOffset = bytesPerLine * y;
                
                for (var x = 0; x < textureWidth; x += 8)
                {
                    var value = 0;

                    for (var j = 0; j < 8 && (x + j < textureWidth); j++)
                    {
                        var color = textureRaw[textureOffset + x + j];
                        var colorSum = Math.Min(1, color.r + color.g + color.b);
                        value |= colorSum << j;
                    }

                    bits[bitsOffset + x / 8] = (byte) value;
                }
            }

            return bits;
        }
    }
}