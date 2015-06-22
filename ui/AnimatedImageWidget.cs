using System;
using System.Drawing;
using System.Windows.Forms;

namespace hcClient.ui
{
    class AnimatedImageWidget : WidgetBase
    {
        #region " Image "

        private Image _image = null;
        private bool _imageAnimated = false;
        public Image Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    stopAnimate();
                    _image = value;
                    if (value != null)
                    {
                        Size = _image.Size;
                        startAnimate();
                    }
                    Invalidate();
                }
            }
        }

        private Image _hoverImage = null;
        public Image HoverImage
        {
            get { return _hoverImage; }
            set { _hoverImage = value; }
        }

        #endregion

        #region " Animation "

        private void stopAnimate()
        {
            if (_imageAnimated)
            {
                ImageAnimator.StopAnimate(_image, ImageFrameChanged);
                _imageAnimated = false;
            }
        }

        private void startAnimate()
        {
            if (ImageAnimator.CanAnimate(_image))
            {
                if (ImageFrameChanged == null) ImageFrameChanged = new EventHandler(OnImageFrameChanged);
                ImageAnimator.Animate(_image, ImageFrameChanged);
                _imageAnimated = true;
            }
        }

        private EventHandler ImageFrameChanged;
        private void OnImageFrameChanged(object o, EventArgs e)
        {
            if (_imageAnimated) Invalidate();
        }

        #endregion

        #region " Painting "

        public override void Paint(PaintEventArgs e)
        {
            Rectangle dstRect = e.ClipRectangle;
            dstRect.Intersect(_widgetRectangle);

            if (!dstRect.IsEmpty && _image != null)
            {
                if (_imageAnimated) ImageAnimator.UpdateFrames(_image);

                Rectangle srcRect = dstRect;
                srcRect.X -= _widgetRectangle.X;
                srcRect.Y -= _widgetRectangle.Y;
                e.Graphics.DrawImage(_image, dstRect, srcRect, GraphicsUnit.Pixel);

                if (Hovered && _hoverImage != null)
                    e.Graphics.DrawImage(_hoverImage, dstRect, srcRect, GraphicsUnit.Pixel);
            }
        }

        #endregion
    }
}
