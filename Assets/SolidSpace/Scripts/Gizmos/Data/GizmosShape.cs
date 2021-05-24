using UnityEngine;

namespace SolidSpace.Gizmos
{
    internal struct GizmosShape
    {
        public EGizmosShapeType type;

        /// <summary>
        /// <para> Square: centerX </para>
        /// <para> Line: startX </para>
        /// </summary>
        public float float0;
        
        /// <summary>
        /// <para> Square: centerY </para>
        /// <para> Line: startY </para>
        /// </summary>
        public float float1;
        
        /// <summary>
        /// <para> Square: sizeX </para>
        /// <para> Line: endX </para>
        /// </summary>
        public float float2;
        
        /// <summary>
        /// <para> Square: sizeY </para>
        /// <para> Line: endY </para>
        /// </summary>
        public float float3;

        public Color32 color;
    }
}