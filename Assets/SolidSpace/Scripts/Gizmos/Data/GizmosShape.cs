using UnityEngine;

namespace SolidSpace.Gizmos
{
    internal struct GizmosShape
    {
        public EGizmosShapeType type;

        /// <summary>
        /// <para> Rect: centerX </para>
        /// <para> Line: startX </para>
        /// </summary>
        public float float0;
        
        /// <summary>
        /// <para> Rect: centerY </para>
        /// <para> Line: startY </para>
        /// </summary>
        public float float1;
        
        /// <summary>
        /// <para> Rect: sizeX </para>
        /// <para> Line: endX </para>
        /// </summary>
        public float float2;
        
        /// <summary>
        /// <para> Rect: sizeY </para>
        /// <para> Line: endY </para>
        /// </summary>
        public float float3;

        /// <summary>
        /// <para> Rect: rotation radians </para>
        /// <para> Line: - </para>
        /// </summary>
        public float float4;

        public Color32 color;
    }
}