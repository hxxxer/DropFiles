using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace DropFiles
{
    // MarqueeSelectionBehavior.cs
    public class ListBoxSelectionBehavior
    {
        private ListBox _listBox;
        private Point _startPoint;
        private Rectangle _selectionRect;
        private bool _isMarqueeSelecting;  // 框选状态
        private bool _isDragging;          // 拖拽状态
        private Canvas _selectionCanvas;
        private ListBoxItem? _dragStartItem; // 记录开始拖拽的项

        public ListBoxSelectionBehavior(ListBox listBox)
        {
            _listBox = listBox;

            // 创建选择框
            _selectionCanvas = new Canvas();
            _selectionRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(50, 51, 153, 255)),
                Stroke = new SolidColorBrush(Color.FromRgb(51, 153, 255)),
                StrokeDashArray = new DoubleCollection(new double[] { 2 }),
                Visibility = Visibility.Collapsed
            };
            _selectionCanvas.Children.Add(_selectionRect);

            if (_listBox.Parent is Grid grid)
            {
                grid.Children.Add(_selectionCanvas);
            }

            _listBox.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            _listBox.PreviewMouseMove += ListBox_PreviewMouseMove;
            _listBox.PreviewMouseLeftButtonUp += ListBox_PreviewMouseLeftButtonUp;
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(_selectionCanvas);

            // 检查鼠标是否点击在 ListBoxItem 上
            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            ListBoxItem clickedItem = FindAncestor<ListBoxItem>(originalSource);

            if (clickedItem != null)
            {
                // 点击在项目上，准备拖拽
                _dragStartItem = clickedItem;
                _isDragging = true;

                // 如果没有按住Ctrl键，并且点击的项目未被选中，清除其他选择
                if (Keyboard.Modifiers != ModifierKeys.Control && !clickedItem.IsSelected)
                {
                    _listBox.SelectedItems.Clear();
                    clickedItem.IsSelected = true;
                }
            }
            else
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
            }

            _listBox.CaptureMouse();
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMarqueeSelecting && !_isDragging) return;

            Point currentPoint = e.GetPosition(_selectionCanvas);

            if (_isMarqueeSelecting)
            {
                // 处理框选
                double left = Math.Min(_startPoint.X, currentPoint.X);
                double top = Math.Min(_startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - _startPoint.X);
                double height = Math.Abs(currentPoint.Y - _startPoint.Y);

                Canvas.SetLeft(_selectionRect, left);
                Canvas.SetTop(_selectionRect, top);
                _selectionRect.Width = width;
                _selectionRect.Height = height;

                UpdateSelection(new Rect(left, top, width, height));
            }
            else if (_isDragging)
            {
                Vector diff = _startPoint - currentPoint;
                // 处理拖拽
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance) // 拖拽阈值
                {
                    var selectedItems = _listBox.SelectedItems.Cast<object>().ToList();
                    DragDrop.DoDragDrop(_listBox, selectedItems, DragDropEffects.Move);
                    _isDragging = false;
                }
            }
        }

        private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMarqueeSelecting = false;
            _isDragging = false;
            _dragStartItem = null;
            _selectionRect.Visibility = Visibility.Collapsed;
            _listBox.ReleaseMouseCapture();
        }

        private void UpdateSelection(Rect selectionRect)
        {
            foreach (var item in _listBox.Items)
            {
                var listBoxItem = (ListBoxItem)_listBox.ItemContainerGenerator.ContainerFromItem(item);
                if (listBoxItem == null) continue;

                Point itemTopLeft = listBoxItem.TranslatePoint(new Point(0, 0), _selectionCanvas);
                Rect itemRect = new Rect(itemTopLeft.X, itemTopLeft.Y, listBoxItem.ActualWidth, listBoxItem.ActualHeight);

                if (selectionRect.IntersectsWith(itemRect))
                {
                    if (!_listBox.SelectedItems.Contains(item))
                        _listBox.SelectedItems.Add(item);
                }
                else if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    _listBox.SelectedItems.Remove(item);
                }
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
