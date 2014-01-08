using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;

namespace CPC.Toolkit.Layout
{
    public class ResizeThumb : Thumb
    {
        Window designerItem;

        public ResizeThumb(Window window)
        {
            designerItem = window;
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double deltaVertical, deltaHorizontal;

            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    deltaVertical = Math.Min(-e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight);
                    designerItem.Dispatcher.Invoke(new Action(delegate()
                    {
                        designerItem.Height = designerItem.ActualHeight - deltaVertical;
                    }));
                    break;
                case VerticalAlignment.Top:
                    deltaVertical = Math.Min(e.VerticalChange, designerItem.ActualHeight - designerItem.MinHeight);
                    //Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) + deltaVertical);
                    designerItem.Dispatcher.Invoke(new Action(delegate()
                    {
                        designerItem.Top += deltaVertical;
                        designerItem.Height = designerItem.ActualHeight - deltaVertical;
                    }));
                    break;
                default:
                    break;
            }

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    deltaHorizontal = Math.Min(e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth);
                    //Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) + deltaHorizontal);
                    designerItem.Dispatcher.Invoke(new Action(delegate()
                    {
                        designerItem.Left += deltaHorizontal;
                        designerItem.Width = designerItem.ActualWidth - deltaHorizontal;
                    }));
                    break;
                case HorizontalAlignment.Right:
                    deltaHorizontal = Math.Min(-e.HorizontalChange, designerItem.ActualWidth - designerItem.MinWidth);
                    designerItem.Dispatcher.Invoke(new Action(delegate()
                    {
                        designerItem.Width = designerItem.ActualWidth - deltaHorizontal;
                    }));
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }
    }

    public static class CustomWPF
    {
        const int Distance = 10;

        public static void ResizeThumb(this Grid grid, Window window)
        {
            // Top
            grid.Children.Add(new ResizeThumb(window)
            {
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(Distance, 0, Distance, 0),
                Cursor = Cursors.SizeNS,
                Opacity = 0,
                Height = Distance
            });

            // Bottom
            grid.Children.Add(new ResizeThumb(window)
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(Distance, 0, Distance, 0),
                Cursor = Cursors.SizeNS,
                Opacity = 0,
                Height = Distance
            });

            // Left
            grid.Children.Add(new ResizeThumb(window)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, Distance, 0, Distance),
                Cursor = Cursors.SizeWE,
                Opacity = 0,
                Width = Distance
            });

            // Right
            grid.Children.Add(new ResizeThumb(window)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, Distance, 0, Distance),
                Cursor = Cursors.SizeWE,
                Opacity = 0,
                Width = Distance
            });

            // Top Left
            grid.Children.Add(new ResizeThumb(window)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Cursor = Cursors.SizeNWSE,
                Opacity = 0,
                Width = Distance,
                Height = Distance
            });

            // Top Right
            grid.Children.Add(new ResizeThumb(window)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Cursor = Cursors.SizeNESW,
                Opacity = 0,
                Width = Distance,
                Height = Distance
            });

            // Bottom Left
            grid.Children.Add(new ResizeThumb(window)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.SizeNESW,
                Opacity = 0,
                Width = Distance,
                Height = Distance
            });

            // Bottom Right
            grid.Children.Add(new ResizeThumb(window)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.SizeNWSE,
                Opacity = 0,
                Width = Distance,
                Height = Distance
            });
        }
    }
}