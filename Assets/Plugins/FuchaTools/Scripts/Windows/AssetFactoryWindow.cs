using UnityEditor;
using UnityEngine;

namespace FuchaTools
{
    internal class AssetFactoryWindow : EditorWindow
    {
        private readonly TypeScannerWindow _scannerWindow;
        private readonly RepaintEventsView _repaintEvents;
        
        private string _outputPath;

        [MenuItem("Assets/Create/Any...", priority = -1)]
        private static void InvokeWindow()
        {
            WindowUtil.OpenToolWindow<AssetFactoryWindow>();
        }

        public AssetFactoryWindow()
        {
            _scannerWindow = new TypeScannerWindow(typeof(ScriptableObject));
            _repaintEvents = new RepaintEventsView(this);
        }

        private void OnEnable()
        {
            _outputPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        }

        private void OnGUI()
        {
            _repaintEvents.OnGUI();
            
            WindowUtil.InitializeWindowPosition(this);

            if (!_scannerWindow.OnGUI(out var selectedType))
            {
                return;
            }

            var asset = ScriptableObjectFactory.CreateAsset(_outputPath, selectedType.Type);
            
            Selection.activeObject = asset;
            
            Close();
        }

        private void OnLostFocus()
        {
            Close();
        }
    }
}
