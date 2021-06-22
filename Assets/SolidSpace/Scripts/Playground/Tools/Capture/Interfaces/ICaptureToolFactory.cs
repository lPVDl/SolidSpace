using Unity.Entities;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Capture
{
    public interface ICaptureToolFactory
    {
        ICaptureTool Create(ICaptureToolHandler handler, params ComponentType[] requiredComponents);
    }
}