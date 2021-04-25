using System;
using UnityEngine;

namespace SolidSpace
{
    public partial class GameCycleController
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