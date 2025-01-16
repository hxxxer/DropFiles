using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace DropFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileInfo> Files { get; set; } = [];
        private bool isInternalDrag = false;
        private bool isInternalDrop = false;

        public MainWindow()
        {
            InitializeComponent();
            FileList.ItemsSource = Files;
        }

        // 当拖动操作进入或移动过窗口时触发此事件
        private void Window_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !isInternalDrag)
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
            if (!isInternalDrag)
            {
                HideDropHint();
            }
            e.Handled = true;
        }

        // 当用户在窗口内释放鼠标按钮完成拖放时触发此事件
        private void Window_Drop(object sender, DragEventArgs e)
        {
            HideDropHint();
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !isInternalDrag)
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
            isInternalDrag = false;  // 重置状态
            isInternalDrop = true;
        }


        // 显示提示覆盖层的方法
        private void ShowDropHint()
        {
            DropHintOverlay.Visibility = Visibility.Visible;
        }

        // 隐藏提示覆盖层的方法
        private void HideDropHint()
        {
            DropHintOverlay.Visibility = Visibility.Collapsed;
        }

        private void FileList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (listBoxItem != null)
            {
                // 获取 FileInfoViewModel 实例
                FileInfo fileToMove = listBoxItem.Content as FileInfo;
                if (fileToMove == null) return;

                isInternalDrag = true;  // 设置内部拖动标志
                isInternalDrop= false;

                // 根据按键状态确定拖放效果
                DragDropEffects effects = DragDropEffects.Move; // 默认为移动

                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    effects = DragDropEffects.Copy;
                }
                else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                {
                    effects = DragDropEffects.Link;
                }

                var data = new DataObject(DataFormats.FileDrop, new string[] { fileToMove.FilePath });
                var result = DragDrop.DoDragDrop(listBoxItem, data, effects);

                // 只有在外部移动操作成功完成时才删除文件
                if (!isInternalDrop && result == DragDropEffects.Move)
                {
                    // 从列表中移除文件
                    Files.Remove(fileToMove);
                }
                isInternalDrag = false;  // 拖动结束后重置标志
            }
        }

        // 从当前对象dependencyObject开始，向上查找类型为T的父元素。找到则返回，否则返回null
        private static T FindAncestor<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent == null) return null;

            var parentT = parent as T;
            return parentT ?? FindAncestor<T>(parent);
        }

        // 处理 Del 键删除选中的 ListBox 项
        private void FileList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var selectedItems = FileList.SelectedItems;
                // 使用 for 循环从后往前删除选中的项
                for (int i = FileList.SelectedItems.Count - 1; i >= 0; i--)
                {
                    var item = FileList.SelectedItems[i];
                    Files.Remove((FileInfo)item);
                }
                e.Handled = true;
            }
        }
    }
}