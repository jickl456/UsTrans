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

namespace UsTrans
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string PgmInfo ="UsTrans Version:"+ Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private bool isSelecting;
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
                this.WindowState = WindowState.Minimized;
                Thread.Sleep(3000);
                // 確保選擇區域有效
                if (isSelecting) return;

                // 計算截圖範圍（從螢幕座標而非 WPF 座標）
                var left = (int)startPoint.X + (int)this.Left;
                var top = (int)startPoint.Y + (int)this.Top;
                var width = (int)(endPoint.X - startPoint.X);
                var height = (int)(endPoint.Y - startPoint.Y);

                // 使用 Graphics.CopyFromScreen 來截取螢幕
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

                // 隱藏選擇區域
                SelectionRectangle.Visibility = Visibility.Collapsed;
                this.WindowState = WindowState.Normal;
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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 開始選擇區域
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                isSelecting = true;
                startPoint = e.GetPosition(this);
                SelectionRectangle.Visibility = Visibility.Visible;
                Canvas.SetLeft(SelectionRectangle, startPoint.X);
                Canvas.SetTop(SelectionRectangle, startPoint.Y);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                // 更新選擇區域的大小
                endPoint = e.GetPosition(this);
                double x = Math.Min(startPoint.X, endPoint.X);
                double y = Math.Min(startPoint.Y, endPoint.Y);
                double width = Math.Abs(startPoint.X - endPoint.X);
                double height = Math.Abs(startPoint.Y - endPoint.Y);

                SelectionRectangle.Width = width;
                SelectionRectangle.Height = height;
                Canvas.SetLeft(SelectionRectangle, x);
                Canvas.SetTop(SelectionRectangle, y);
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // 結束選擇區域
            isSelecting = false;
        }
    }
}
