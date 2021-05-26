using System;
using UnityEngine;

namespace SolidSpace.Playground
{
    [Serializable]
    public class ShipSpawnManagerConfig
    {
        public bool MouseControl => _mouseControl;
        public Texture2D ShipTexture => _shipTexture;
        
        [SerializeField] private bool _mouseControl;
        [SerializeField] private Texture2D _shipTexture;
    }
}