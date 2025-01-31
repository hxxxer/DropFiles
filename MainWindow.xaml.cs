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

        //private void InitializeSelectionUI()
        //{
        //    _selectionCanvas = new Canvas();
        //    _selectionRect = new Rectangle
        //    {
        //        Fill = new SolidColorBrush(Color.FromArgb(50, 51, 153, 255)),
        //        Stroke = new SolidColorBrush(Color.FromRgb(51, 153, 255)),
        //        Visibility = Visibility.Collapsed,
        //        IsEnabled = false,
        //    };
        //    _selectionCanvas.Children.Add(_selectionRect);

        //    if (FileList.Parent is Grid grid)
        //    {
        //        grid.Children.Add(_selectionCanvas);
        //    }
        //}

        //private void AttachEventHandlers()
        //{
        //    FileList.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
        //    FileList.PreviewMouseMove += ListBox_PreviewMouseMove;
        //    FileList.PreviewMouseLeftButtonUp += ListBox_PreviewMouseLeftButtonUp;
        //}

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

        private void FileList_SelectionTo0(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Files.Count == 0)
                ShowDropHint();
        }

        //private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    _originalSelectedItems = new List<object>(FileList.SelectedItems.Cast<object>());
        //    _startPoint = e.GetPosition(_selectionCanvas);

        //    // 检查鼠标是否点击在 ListBoxItem 上
        //    DependencyObject originalSource = e.OriginalSource as DependencyObject;
        //    ListBoxItem clickedItem = FindAncestor<ListBoxItem>(originalSource);

        //    if (clickedItem != null)
        //    {
        //        // 点击在项目上，准备拖拽
        //        //_dragStartItem = clickedItem;
        //        _isDragging = true;

        //        // 如果没有按住Ctrl键，并且点击的项目未被选中，清除其他选择
        //        //if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && !clickedItem.IsSelected)
        //        //{
        //        //    FileList.SelectedItems.Clear();
        //        //    clickedItem.IsSelected = true;
        //        //}
        //    }
        //    else
        //    {
        //        // 点击在空白处，准备框选
        //        _isMarqueeSelecting = true;
        //        if (Keyboard.Modifiers != ModifierKeys.Control)
        //            FileList.SelectedItems.Clear();

        //        _selectionRect.Visibility = Visibility.Visible;
        //        Canvas.SetLeft(_selectionRect, _startPoint.X);
        //        Canvas.SetTop(_selectionRect, _startPoint.Y);
        //        _selectionRect.Width = 0;
        //        _selectionRect.Height = 0;

        //        FileList.CaptureMouse();
        //    }

        //}

        //private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!_isMarqueeSelecting && !_isDragging) return;

        //    Point currentPoint = e.GetPosition(_selectionCanvas);

        //    if (_isMarqueeSelecting && e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        // 处理框选
        //        double left = Math.Min(_startPoint.X, currentPoint.X);
        //        double top = Math.Min(_startPoint.Y, currentPoint.Y);
        //        double width = Math.Abs(currentPoint.X - _startPoint.X);
        //        double height = Math.Abs(currentPoint.Y - _startPoint.Y);
        //        double viewportWidth = FileList.ActualWidth;
        //        double viewportHeight = FileList.ActualHeight;

        //        // 边界检查：确保选择框不会超出可视范围
        //        //left = Math.Max(0, Math.Min(left, viewportWidth));
        //        //top = Math.Max(0, Math.Min(top, viewportHeight));
        //        width = Math.Max(0, Math.Min(width, viewportWidth - left));
        //        height = Math.Max(0, Math.Min(height, viewportHeight - top));

        //        Canvas.SetLeft(_selectionRect, left);
        //        Canvas.SetTop(_selectionRect, top);
        //        _selectionRect.Width = width;
        //        _selectionRect.Height = height;

        //        UpdateSelection(new Rect(left, top, width, height));
        //    }
        //    else if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        Vector diff = _startPoint - currentPoint;
        //        // 处理拖拽
        //        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
        //            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) // 拖拽阈值
        //        {
        //            // 设置内部拖动标志
        //            FileListExtend._isInternalDrag = true;
        //            FileListExtend._isInternalDrop = false;

        //            var selectedItems = FileList.SelectedItems.Cast<FileInfo>();
        //            var filePaths = selectedItems.Select(item => item.FilePath).ToArray();

        //            //if (filePaths.Length > 0)
        //            //{
        //            //}
        //            var dataObject = new DataObject(DataFormats.FileDrop, filePaths);
        //            //Debug.Assert(FileListExtend._isInternalDrag);

        //            var result = DragDrop.DoDragDrop(FileList, dataObject, DragDropEffects.Move);
        //            //Debug.Assert(FileListExtend._isInternalDrag);

        //            // 只有在外部移动操作成功完成时才删除文件
        //            if (!FileListExtend._isInternalDrop && result == DragDropEffects.Move)
        //            {
        //                // 使用 for 循环从后往前删除选中的项
        //                for (int i = FileList.SelectedItems.Count - 1; i >= 0; i--)
        //                {
        //                    var item = FileList.SelectedItems[i];
        //                    //FileList.Items.Remove((FileInfo)item);
        //                    Files.Remove((FileInfo)item);
        //                }
        //            }

        //            //Debug.Assert(FileListExtend._isInternalDrag);
        //            _isDragging = false;
        //            //FileListExtend._isInternalDrag = false;  // 拖动结束后重置标志
        //            //aaaa = 0;
        //        }
        //    }
        //}

        //private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    _originalSelectedItems.Clear();
        //    FileListExtend._isInternalDrag = false;
        //    //Debug.Assert(FileListExtend._isInternalDrag);
        //    _isMarqueeSelecting = false;
        //    _isDragging = false;
        //    //_dragStartItem = null;
        //    _selectionRect.Visibility = Visibility.Collapsed;
        //    FileList.ReleaseMouseCapture();
        //}

        //private void UpdateSelection(Rect selectionRect)
        //{
        //    foreach (var item in FileList.Items)
        //    {
        //        var listBoxItem = (ListBoxItem)FileList.ItemContainerGenerator.ContainerFromItem(item);
        //        if (listBoxItem == null) continue;

        //        Point itemTopLeft = listBoxItem.TranslatePoint(new Point(0, 0), _selectionCanvas);
        //        Rect itemRect = new Rect(itemTopLeft.X, itemTopLeft.Y, listBoxItem.ActualWidth, listBoxItem.ActualHeight);

        //        bool shouldSelect;
        //        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        //            shouldSelect = selectionRect.IntersectsWith(itemRect) ^ _originalSelectedItems.Contains(item);
        //        else
        //            shouldSelect = selectionRect.IntersectsWith(itemRect);

        //        if (shouldSelect)
        //            FileList.SelectedItems.Add(item);
        //        else
        //            FileList.SelectedItems.Remove(item);
        //    }
        //}

        //private void FileList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    // 记录鼠标按下时的位置
        //    _startPoint = e.GetPosition(null);

        //    // 获取点击的 ListBoxItem
        //    var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        //    if (listBoxItem != null)
        //    {
        //        // 获取 FileInfo 实例
        //        FileInfo fileToMove = listBoxItem.Content as FileInfo;
        //        if (fileToMove == null) return;

        //        // 选中当前项
        //        //listBoxItem.IsSelected = true;

        //        // 保存拖动的项
        //        _draggedItem = listBoxItem;
        //    }
        //}

        //private void FileList_MouseMove(object sender, MouseEventArgs e)
        //{
        //    // 判断是否开始拖动
        //    if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
        //    {
        //        Point mousePos = e.GetPosition(null);
        //        Vector diff = _startPoint - mousePos;

        //        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
        //            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        //        {
        //            // 获取 FileInfo 实例
        //            FileInfo fileToMove = _draggedItem.Content as FileInfo;
        //            if (fileToMove == null) return;

        //            // 设置内部拖动标志
        //            FileListExtend._isInternalDrag = true;
        //            FileListExtend._isInternalDrop = false;

        //            // 根据按键状态确定拖放效果
        //            DragDropEffects effects = DragDropEffects.Move; // 默认为移动

        //            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        //            {
        //                effects = DragDropEffects.Copy;
        //            }
        //            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        //            {
        //                effects = DragDropEffects.Link;
        //            }

        //            // 创建拖动数据
        //            var data = new DataObject(DataFormats.FileDrop, new string[] { fileToMove.FilePath });

        //            // 开始拖动
        //            var result = DragDrop.DoDragDrop(_draggedItem, data, effects);

        //            // 只有在外部移动操作成功完成时才删除文件
        //            if (!FileListExtend._isInternalDrop && result == DragDropEffects.Move)
        //            {
        //                // 从列表中移除文件
        //                Files.Remove(fileToMove);
        //                if (Files.Count == 0)
        //                {
        //                    ShowDropHint();
        //                }
        //            }

        //            // 重置拖动项
        //            _draggedItem = null;
        //            FileListExtend._isInternalDrag = false;  // 拖动结束后重置标志
        //        }
        //    }
        //}

        //private void FileList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    // 结束拖动
        //    if (_draggedItem != null)
        //    {
        //        _draggedItem = null;
        //        isInternalDrag = false;  // 拖动结束后重置标志
        //    }
        //}

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

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Debug.Assert(false);
        }
    }

}