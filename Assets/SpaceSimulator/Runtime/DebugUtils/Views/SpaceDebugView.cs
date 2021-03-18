using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SpaceSimulator.Runtime.DebugUtils
{
    public class SpaceDebugView : MonoBehaviour
    {
        [SerializeField] private int _fieldHeight;
        [SerializeField] private int _fieldWidth;
        [SerializeField] private float _cashUpdateDelay;
        [SerializeField] private float _forgetDelay;

        private StringBuilder _stringBuilder;
        private float _lastCashTime;
        private Dictionary<string, string> _intCash;

        private void Awake()
        {
            _stringBuilder = new StringBuilder();
            _intCash = new Dictionary<string, string>();
        }

        private void OnGUI()
        {
            var time = Time.time;
            if (time > _lastCashTime + _cashUpdateDelay)
            {
                _lastCashTime = time;
                UpdateAllCash();
            }
            
            var offset = 0;
            DrawValues(_intCash, ref offset);
        }

        private void DrawValues(Dictionary<string, string> values, ref int offset)
        {
            foreach (var item in values)
            {
                var rect = new Rect(0, offset, _fieldWidth, _fieldHeight);
                GUI.TextArea(rect, item.Value);
                offset += _fieldHeight;
            }
        }

        private void UpdateAllCash()
        {
            UpdateCash(_intCash, SpaceDebug.IntStates);
        }

        private void UpdateCash<T>(Dictionary<string, string> cash, IReadOnlyDictionary<string, SpaceDebugValue<T>> source)
        {
            cash.Clear();

            var time = Time.time;
            foreach (var value in source)
            {
                var debugValue = value.Value;

                if (time > debugValue.logTime + _forgetDelay)
                {
                    continue;
                }

                cash[value.Key] = FormatValue(value.Key, debugValue);
            }
        }

        private string FormatValue<T>(string id, SpaceDebugValue<T> value)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(id);
            _stringBuilder.Append(": ");
            _stringBuilder.Append(value.value);
            
            return _stringBuilder.ToString();
        }
    }
}