using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace hcClient.ui
{
    class LampArrayWidget : WidgetBase, IActiveWidget
    {
        #region " IActiveWidget "

        private int _id = 0;
        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public int Data
        {
            get { return State; }
            set { State = value; }
        }

        #endregion

        #region " BasePoints "

        private Point[] _basePoints = null;
        public Point[] BasePoints
        {
            get { return _basePoints; }
            set
            {
                if (_basePoints != value)
                {
                    _basePoints = value;
                    updateImageRectangles();
                    Invalidate();
                }
            }
        }

        private Rectangle[] _imageRectangles = null;

        private void updateImageRectangles()
        {
            if (_basePoints == null || _basePoints.Length == 0)
            {
                _imageRectangles = null;
                return;
            }

            _imageRectangles = new Rectangle[_basePoints.Length];
            updateImageSizes();
        }

        #endregion

        #region " Image "

        private Image _image = null;
        private Image Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    updateImageSizes();
                    Invalidate();
                }
            }
        }

        private void updateImageSizes()
        {
            if (_basePoints == null || _basePoints.Length == 0)
                return;

            Size s = (_image == null ? Size.Empty : _image.Size);
            int centerX = s.Width / 2;
            int centerY = s.Height / 2;

            for (int i = 0; i < _basePoints.Length; i++)
            {
                _imageRectangles[i].X = X + _basePoints[i].X - centerX;
                _imageRectangles[i].Y = Y + _basePoints[i].Y - centerY;
                _imageRectangles[i].Size = s;
            }
        }

        #endregion

        #region " Images "

        private Image[] _images = null;
        public Image[] Images
        {
            get { return _images; }
            set
            {
                _images = value;
                updateImage();
            }
        }

        private int _state = -1;
        public int State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    updateImage();
                }
            }
        }

        private void updateImage()
        {
            if (_images == null || _images.Length == 0)
            {
                Image = null;
                return;
            }

            if (_state < 0)
            {
                Image = _images[0]; // maybe null
            }
            else if (_state >= _images.Length)
            {
                Image = _images[_images.Length - 1];
            }
            else
            {
                Image = _images[_state];
            }
        }

        #endregion

        #region " Disabled "

        private bool _disabled = false;
        public bool Disabled
        {
            get { return _disabled; }
            set
            {
                if (_disabled != value)
                {
                    _disabled = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region " Overrided Methods "

        public override void Offset(int dx, int dy)
        {
            if (_imageRectangles != null)
            {
                for (int i = 0; i < _imageRectangles.Length; i++)
                    _imageRectangles[i].Offset(dx, dy);
            }
            base.Offset(dx, dy);
        }

        protected override void OnMoved(EventArgs e)
        {
            if (Region == null)
                Region = new Region(_widgetRectangle);
            base.OnMoved(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Region = new Region(_widgetRectangle);
            base.OnResize(e);
        }

        private bool _pressed = false;

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (!_disabled)
            {
                _pressed = true;
                Invalidate();
                base.OnMouseDown(e);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        public override void OnClick(EventArgs e)
        {
            if (Parent != null)
                Parent.ChangeData(ID, (Data > 0 ? 0 : 1));
        }

        #endregion

        #region " Painting "

        public override void Paint(PaintEventArgs e)
        {
            if (Region != null)
            {
                if (_pressed)
                    e.Graphics.FillRegion(new SolidBrush(Style.RegionPressed), Region);
                else
                    e.Graphics.FillRegion(new SolidBrush(Style.Region), Region);
            }

            if (_image == null || _imageRectangles == null || _imageRectangles.Length == 0)
                return;

            Rectangle dstRect, srcRect;
            for (int i = 0; i < _imageRectangles.Length; i++)
            {
                dstRect = e.ClipRectangle;
                dstRect.Intersect(_imageRectangles[i]);
                if (!dstRect.IsEmpty)
                {
                    srcRect = dstRect;
                    srcRect.X -= _imageRectangles[i].X;
                    srcRect.Y -= _imageRectangles[i].Y;
                    e.Graphics.DrawImage(_image, dstRect, srcRect, GraphicsUnit.Pixel);
                }
            }
        }

        #endregion
    }
}
