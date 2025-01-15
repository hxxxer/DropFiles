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
        public ObservableCollection<string> Files { get; set; } = [];
        private bool isInternalDrag = false;

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
                    if (!Files.Contains(file))
                        Files.Add(file);
                }
            }
            isInternalDrag = false;  // 重置状态
        }

        // 当拖动操作给出反馈时触发此事件
        private void Window_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // 如果鼠标指针移出了窗口，则隐藏提示覆盖层
            if (Mouse.Captured == null || !this.IsMouseOver)
            {
                HideDropHint();
            }

            e.Handled = true;
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
                isInternalDrag = true;  // 设置内部拖动标志
                var data = new DataObject(DataFormats.FileDrop, new string[] { (string)listBoxItem.Content });
                DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
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
    }
}