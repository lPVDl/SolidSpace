using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Entities.Atlases.Editor
{
    internal class AtlasChunk2DConfigDrawer : OdinValueDrawer<AtlasChunk2DConfig>
    {
        private readonly StringBuilder _stringBuilder;
        private readonly Dictionary<AtlasChunk2DConfig, string> _chunkInfoCash;
        
        public AtlasChunk2DConfigDrawer()
        {
            _stringBuilder = new StringBuilder();
            _chunkInfoCash = new Dictionary<AtlasChunk2DConfig, string>();
        }
        
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var hasLabel = label != null;
            var rect = EditorGUILayout.GetControlRect(hasLabel, 60 + (hasLabel ? 20 : 0));
            rect.height = 20;

            if (hasLabel)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
                rect.y += 20;
            }

            var value = ValueEntry.SmartValue;

            var valueName = ObjectNames.NicifyVariableName(nameof(value.itemSize));
            value.itemSize = EditorGUI.IntField(rect, valueName, value.itemSize);

            rect.y += 20;
            valueName = ObjectNames.NicifyVariableName(nameof(value.itemCount));
            value.itemCount = EditorGUI.IntField(rect, valueName, value.itemCount);

            ValueEntry.SmartValue = value;

            rect.y += 20;

            if (!_chunkInfoCash.TryGetValue(value, out var info))
            {
                info = CreateInfoMessage(value);
                _chunkInfoCash[value] = info;
            }
            
            EditorGUI.HelpBox(rect, info, MessageType.Info);
        }

        private string CreateInfoMessage(AtlasChunk2DConfig data)
        {
            _stringBuilder.Clear();

            _stringBuilder.Append(data.itemSize);
            _stringBuilder.Append("x");
            _stringBuilder.Append(data.itemSize);
            _stringBuilder.Append(" size; ");
            
            var itemCount = (int) Math.Ceiling(Math.Sqrt(data.itemCount));
            _stringBuilder.Append(itemCount);
            _stringBuilder.Append("x");
            _stringBuilder.Append(itemCount);
            _stringBuilder.Append(" items; ");

            var chunkSize = itemCount * data.itemSize;
            _stringBuilder.Append(chunkSize);
            _stringBuilder.Append("x");
            _stringBuilder.Append(chunkSize);
            _stringBuilder.Append(" chunk");

            return _stringBuilder.ToString();
        }
    }
}