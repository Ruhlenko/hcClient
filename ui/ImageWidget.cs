using System;
using System.Drawing;
using System.Windows.Forms;

namespace hcClient.ui
{
    class ImageWidget : WidgetBase
    {
        #region " BasePoint "

        private Point _basePoint = Point.Empty;
        public Point BasePoint
        {
            get { return _basePoint; }
            set
            {
                if (_basePoint != value)
                {
                    _basePoint = value;
                    updateWidgetRectangle();
                }
            }
        }

        public override void Offset(int dx, int dy)
        {
            base.Offset(dx, dy);
            _basePoint.Offset(dx, dy);
        }

        #endregion

        #region " ImageAlign "

        private ContentAlignment _imageAlign = ContentAlignment.MiddleCenter;
        public ContentAlignment ImageAlign
        {
            get { return _imageAlign; }
            set
            {
                if (Enum.IsDefined(typeof(ContentAlignment), value))
                {
                    _imageAlign = value;
                    updateWidgetRectangle();
                }
            }
        }

        private void updateWidgetRectangle()
        {
            Point newLocation = Point.Empty;

            switch (_imageAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    newLocation.X = _basePoint.X;
                    break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    newLocation.X = _basePoint.X - Size.Width / 2;
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    newLocation.X = _basePoint.X - Size.Width;
                    break;
            }

            switch (_imageAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    newLocation.Y = _basePoint.Y;
                    break;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    newLocation.Y = _basePoint.Y - Size.Height / 2;
                    break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    newLocation.Y = _basePoint.Y - Size.Height;
                    break;
            }

            Location = newLocation;
        }

        #endregion

        #region " Image "

        private Image _image = null;
        public Image Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    if (value != null)
                    {
                        Size = _image.Size;
                        updateWidgetRectangle();
                    }
                    Invalidate();
                }
            }
        }

        #endregion

        #region " Painting "

        public override void Paint(PaintEventArgs e)
        {
            Rectangle dstRect = e.ClipRectangle;
            dstRect.Intersect(_widgetRectangle);

            if (!dstRect.IsEmpty && _image != null)
            {
                Rectangle srcRect = dstRect;
                srcRect.X -= _widgetRectangle.X;
                srcRect.Y -= _widgetRectangle.Y;
                e.Graphics.DrawImage(_image, dstRect, srcRect, GraphicsUnit.Pixel);
            }
        }

        #endregion
    }
}
