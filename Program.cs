using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace WindowRetriever
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }

    internal class MainWindow : Form
    {
        private ListBox windowList;
        private Dictionary<string, IntPtr> windowHandles;

        public MainWindow()
        {
            windowList = new ListBox();
            windowList.Dock = DockStyle.Fill;
            windowList.DoubleClick += WindowList_DoubleClick;
            this.Controls.Add(windowList);
            this.Load += MainWindow_Load;
            this.Text = "ȭ�� �� ������ ���";
            this.Size = new System.Drawing.Size(400, 600);
            windowHandles = new Dictionary<string, IntPtr>();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            EnumWindowsProc enumProc = new EnumWindowsProc(EnumerateWindows);
            EnumWindows(enumProc, IntPtr.Zero);
        }

        private void WindowList_DoubleClick(object sender, EventArgs e)
        {
            string selectedTitle = windowList.SelectedItem.ToString();
            if (windowHandles.ContainsKey(selectedTitle))
            {
                IntPtr hWnd = windowHandles[selectedTitle];
                SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0040);
            }
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private bool EnumerateWindows(IntPtr hWnd, IntPtr lParam)
        {
            if (IsWindowVisible(hWnd))
            {
                GetWindowRect(hWnd, out RECT rect);

                if (IsWindowOutOfBounds(rect))
                {
                    string title = GetWindowTitle(hWnd);
                    if (string.IsNullOrEmpty(title)) title = "[���� ����]";
                    windowList.Items.Add(title);
                    windowHandles[title] = hWnd;
                }
            }
            return true;
        }

        private static bool IsWindowOutOfBounds(RECT rect)
        {
            // ��� ȭ�鿡 ���� �����찡 ȭ�� ���� ������ ���ԵǴ��� Ȯ��
            bool isInsideAnyScreen = Screen.AllScreens.Any(screen =>
                rect.Left >= screen.Bounds.Left &&
                rect.Top >= screen.Bounds.Top &&
                rect.Right <= screen.Bounds.Right &&
                rect.Bottom <= screen.Bounds.Bottom
            );

            // �����찡 � ȭ�鿡�� ������ ���Ե��� ������ true ��ȯ (ȭ�� ���̰ų� ��迡 ���� ����)
            return !isInsideAnyScreen;
        }


        private static string GetWindowTitle(IntPtr hWnd)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(hWnd, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return string.Empty;
        }
    }
}
