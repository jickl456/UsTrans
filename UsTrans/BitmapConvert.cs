using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using Tesseract;

namespace UsTrans
{
    public  class BitmapConvert
    {
        public static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
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

        public static Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource bitmapSource = (BitmapSource)imageSource;
            Bitmap bmp = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bitmapSource.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        // 將 Bitmap 轉換為 Pix 格式，以便 Tesseract 使用
        public static Pix BitmapToPix(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                // 將 Bitmap 保存為 MemoryStream
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                // 使用 Tesseract 的 Pix 將 MemoryStream 轉換為 Pix 格式
                return Pix.LoadFromMemory(ms.ToArray());
            }
        }
    }
}
