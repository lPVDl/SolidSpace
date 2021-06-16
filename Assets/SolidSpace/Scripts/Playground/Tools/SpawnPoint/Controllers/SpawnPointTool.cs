using System.Collections.Generic;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    internal class SpawnPointTool : ISpawnPointTool
    {
        public IUIManager UIManager { get; set; }
        public IPointerTracker Pointer { get; set; }
        public IToolWindow Window { get; set; }

        public IEnumerable<float2> Update()
        {
            if (UIManager.IsMouseOver || !Pointer.ClickedThisFrame)
            {
                yield break;
            }

            yield return Pointer.Position;
        }

        public void SetEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                UIManager.AddToRoot(Window, "ContainerA");
            }
            else
            {
                UIManager.RemoveFromRoot(Window, "ContainerA");
            }
        }
    }
}