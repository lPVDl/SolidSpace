using System;

namespace SpaceSimulator.Controllers
{
    public partial class UpdatingController
    {
        private class UpdateHandler : SolidMonoBehaviour
        {
            public event Action OnUpdate;

            private void Update()
            {
                OnUpdate?.Invoke();
            }
        }
    }
}