using System;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    internal class UpdatingBehaviour : MonoBehaviour
    {
        public event Action OnUpdate;

        private void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}