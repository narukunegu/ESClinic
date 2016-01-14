using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ESClinic.Framework
{
    public class ImageDataConverter
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr handle);

        public static BitmapSource Bs;
        public static IntPtr Ip;

        public static BitmapSource BitmapToBitmapSource(Bitmap source)
        {
            Ip = source.GetHbitmap();

            Bs = Imaging.CreateBitmapSourceFromHBitmap(Ip, IntPtr.Zero, Int32Rect.Empty,

                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(Ip);

            return Bs;
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static BitmapImage BytesToBitmapImage(byte[] bytes)
        {
            using (MemoryStream memory = new MemoryStream(bytes))
            {
                memory.Seek(0, SeekOrigin.Begin);
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static byte[] BitmapSourceToBytes(BitmapSource bitmapImage)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream memory = new MemoryStream())
            {
                encoder.Save(memory);
                return memory.ToArray();
            }
        }

        public static byte[] BitmapImageToBytes(BitmapImage bitmapImage)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream memory = new MemoryStream())
            {
                encoder.Save(memory);
                return memory.ToArray();
            }
        }

    }
}