using System;
using System.Drawing;
using System.Windows.Forms;

namespace hcClient.ui
{
    enum TextStyle { Normal, OutlineWhite, OutlineBlack };

    class TextWidget : WidgetBase
    {
        #region " Constructor "

        public TextWidget()
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

        #region " TextStyle "

        private TextStyle _textStyle = TextStyle.Normal;
        public TextStyle TextStyle
        {
            get { return _textStyle; }
            set
            {
                _textStyle = value;
                Invalidate();
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

        private Color _foreColor = Color.White;
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

        private Color _backColor = Color.Transparent;
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

        #region " Painting "

        public override void Paint(PaintEventArgs e)
        {
            if (_backColor != Color.Transparent)
            {
                e.Graphics.FillRectangle(new SolidBrush(_backColor), _widgetRectangle);
            }

            if (_text != null && _font != null)
            {
                int opacity = _foreColor.A;
                if (_disabled) opacity = opacity / 2;

                Rectangle rect = applyPadding(_widgetRectangle, _padding);
                if (_textStyle != TextStyle.Normal)
                {
                    Brush outlineBrush;
                    if (_textStyle == TextStyle.OutlineBlack)
                        outlineBrush = new SolidBrush(Color.Black);
                    else
                        outlineBrush = new SolidBrush(Color.White);

                    rect.Offset(1, 0);
                    e.Graphics.DrawString(_text, _font, outlineBrush, rect, _textFormat);
                    rect.Offset(-1, -1);
                    e.Graphics.DrawString(_text, _font, outlineBrush, rect, _textFormat);
                    rect.Offset(-1, 1);
                    e.Graphics.DrawString(_text, _font, outlineBrush, rect, _textFormat);
                    rect.Offset(1, 1);
                    e.Graphics.DrawString(_text, _font, outlineBrush, rect, _textFormat);
                    rect.Offset(0, -1);
                }
                e.Graphics.DrawString(_text, _font, new SolidBrush(Color.FromArgb(opacity, _foreColor)), rect, _textFormat);
            }
        }

        #endregion

        #region " IDisposable "

        public override void Dispose()
        {
            _textFormat.Dispose();
            base.Dispose();
        }

        #endregion
    }
}
