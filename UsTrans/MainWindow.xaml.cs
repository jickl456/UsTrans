using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Gma.System.MouseKeyHook;

namespace UsTrans
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string PgmInfo ="UsTrans Version:"+ Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private bool isSelecting;
        private IKeyboardMouseEvents globalMouseEvents;
        private System.Windows.Point startPoint;
        private System.Windows.Point endPoint;
        public MainWindow()
        {
            InitializeComponent();
            PgmTitle.Content = PgmInfo;
            App.main = this;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("確定要關閉嗎？", "關閉UsTrans", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
                App.Current.Shutdown();
            }
        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrtScrBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedMode.Text))
                MessageBox.Show("請選擇模式");
            else if (SelectedMode.Text == AreaScreen.Content.ToString()) //區域Mode
            {
                SubscribeGlobalMouseEvents();
                this.WindowState = WindowState.Minimized;
                MessageBox.Show("請選擇截圖範圍");
                isSelecting = true; // 設定選取模式
            }
            else //全屏Mode
            {
                this.WindowState = WindowState.Minimized;
                using (var bitmap = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                    }

                    // 將 Bitmap 轉換為 BitmapImage
                    var bitmapImage = ConvertBitmapToBitmapImage(bitmap);
                    ScreenshotImage.Source = bitmapImage; // 顯示在 Image 控件上
                }
                this.WindowState = WindowState.Normal;
            }
        }
        #region BitMap轉換 Image
        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 確保流在加載後關閉
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 鎖定以提高性能

                return bitmapImage;
            }
        }
        #endregion

        private void CaptureScreen()
        {
            // 計算截圖範圍
            int left = (int)startPoint.X;
            int top = (int)startPoint.Y;
            int width = (int)(endPoint.X - startPoint.X);
            int height = (int)(endPoint.Y - startPoint.Y);

            // 使用 Graphics.CopyFromScreen 截取螢幕
            try
            {
                using (var bitmap = new Bitmap(width, height))
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(left, top, 0, 0, bitmap.Size);
                    }

                    // 將 Bitmap 轉換為 BitmapImage
                    var bitmapImage = ConvertBitmapToBitmapImage(bitmap);
                    ScreenshotImage.Source = bitmapImage; // 顯示在 Image 控件上
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("擷取發生錯誤 :"+ex.Message);
            }
        } //擷取範圍屏幕
        #region 全局滑鼠事件Hook
        private void SubscribeGlobalMouseEvents()
        {
            globalMouseEvents = Hook.GlobalEvents(); // 訂閱全局滑鼠事件
            globalMouseEvents.MouseDownExt += GlobalMouseEvents_MouseDownExt; // 監聽滑鼠按下事件
            globalMouseEvents.MouseMove += GlobalMouseEvents_MouseMove; // 監聽滑鼠移動事件
            globalMouseEvents.MouseUpExt += GlobalMouseEvents_MouseUpExt; // 監聽滑鼠放開事件
        }

        private void UnsubscribeGlobalMouseEvents()
        {
            if (globalMouseEvents == null) return;
            globalMouseEvents.MouseDownExt -= GlobalMouseEvents_MouseDownExt;
            globalMouseEvents.MouseMove -= GlobalMouseEvents_MouseMove;
            globalMouseEvents.MouseUpExt -= GlobalMouseEvents_MouseUpExt;
            globalMouseEvents.Dispose();
            globalMouseEvents = null;
        }

        private void GlobalMouseEvents_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (isSelecting)
            {
                // 紀錄滑鼠按下的位置作為開始點
                startPoint = new System.Windows.Point(e.X, e.Y);
                Console.WriteLine($"開始選取點: {startPoint.X}, {startPoint.Y}");
            }
        }

        private void GlobalMouseEvents_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isSelecting)
            {
                // 當滑鼠移動時，更新選取的終點
                endPoint = new System.Windows.Point(e.X, e.Y);
                Console.WriteLine($"滑鼠移動點: {endPoint.X}, {endPoint.Y}");
            }
        }

        private void GlobalMouseEvents_MouseUpExt(object sender, MouseEventExtArgs e)
        {
            if (isSelecting)
            {
                // 當滑鼠放開時，停止選取並開始截圖
                endPoint = new System.Windows.Point(e.X, e.Y);
                Console.WriteLine($"結束選取點: {endPoint.X}, {endPoint.Y}");

                isSelecting = false; // 停止選取
                CaptureScreen(); // 截圖選取範圍
                UnsubscribeGlobalMouseEvents(); // 停止監聽全局滑鼠事件
                this.WindowState = WindowState.Normal; // 還原 WPF 窗口
            }
        } 
        #endregion
    }
}
