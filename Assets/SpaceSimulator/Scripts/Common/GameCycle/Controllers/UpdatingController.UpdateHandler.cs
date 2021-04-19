using System;
using UnityEngine;

namespace SpaceSimulator
{
    public partial class UpdatingController
    {
        private class UpdateHandler : MonoBehaviour
        {
            public event Action OnUpdate;

            private void Update()
            {
                OnUpdate?.Invoke();
            }
        }
    }
}