using UnityEditor;

namespace FuchaTools
{
    public class EditorWindowFactoryWindow : EditorWindow
    {
        private readonly TypeScannerWindow _scannerWindow;
        private readonly RepaintEventsView _repaintEvents;

        [MenuItem("Window/Open...", priority = -1)]
        private static void OpenWindow()
        {
            WindowUtil.OpenToolWindow<EditorWindowFactoryWindow>();
        }

        public EditorWindowFactoryWindow()
        {
            _scannerWindow = new TypeScannerWindow(typeof(EditorWindow));
            _repaintEvents = new RepaintEventsView(this);
        }

        private void OnGUI()
        {
            _repaintEvents.OnGUI();
            
            WindowUtil.InitializeWindowPosition(this);

            if (!_scannerWindow.OnGUI(out var selectedType))
            {
                return;
            }

            GetWindow(selectedType.Type, false, selectedType.NameShort);
            
            Close();
        }

        private void OnLostFocus()
        {
            Close();
        }
    }
}