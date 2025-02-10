using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using ControlzEx.Standard;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace DropFiles
{
    public class ListBoxSelectionBehavior
    {
        private ListBox _listBox;
        private Point _startPoint;
        private Rectangle _selectionRect;
        private bool _isMarqueeSelecting;  // 框选状态
        private bool _isDragging;          // 拖拽状态
        private Canvas _selectionCanvas;
        private ListBoxItem? _lastHoverItem; // 记录开始拖拽的项
        private bool _hadDrop;
        private List<object>? _originalSelectedItems = [];
        private ObservableCollection<FileInfo> _files { get; set; } = [];
        public bool _isInternalDrag;
        public bool _isInternalDrop;

        public ListBoxSelectionBehavior(ListBox listBox, ObservableCollection<FileInfo> Files)
        {
            _listBox = listBox;
            _files = Files;
            _isInternalDrag = false;
            _isInternalDrop = false;
            _hadDrop = false;

            InitializeSelectionUI();
            AttachEventHandlers();
        }

        private void InitializeSelectionUI()
        {
            // 创建选择框
            _selectionCanvas = new Canvas();
            _selectionRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(50, 51, 153, 255)),
                Stroke = new SolidColorBrush(Color.FromRgb(51, 153, 255)),
                Visibility = Visibility.Collapsed,
                IsEnabled = false,
            };
            _selectionCanvas.Children.Add(_selectionRect);

            if (_listBox.Parent is Grid grid)
            {
                grid.Children.Add(_selectionCanvas);
            }
        }

        private void AttachEventHandlers()
        {
            _listBox.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            _listBox.PreviewMouseMove += ListBox_PreviewMouseMove;
            _listBox.PreviewMouseLeftButtonUp += ListBox_PreviewMouseLeftButtonUp;
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            ScrollBar scrollBar = FindAncestor<ScrollBar>(originalSource);
            // 点击事件发生在滚动条上
            if (scrollBar != null) return;

            _originalSelectedItems = new List<object>(_listBox.SelectedItems.Cast<object>());
            _startPoint = e.GetPosition(_selectionCanvas);

            // 检查鼠标是否点击在 ListBoxItem 上
            ListBoxItem clickedItem = FindAncestor<ListBoxItem>(originalSource);

            if (clickedItem != null)
            {
                // 点击在项目上，准备拖拽
                //_dragStartItem = clickedItem;
                _isDragging = true;

                // 如果点击的项目已经被选中，则阻止清除其它选中项的行为；
                // 或者如果按住Ctrl键，则阻止改变选中项的选中状态的行为。因为这个行为已经挪到PreviewMouseLeftButtonUp里
                if (Keyboard.Modifiers == ModifierKeys.Control || (clickedItem.IsSelected && Keyboard.Modifiers != ModifierKeys.Shift)) 
                    e.Handled = true;
            }
            else if (_listBox.Items.Count > 0)
            {
                // 点击在空白处，准备框选
                _isMarqueeSelecting = true;
                if (Keyboard.Modifiers != ModifierKeys.Control)
                    _listBox.SelectedItems.Clear();

                _selectionRect.Visibility = Visibility.Visible;
                Canvas.SetLeft(_selectionRect, _startPoint.X);
                Canvas.SetTop(_selectionRect, _startPoint.Y);
                _selectionRect.Width = 0;
                _selectionRect.Height = 0;

                _listBox.CaptureMouse();
            }

        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMarqueeSelecting && !_isDragging) return;

            Point currentPoint = e.GetPosition(_selectionCanvas);
            var hitItem = GetHoverItemUnderMouse(e);

            if (_isMarqueeSelecting && e.LeftButton == MouseButtonState.Pressed)
            {
                // 处理框选
                double left = Math.Min(_startPoint.X, currentPoint.X);
                double top = Math.Min(_startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - _startPoint.X);
                double height = Math.Abs(currentPoint.Y - _startPoint.Y);
                double viewportWidth = _listBox.ActualWidth;
                double viewportHeight = _listBox.ActualHeight;

                // 边界检查：确保选择框不会超出可视范围
                //left = Math.Max(0, Math.Min(left, viewportWidth));
                //top = Math.Max(0, Math.Min(top, viewportHeight));
                width = Math.Max(0, Math.Min(width, viewportWidth - left));
                height = Math.Max(0, Math.Min(height, viewportHeight - top));

                Canvas.SetLeft(_selectionRect, left);
                Canvas.SetTop(_selectionRect, top);
                _selectionRect.Width = width;
                _selectionRect.Height = height;

                // 增量状态对比
                if (ReferenceEquals(hitItem, _lastHoverItem) && hitItem != null)
                    return; // 鼠标仍在同一项上，跳过后续处理
                _lastHoverItem = hitItem;

                UpdateSelection(new Rect(left, top, width, height));
            }
            else if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Vector diff = _startPoint - currentPoint;
                // 处理拖拽
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) // 拖拽阈值
                {
                    // 设置内部拖动标志
                    _isInternalDrag = true;
                    _isInternalDrop = false;
                    _hadDrop = true;
                    _listBox.SelectedItems.Add(hitItem.Content);

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

                    var selectedItems = _listBox.SelectedItems.Cast<FileInfo>();
                    var filePaths = selectedItems.Select(item => item.FilePath).ToArray();

                    //if (filePaths.Length > 0)
                    //{
                    //}
                    var dataObject = new DataObject(DataFormats.FileDrop, filePaths);

                    var result = DragDrop.DoDragDrop(_listBox, dataObject, effects);

                    // 只有在外部移动操作成功完成时才删除文件
                    if (!_isInternalDrop && result == DragDropEffects.Move)
                    {
                        // 使用 for 循环从后往前删除选中的项
                        for (int i = _listBox.SelectedItems.Count - 1; i >= 0; i--)
                        {
                            var item = _listBox.SelectedItems[i];
                            //_listBox.Items.Remove((FileInfo)item);
                            _files.Remove(item as FileInfo);
                            //_ = _listBox.ItemsSource is _files;
                        }
                    }

                    _isDragging = false;
                    _isInternalDrag = false;  // 拖动结束后重置标志
                }
            }
        }

        private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _originalSelectedItems.Clear();
            _isInternalDrag = false;
            _isMarqueeSelecting = false;
            //_dragStartItem = null;
            _selectionRect.Visibility = Visibility.Collapsed;
            _listBox.ReleaseMouseCapture();

            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            // 检查鼠标是否点击在 ListBoxItem 上
            ListBoxItem clickedItem = FindAncestor<ListBoxItem>(originalSource);
            if (!_hadDrop && _isDragging && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                if (clickedItem != null) 
                {
                    if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        _listBox.SelectedItems.Clear();
                        clickedItem.IsSelected = true;
                    }
                    else
                    {
                        if (clickedItem.IsSelected)
                            _listBox.SelectedItems.Remove(clickedItem.Content);
                        else
                            _listBox.SelectedItems.Add(clickedItem.Content);
                    }
                }
                else
                    _listBox.SelectedItems.Clear();
            }
            _hadDrop = false;
            _isDragging = false;
        }

        private void UpdateSelection(Rect selectionRect)
        {
            foreach (var item in _listBox.Items)
            {
                var listBoxItem = (ListBoxItem)_listBox.ItemContainerGenerator.ContainerFromItem(item);
                if (listBoxItem == null) continue;

                Point itemTopLeft = listBoxItem.TranslatePoint(new Point(0, 0), _selectionCanvas);
                Rect itemRect = new Rect(itemTopLeft.X, itemTopLeft.Y, listBoxItem.ActualWidth, listBoxItem.ActualHeight);

                bool shouldSelect;
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    shouldSelect = selectionRect.IntersectsWith(itemRect) ^ _originalSelectedItems.Contains(item);
                else
                    shouldSelect = selectionRect.IntersectsWith(itemRect);

                if (shouldSelect)
                    _listBox.SelectedItems.Add(item);
                else
                    _listBox.SelectedItems.Remove(item);
            }
        }

        // 从当前对象开始，向上查找类型为T的父元素。找到则返回，否则返回null
        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current is not null and not T)
            {
                current = VisualTreeHelper.GetParent(current);
            }
            if (current is null) return null;
            return current as T;
        }

        // 精准命中检测方法
        private ListBoxItem GetHoverItemUnderMouse(MouseEventArgs e)
        {
            var hitResult = VisualTreeHelper.HitTest(_listBox, e.GetPosition(_listBox));
            if (hitResult is null) return null;
            return FindAncestor<ListBoxItem>(hitResult.VisualHit);
        }
    }
}
