using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace UsTrans
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static NotifyIcon trayIcon;
        public static bool CloseMainWin = false;
        public static Window main { get; set; }
        #region [===防止程式開啟第二次===]
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (ps != null && ps.Length > 1)
            {
                System.Windows.MessageBox.Show("【Trans系統】執行中！", "提示訊息", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(1);
            }
            RemoveTrayIcon();
            AddTrayIcon();
        }
        #endregion
        private void AddTrayIcon()
        {
            if (trayIcon != null)
                return;

            trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("trans.ico"),
                Text = "UsTrans"
            };
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
            ContextMenu menu = new ContextMenu();
            MenuItem openItem = new MenuItem { Text = "開啟 open" };
            MenuItem closeItem = new MenuItem { Text = "結束 close" };
            openItem.Click += TrayIcon_DoubleClick;
            closeItem.Click += CloseItem_Click;
            menu.MenuItems.Add(openItem);
            menu.MenuItems.Add(closeItem);
            trayIcon.ContextMenu = menu;
        }

        private void CloseItem_Click(object sender, EventArgs e)
        {
            main.Close();
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            main.WindowState = WindowState.Normal;
            main.Activate();
        }

        public static void RemoveTrayIcon()
        {
            if (trayIcon == null)
                return;

            trayIcon.Visible = false;
            trayIcon.Dispose();
            trayIcon = null;
        }
    }
}
