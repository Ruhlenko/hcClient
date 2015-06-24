using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace hcClient.ui
{
    class ButtonWidget : WidgetBase
    {
        #region " Constructor "

        public ButtonWidget()
        {
            updateTextFormat();
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

        #region " Image "

        private Image _image = null;
        public Image Image
        {
            get { return _image; }
            set
            {
                _image = value;
                updateImageRect();
            }
        }

        private Rectangle _imageRect = Rectangle.Empty;
        private void updateImageRect()
        {
            if (_image == null)
            {
                _imageRect = Rectangle.Empty;
                return;
            }
            _imageRect.Size = _image.Size;
            _imageRect.X = Left + (Width - _imageRect.Width) / 2;
            _imageRect.Y = Top + (Height - _imageRect.Height) / 2;
        }

        #endregion

        #region " Text "

        private string _text = null;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                Invalidate();
            }
        }

        #endregion

        #region " TextAlign "

        private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
        private StringFormat _textFormat;
        public ContentAlignment TextAlign
        {
            get { return _textAlign; }
            set
            {
                if (Enum.IsDefined(typeof(ContentAlignment), value))
                {
                    _textAlign = value;
                    updateTextFormat();
                }
            }
        }

        private void updateTextFormat()
        {
            _textFormat = new StringFormat(StringFormat.GenericDefault);
            switch (_textAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    _textFormat.LineAlignment = StringAlignment.Near;
                    break;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    _textFormat.LineAlignment = StringAlignment.Center;
                    break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    _textFormat.LineAlignment = StringAlignment.Far;
                    break;
            }
            switch (_textAlign)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    _textFormat.Alignment = StringAlignment.Near;
                    break;
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    _textFormat.Alignment = StringAlignment.Center;
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    _textFormat.Alignment = StringAlignment.Far;
                    break;
            }
        }

        #endregion

        #region " Font "

        private Font _font = null;
        public Font Font
        {
            get { return _font; }
            set
            {
                _font = value;
                Invalidate();
            }
        }

        #endregion

        #region " ForeColor "

        private Color _foreColor = Style.TextColor;
        public Color ForeColor
        {
            get { return _foreColor; }
            set
            {
                _foreColor = value;
                Invalidate();
            }
        }

        #endregion

        #region " BackColor "

        private Color _backColor = Style.Button;
        public Color BackColor
        {
            get { return _backColor; }
            set
            {
                _backColor = value;
                Invalidate();
            }
        }

        #endregion

        #region " Active "

        private bool _active = false;
        public bool Active
        {
            get { return _active; }
            set
            {
                if (_active != value)
                {
                    _active = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region " ActiveColor "

        private Color _activeColor = Style.ButtonActive;
        public Color ActiveColor
        {
            get { return _activeColor; }
            set
            {
                _activeColor = value;
                Invalidate();
            }
        }

        #endregion

        #region " Padding "

        private Padding _padding = new Padding(0);
        public Padding Padding
        {
            get { return _padding; }
            set
            {
                _padding = value;
                Invalidate();
            }
        }

        private static Rectangle applyPadding(Rectangle _r, Padding _p)
        {
            Rectangle r = _r;

            r.X += _p.Left;
            r.Y += _p.Top;
            r.Width -= _p.Horizontal;
            r.Height -= _p.Vertical;

            return r;
        }

        #endregion

        #region " Overrided Methods "

        protected override void OnMoved(EventArgs e)
        {
            Region = new Region(_widgetRectangle);
            updateImageRect();
            base.OnMoved(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Region = new Region(_widgetRectangle);
            updateImageRect();
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

        #endregion

        #region " Painting "

        public override void Paint(PaintEventArgs e)
        {
            if (_backColor != Color.Transparent)
            {
                Color fillColor = (_active ? _activeColor : _backColor);

                if (_disabled)
                    fillColor = Style.Disabled(fillColor);
                else if (_pressed)
                    fillColor = Style.Pressed(fillColor);

                e.Graphics.FillRectangle(new SolidBrush(fillColor), _widgetRectangle);
            }

            if (_image != null)
            {
                Rectangle dstRect = e.ClipRectangle;
                dstRect.Intersect(_imageRect);
                if (!dstRect.IsEmpty)
                {
                    Rectangle srcRect = dstRect;
                    srcRect.X -= _imageRect.X;
                    srcRect.Y -= _imageRect.Y;

                    e.Graphics.DrawImage(_image, dstRect, srcRect, GraphicsUnit.Pixel);
                }
            }

            if (_text != null && _font != null && _text.Length > 0)
            {
                int opacity = (_disabled ? _foreColor.A / 2 : _foreColor.A);
                Rectangle rect = applyPadding(_widgetRectangle, _padding);
                e.Graphics.DrawString(_text, _font, new SolidBrush(Color.FromArgb(opacity, _foreColor)), rect, _textFormat);
            }
        }

        #endregion
    }
}
