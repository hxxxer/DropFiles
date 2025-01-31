using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace DropFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<FileInfo> Files { get; set; } = [];
        private ListBoxSelectionBehavior FileListExtend;

        public MainWindow()
        {
            InitializeComponent();
            FileList.ItemsSource = Files;
            Files.CollectionChanged += FileList_SelectionTo0;

            FileListExtend = new ListBoxSelectionBehavior(FileList, Files);

        }

        // 当拖动操作进入或移动过窗口时触发此事件
        private void Window_PreviewDragEnter(object sender, DragEventArgs e)
        {
            //if (_dropDone) Debug.WriteLine("123"); 
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !FileListExtend._isInternalDrag)
            {
                e.Effects = DragDropEffects.Copy;
                ShowDropHint();
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        // 当拖动操作离开窗口时触发此事件
        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            if (!FileListExtend._isInternalDrag)
            {
                HideDropHint();
            }
            e.Handled = true;
        }

        // 当用户在窗口内释放鼠标按钮完成拖放时触发此事件
        private void Window_Drop(object sender, DragEventArgs e)
        {
            HideDropHint();
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !FileListExtend._isInternalDrag)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    // 创建 FileInfoViewModel 实例并检查是否已经包含该文件
                    if (!Files.Any(f => f.FilePath == file))
                    {
                        var fileInfo = new FileInfo(file);
                        Files.Add(new FileInfo(file));
                    }
                }
            }
            FileListExtend._isInternalDrag = false;  // 重置状态
            FileListExtend._isInternalDrop = true;
        }


        // 显示提示覆盖层的方法
        private void ShowDropHint()
        {
            Grid? overlay = FindName("DropHintOverlay") as Grid;
            Rectangle? tline = FindName("DropHintLine") as Rectangle;
            if (overlay != null && tline != null)
            {
                // 触发进入动画
                overlay.IsHitTestVisible = true;
                overlay.IsEnabled = true;
                tline.IsEnabled = false;
            }
        }

        // 隐藏提示覆盖层的方法
        private void HideDropHint()
        {
            var overlay = FindName("DropHintOverlay") as Grid;
            Rectangle? tline = FindName("DropHintLine") as Rectangle;
            if (overlay != null && tline != null)
            {
                // 触发退出动画
                overlay.IsHitTestVisible = false;
                overlay.IsEnabled = false;
                tline.IsEnabled = true;
            }
            FileList.Focus();
        }

        private void FileList_SelectionTo0(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Files.Count == 0)
                ShowDropHint();
        }

        // 处理 Del 键删除选中的 ListBox 项
        private void FileList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // 使用 for 循环从后往前删除选中的项
                for (int i = FileList.SelectedItems.Count - 1; i >= 0; i--)
                {
                    var item = FileList.SelectedItems[i];
                    Files.Remove((FileInfo)item);
                    //if (Files.Count == 0)
                    //{
                    //    ShowDropHint();
                    //}
                }
                e.Handled = true;
            }
        }
    }
}