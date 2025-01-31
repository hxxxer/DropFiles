using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace DropFiles
{
    public class FileInfo : INotifyPropertyChanged
    {
        private string _fileName;
        private string _filePath;
        private BitmapImage? _icon;

        public event PropertyChangedEventHandler? PropertyChanged;

        // 用于触发 PropertyChanged 事件的辅助方法
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // 使用 ?. 安全地调用事件
        }

        // 文件名属性
        public string FileName
        {
            get => _fileName;
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
            get => _filePath;
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
        public BitmapImage? Icon
        {
            get => _icon;
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

        private class ShellIcon
        {
            // 导入shell32.dll的ExtractIcon函数
            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

            public static Icon? GetFolderIcon(int index)
            {
                // 从shell32.dll提取文件夹图标 (索引3是标准的关闭文件夹图标)
                IntPtr hIcon = ExtractIcon(IntPtr.Zero, "shell32.dll", index);

                if (hIcon != IntPtr.Zero)
                {
                    return System.Drawing.Icon.FromHandle(hIcon);
                }

                return null;
            }
        }


        // 根据文件路径更新图标的方法
        private void UpdateIcon()
        {
            try
            {
                if (Directory.Exists(FilePath))
                {
                    var icon = ShellIcon.GetFolderIcon(3);
                    Icon = ConvertIconToBitmapImage(icon);
                }
                else if (File.Exists(FilePath))
                {
                    using var icon = System.Drawing.Icon.ExtractAssociatedIcon(FilePath);
                    Icon = ConvertIconToBitmapImage(icon);
                }
                else
                {
                    Icon = null;
                }
            }
            catch (Exception ex)
            {
                // 处理异常，比如日志记录或设置默认图标
                Console.WriteLine($"Error loading file icon: {ex.Message}");
                Icon = null; // 或者设置为默认图标
            }
        }

        private static BitmapImage ConvertIconToBitmapImage(Icon icon)
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
    }
}
