using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;

namespace DropFiles
{
    public class FileInfo : INotifyPropertyChanged
    {
        private string _fileName;
        private string _filePath;
        private BitmapImage _icon;

        public event PropertyChangedEventHandler PropertyChanged;

        // 用于触发 PropertyChanged 事件的辅助方法
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 文件名属性
        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }

        // 文件路径属性
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(FilePath));

                    // 当文件路径改变时，也尝试更新图标
                    UpdateIcon();
                }
            }
        }

        // 文件图标属性
        public BitmapImage Icon
        {
            get { return _icon; }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        // 构造函数，接收文件路径作为参数
        public FileInfo(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
        }

        // 根据文件路径更新图标的方法
        private void UpdateIcon()
        {
            try
            {
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(FilePath))
                {
                    Icon = ConvertIconToBitmapImage(icon);
                }
            }
            catch (Exception ex)
            {
                // 处理异常，比如日志记录或设置默认图标
                Console.WriteLine($"Error loading file icon: {ex.Message}");
                Icon = null; // 或者设置为默认图标
            }
        }

        // 从图标路径提取位图图像的方法
        //private BitmapImage ExtractIcon(string iconPath)
        //{
        //    if (!string.IsNullOrEmpty(iconPath))
        //    {
        //        using (System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(iconPath))
        //        {
        //            using (System.Drawing.Bitmap bitmap = icon.ToBitmap())
        //            {
        //                IntPtr hBitmap = bitmap.GetHbitmap();
        //                BitmapImage bitmapImage = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
        //                    hBitmap,
        //                    IntPtr.Zero,
        //                    Int32Rect.Empty,
        //                    BitmapSizeOptions.FromEmptyOptions());

        //                DeleteObject(hBitmap); // 清理非托管资源

        //                return bitmapImage;
        //            }
        //        }
        //    }
        //    return null;
        //}

        private BitmapImage ConvertIconToBitmapImage(Icon icon)
        {
            using (var bitmap = icon.ToBitmap())
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                //icon.Save(memoryStream);
                memoryStream.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 使图像线程安全

                return bitmapImage;
            }
        }

        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //private static extern bool DeleteObject(IntPtr hObject);
    }
}
