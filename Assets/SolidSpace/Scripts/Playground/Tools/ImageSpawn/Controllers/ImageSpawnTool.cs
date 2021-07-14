using System;
using System.IO;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.Splitting;
using SolidSpace.Entities.Splitting.Enums;
using SolidSpace.Entities.World;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Playground.Core;
using SolidSpace.Profiling;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using Object = UnityEngine.Object;

namespace SolidSpace.Playground.Tools.ImageSpawn
{
    public class ImageSpawnTool : IPlaygroundTool
    {
        private readonly IPlaygroundUIManager _playgroundUI;
        private readonly IUIFactory _uiFactory;
        private readonly IUIManager _uiManager;
        private readonly IPointerTracker _pointer;
        private readonly ISpriteColorSystem _spriteSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;

        private IToolWindow _window;
        private IStringField _pathField;
        private JobMemoryAllocator _jobMemory;
        private EntityArchetype _shipArchetype;

        public ImageSpawnTool(IPlaygroundUIManager playgroundUI, IUIFactory uiFactory, IUIManager uiManager, 
            IPointerTracker pointer, ISpriteColorSystem spriteSystem, IHealthAtlasSystem healthSystem, 
            IEntityManager entityManager)
        {
            _playgroundUI = playgroundUI;
            _uiFactory = uiFactory;
            _uiManager = uiManager;
            _pointer = pointer;
            _spriteSystem = spriteSystem;
            _healthSystem = healthSystem;
            _entityManager = entityManager;
        }
        
        public void OnInitialize()
        {
            var shipComponents = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RotationComponent),
                typeof(RectSizeComponent),
                typeof(RectColliderComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent),
                typeof(VelocityComponent),
                typeof(ActorComponent),
                typeof(RigidbodyComponent)
            };
            _shipArchetype = _entityManager.CreateArchetype(shipComponents);
            
            _jobMemory = new JobMemoryAllocator();
            
            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Image");

            _pathField = _uiFactory.CreateStringField();
            _pathField.SetLabel("Path");
            _pathField.SetValue("Ships.png");
            _window.AttachChild(_pathField);
        }

        public void OnUpdate()
        {
            if (_uiManager.IsMouseOver || !_pointer.ClickedThisFrame)
            {
                return;
            }

            ExecuteTool();
            
            _jobMemory.DisposeAllocations();
        }

        private void ExecuteTool()
        {
            var imagePath = Path.Combine(Application.streamingAssetsPath, _pathField.Value);
            if (!TryLoadTexture(imagePath, out var texture))
            {
                return;
            }

            if (texture.width > 256 || texture.height > 256)
            {
                Debug.LogError("Each texture size must be <= 256");
                return;
            }

            var pixels = new NativeArray<Color32>(texture.GetPixels32(), Allocator.TempJob);
            var frameArray = ConvertToBitArray(pixels, texture.width, texture.height);

            var seedJob = new ShapeSeedJob
            {
                inFrameBits = frameArray,
                inFrameSize = new int2(texture.width, texture.height),
                outConnections = _jobMemory.CreateNativeArray<byte2>(256),
                outConnectionCount = _jobMemory.CreateNativeReference<int>(),
                outResultCode = _jobMemory.CreateNativeReference<EShapeSeedResult>(),
                outSeedBounds = _jobMemory.CreateNativeArray<ByteBounds>(256),
                outSeedCount = _jobMemory.CreateNativeReference<int>(),
                outSeedMask = _jobMemory.CreateNativeArray<byte>(texture.width * texture.height),
            };
            seedJob.Schedule().Complete();

            var resultCode = seedJob.outResultCode.Value;
            if (resultCode != EShapeSeedResult.Normal)
            {
                Debug.LogError($"Seed job ended with code '{resultCode}'");
                pixels.Dispose();
                return;
            }

            var readJob = new ShapeReadJob
            {
                inConnections = seedJob.outConnections,
                inConnectionCount = seedJob.outConnectionCount.Value,
                inOutBounds = seedJob.outSeedBounds,
                inSeedCount = seedJob.outSeedCount.Value,
                outShapeCount = _jobMemory.CreateNativeReference<int>(),
                outShapeRootSeeds = _jobMemory.CreateNativeArray<byte>(256),
            };
            readJob.Schedule().Complete();

            var shapeCount = readJob.outShapeCount.Value;
            var handles = _jobMemory.CreateNativeArray<JobHandle>(shapeCount * 2);
            var handleCount = 0;
            
            for (var i = 0; i < shapeCount; i++)
            {
                var bounds = readJob.inOutBounds[i];
                var width = bounds.max.x - bounds.min.x + 1;
                var height = bounds.max.y - bounds.min.y + 1;
                if (width > 32 || height > 32)
                {
                    continue;
                }

                var spriteIndex = _spriteSystem.Allocate(width, height);
                var healthIndex = _healthSystem.Allocate(width * height);

                var posX = (bounds.max.x + bounds.min.x) / 2f - texture.width * 0.5f + _pointer.Position.x;
                var posY = (bounds.max.y + bounds.min.y) / 2f - texture.height * 0.5f + _pointer.Position.y;

                SpawnEntity(new float2(posX, posY), new float2(width, height), spriteIndex, healthIndex);
            }

            pixels.Dispose();
        }

        private void SpawnEntity(float2 position, float2 size, AtlasIndex spriteIndex, AtlasIndex healthIndex)
        {
            var entity = _entityManager.CreateEntity(_shipArchetype);
            
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(entity, new RectSizeComponent
            {
                value = new half2((half)size.x, (half)size.y)
            });
            _entityManager.SetComponentData(entity, new RotationComponent
            {
                value = 0
            });
            _entityManager.SetComponentData(entity, new SpriteRenderComponent
            {
                index = spriteIndex
            });
            _entityManager.SetComponentData(entity, new HealthComponent
            {
                index = healthIndex
            });
            _entityManager.SetComponentData(entity, new ActorComponent
            {
                isActive = false
            });
        }

        private bool TryLoadTexture(string path, out Texture2D texture)
        {
            texture = default;
            
            if (!File.Exists(path))
            {
                Debug.LogError($"File '{path}' does not exist");
                return false;
            }
            
            var bytes = File.ReadAllBytes(path);
            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            Object.Destroy(texture);
            
            if (texture.LoadImage(bytes))
            {
                return true;
            }
            
            Debug.LogError("Failed to load image into texture");

            return false;
        }
        
        private NativeArray<byte> ConvertToBitArray(NativeArray<Color32> pixels, int textureWidth, int textureHeight)
        {
            var bytesPerLine = (int) Math.Ceiling(textureWidth / 8f);
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
                        var color = pixels[textureOffset + x + j];
                        var colorSum = Math.Min(1, color.r + color.g + color.b);
                        value |= colorSum << j;
                    }

                    bits[bitsOffset + x / 8] = (byte) value;
                }
            }

            return bits;
        }

        public void OnActivate(bool isActive)
        {
            _playgroundUI.SetElementVisible(_window, isActive);
        }

        public void OnFinalize()
        {
            
        }
    }
}