using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace CPC.Toolkit.Scanner
{
    public class CropThumb : Thumb
    {
        #region Fields

        int _cpx;

        #endregion

        #region Constructor

        internal CropThumb(int cpx)
            : base()
        {
            _cpx = cpx;
        }

        #endregion

        #region Overrides

        protected override Visual GetVisualChild(int index)
        {
            return null;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRoundedRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(new Size(_cpx, _cpx)), 1, 1);
        }

        #endregion

        #region Positioning

        internal void SetPos(double x, double y)
        {
            Canvas.SetTop(this, y - _cpx / 2);
            Canvas.SetLeft(this, x - _cpx / 2);
        }

        #endregion
    }
}
