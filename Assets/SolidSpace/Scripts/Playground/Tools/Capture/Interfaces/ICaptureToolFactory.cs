using Unity.Entities;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Capture
{
    public interface ICaptureToolFactory
    {
        ICaptureTool Create(ICaptureToolHandler handler, Color gizmosColor, params ComponentType[] requiredComponents);
    }
}