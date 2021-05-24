using System;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    internal class GizmosBehaviour : MonoBehaviour
    {
        public event Action DrawGizmos;

        private bool _isBroken;

        private void OnDrawGizmos()
        {
            if (_isBroken)
            {
                return;
            }

            try
            {
                DrawGizmos?.Invoke();
            }
            catch
            {
                _isBroken = true;

                throw;
            }
        }
    }
}