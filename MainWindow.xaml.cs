using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DropFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> Files { get; set; } = [];

        public MainWindow()
        {
            InitializeComponent();
            FileList.ItemsSource = Files;
        }

        // 当拖动操作进入或移动过窗口时触发此事件
        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                ShowDropHint(); // 显示提示覆盖层
            }
            else
            {
                e.Effects = DragDropEffects.None;
                HideDropHint(); // 隐藏提示覆盖层
            }

            e.Handled = true;
        }

        // 当用户在窗口内释放鼠标按钮完成拖放时触发此事件
        private void Window_Drop(object sender, DragEventArgs e)
        {
            HideDropHint(); // 立即隐藏提示覆盖层
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    if (!Files.Contains(file))
                        Files.Add(file);
                }
            }
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
                var data = new DataObject(DataFormats.FileDrop, new string[] { (string)listBoxItem.Content });
                DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
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