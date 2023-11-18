using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomColorDialog
{
    public class DiktorColorDialog : ColorDialog
    {
        // BY DIKTOR

        public event Action<Color> CurrentColorEvent;
        public Color CurrentColor = Color.Black;
        public DialogResult DialogResultAsync = new DialogResult();
        public bool DialogClosedAsync = false;

        private const int GA_ROOT = 2;
        private const int WM_CTLCOLOREDIT = 0x133;

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hWnd, int gaFlags);

        private List<ApiWindow> EditWindows;

        public DiktorColorDialog()
        {
            FullOpen = true;
        }

        public async void ShowDialogAsync()
            => await Task.Run(() =>
            {
                DialogClosedAsync = false;
                DialogResultAsync = new DialogResult();

                DialogResultAsync = this.ShowDialog();
                DialogClosedAsync = true;
            });


        /*protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_CTLCOLOREDIT:
                    if (EditWindows == null)
                    {
                        IntPtr mainWindow = GetAncestor(hWnd, GA_ROOT);
                        if (mainWindow != IntPtr.Zero)
                        {
                            EditWindows = new List<ApiWindow>(new WindowsEnumerator().GetChildWindows(mainWindow, "Edit"));
                        }
                    }

                    if (EditWindows != null && EditWindows.Count == 6)
                    {
                        string strRed = WindowsEnumerator.WindowText(EditWindows[3].hWnd);
                        string strGreen = WindowsEnumerator.WindowText(EditWindows[4].hWnd);
                        string strBlue = WindowsEnumerator.WindowText(EditWindows[5].hWnd);

                        if (int.TryParse(strRed, out int Red) &&
                            int.TryParse(strGreen, out int Green) &&
                            int.TryParse(strBlue, out int Blue))
                        {
                            CurrentColor?.Invoke(Color.FromArgb(Red, Green, Blue));
                        }
                    }
                    break;
            }

            return base.HookProc(hWnd, msg, wParam, lParam);
        }*/

        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_CTLCOLOREDIT:
                    if (EditWindows == null)
                    {
                        IntPtr mainWindow = GetAncestor(hWnd, GA_ROOT);
                        if (mainWindow != IntPtr.Zero)
                        {
                            EditWindows = new List<ApiWindow>(new WindowsEnumerator().GetChildWindows(mainWindow.ToInt32(), "Edit"));
                        }
                    }

                    // Uncomment the following code to handle color changes in real-time
                    string strRed = WindowsEnumerator.WindowText(EditWindows[3].hWnd);
                    string strGreen = WindowsEnumerator.WindowText(EditWindows[4].hWnd);
                    string strBlue = WindowsEnumerator.WindowText(EditWindows[5].hWnd);

                    if (int.TryParse(strRed, out int Red) &&
                        int.TryParse(strGreen, out int Green) &&
                        int.TryParse(strBlue, out int Blue))
                    {
                        bool event_flag = ((CurrentColor.R != Red) || (CurrentColor.G != Green) || (CurrentColor.B != Blue)); // нужно ли обновлять значение
                        if (event_flag)
                        {
                            CurrentColor = Color.FromArgb(Red, Green, Blue);
                            CurrentColorEvent?.Invoke(CurrentColor);
                        }
                    }
                    break;
            }

            return base.HookProc(hWnd, msg, wParam, lParam);
        }
    }

    public class ApiWindow
    {
        public IntPtr hWnd;
        public string ClassName;
        public string MainWindowTitle;
    }

    public class WindowsEnumerator
    {
        private delegate int EnumCallBackDelegate(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        private static extern int EnumWindows(EnumCallBackDelegate lpEnumFunc, int lParam);

        [DllImport("user32.dll")]
        private static extern int EnumChildWindows(IntPtr hWndParent, EnumCallBackDelegate lpEnumFunc, int lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int IsWindowVisible(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int GetParent(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, StringBuilder lParam);

        private List<ApiWindow> _listChildren = new List<ApiWindow>();
        private List<ApiWindow> _listTopLevel = new List<ApiWindow>();

        private string _topLevelClass = string.Empty;
        private string _childClass = string.Empty;

        public ApiWindow[] GetTopLevelWindows()
        {
            EnumWindows(EnumWindowProc, 0);
            return _listTopLevel.ToArray();
        }

        public ApiWindow[] GetTopLevelWindows(string className)
        {
            _topLevelClass = className;
            return GetTopLevelWindows();
        }

        public ApiWindow[] GetChildWindows(int hwnd)
        {
            _listChildren.Clear();
            EnumChildWindows((IntPtr)hwnd, EnumChildWindowProc, 0);
            return _listChildren.ToArray();
        }

        public ApiWindow[] GetChildWindows(int hwnd, string childClass)
        {
            _childClass = childClass;
            return GetChildWindows(hwnd);
        }

        private int EnumWindowProc(IntPtr hwnd, int lParam)
        {
            if (GetParent(hwnd) == 0 && IsWindowVisible(hwnd) != 0)
            {
                ApiWindow window = GetWindowIdentification(hwnd);
                if (string.IsNullOrEmpty(_topLevelClass) || window.ClassName.ToLower() == _topLevelClass.ToLower())
                {
                    _listTopLevel.Add(window);
                }
            }
            return 1;
        }

        private int EnumChildWindowProc(IntPtr hwnd, int lParam)
        {
            ApiWindow window = GetWindowIdentification(hwnd);
            if (string.IsNullOrEmpty(_childClass) || window.ClassName.ToLower() == _childClass.ToLower())
            {
                _listChildren.Add(window);
            }
            return 1;
        }

        private ApiWindow GetWindowIdentification(IntPtr hwnd)
        {
            StringBuilder classBuilder = new StringBuilder(64);
            GetClassName(hwnd, classBuilder, 64);

            ApiWindow window = new ApiWindow
            {
                ClassName = classBuilder.ToString(),
                MainWindowTitle = WindowText(hwnd),
                hWnd = hwnd
            };

            return window;
        }

        public static string WindowText(IntPtr hwnd)
        {
            const int W_GETTEXT = 0xD;
            const int W_GETTEXTLENGTH = 0xE;

            StringBuilder SB;
            int length = SendMessage(hwnd, W_GETTEXTLENGTH, 0, 0);
            if (length > 0)
            {
                SB = new StringBuilder(length + 1);
                SendMessage(hwnd, W_GETTEXT, SB.Capacity, SB);
            }
            else
            {
                SB = new StringBuilder(0);
            }
            return SB.ToString();
        }
    }
}
