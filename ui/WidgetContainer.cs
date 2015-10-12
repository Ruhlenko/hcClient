using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace hcClient.ui
{
    class WidgetContainer : Widget, IWidgetContainer
    {
        #region " Widgets "

        protected List<Widget> _widgets = new List<Widget>();
        protected Widget _mouseDownWidget = null;

        public virtual void AddWidget(Widget widget)
        {
            _widgets.Add(widget);
            widget.Parent = this;
            Invalidate(widget.WidgetRectangle);
        }

        public void RemoveWidget(Widget widget)
        {
            _widgets.Remove(widget);
            widget.Parent = null;
            Invalidate(widget.WidgetRectangle);
        }

        public void PopupWidget(Widget widget, WidgetPosition position)
        {
            if (Parent != null)
                Parent.PopupWidget(widget, position);
        }

        public void ClosePopup()
        {
            if (Parent != null)
                Parent.ClosePopup();
        }

        #endregion

        #region " Background Image "

        private Image _backImage = null;
        public Image Background
        {
            get { return _backImage; }
            set
            {
                _backImage = value;
                updateBackImageRect();
                Invalidate();
            }
        }

        private Alignment _backAlign = Alignment.MiddleCenter;
        public Alignment BackgroundAlign
        {
            get { return _backAlign; }
            set
            {
                if (_backAlign != value)
                {
                    _backAlign = value;
                    updateBackImageRect();
                    Invalidate();
                }
            }
        }

        private Point _backLocation = Point.Empty;
        public Point BackgroundLocation
        {
            get { return _backLocation; }
            set
            {
                if (_backAlign == Alignment.Custom)
                {
                    _backLocation = value;
                    updateBackImageRect();
                    Invalidate();
                }
            }
        }

        #endregion

        #region " Methods "

        private Rectangle _backImageRect = Rectangle.Empty;

        private void updateBackImageRect()
        {
            if (_backImage == null)
            {
                _backImageRect = Rectangle.Empty;
                return;
            }

            if (_backAlign == Alignment.Custom)
            {
                _backImageRect.X = this.X + _backLocation.X;
                _backImageRect.Y = this.Y + _backLocation.Y;
                return;
            }

            _backImageRect.Size = _backImage.Size;

            switch (_backAlign)
            {
            case Alignment.TopLeft:
            case Alignment.MiddleLeft:
            case Alignment.BottomLeft:
                _backImageRect.X = this.X;
                break;
            case Alignment.TopCenter:
            case Alignment.MiddleCenter:
            case Alignment.BottomCenter:
                _backImageRect.X = this.X + (this.Width - _backImageRect.Width) / 2;
                break;
            case Alignment.TopRight:
            case Alignment.MiddleRight:
            case Alignment.BottomRight:
                _backImageRect.X = this.Right - _backImageRect.Width;
                break;
            }

            switch (_backAlign)
            {
            case Alignment.TopLeft:
            case Alignment.TopCenter:
            case Alignment.TopRight:
                _backImageRect.Y = this.Y;
                break;
            case Alignment.MiddleLeft:
            case Alignment.MiddleCenter:
            case Alignment.MiddleRight:
                _backImageRect.Y = this.Y + (this.Height - _backImageRect.Height) / 2;
                break;
            case Alignment.BottomLeft:
            case Alignment.BottomCenter:
            case Alignment.BottomRight:
                _backImageRect.Y = this.Bottom - _backImageRect.Height;
                break;
            }

            return;
        }

        public void Invalidate(Rectangle rect)
        {
            if (Visible && Parent != null)
                Parent.Invalidate(rect);
        }

        public override void Offset(int dx, int dy)
        {
            if (dx != 0 || dy != 0)
            {
                Rectangle oldRectangle = _widgetRectangle;
                _widgetRectangle.Offset(dx, dy);

                foreach (var w in _widgets)
                    w.Offset(dx, dy);

                if (Visible && Parent != null)
                {
                    Parent.Invalidate(oldRectangle);
                    Parent.Invalidate(_widgetRectangle);
                }

                OnMoved(EventArgs.Empty);
            }
        }

        protected override void OnMoved(EventArgs e)
        {
            Region = new Region(_widgetRectangle);
            updateBackImageRect();
            base.OnMoved(e);
        }

        protected override void OnResize(EventArgs e)
        {
            Region = new Region(_widgetRectangle);
            updateBackImageRect();
            base.OnResize(e);
        }

        #endregion

        #region " Painting "

        public override void Paint(PaintEventArgs e)
        {
            if (_backImage != null)
            {
                Rectangle dstRect = e.ClipRectangle;
                dstRect.Intersect(_backImageRect);
                if (!dstRect.IsEmpty)
                {
                    Rectangle srcRect = dstRect;
                    srcRect.X -= _backImageRect.X;
                    srcRect.Y -= _backImageRect.Y;
                    e.Graphics.DrawImage(_backImage, dstRect, srcRect, GraphicsUnit.Pixel);
                }
            }

            foreach (var w in _widgets)
            {
                if (w.Visible && w.WidgetRectangle.IntersectsWith(e.ClipRectangle))
                {
                    //if (w.Region != null) e.Graphics.FillRegion(new SolidBrush(Color.FromArgb(32, 0, 255, 0)), w.Region);
                    w.Paint(e);
                }
            }
        }

        #endregion

        #region " Mouse Events "

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDownWidget != null)
                _mouseDownWidget.OnMouseMove(e);
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            foreach (var w in _widgets)
            {
                if (w.Visible && w.Region != null && w.Region.IsVisible(e.Location))
                    _mouseDownWidget = w;
            }

            if (_mouseDownWidget != null)
                _mouseDownWidget.OnMouseDown(e);
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_mouseDownWidget != null)
            {
                _mouseDownWidget.OnMouseUp(e);
                _mouseDownWidget = null;
            }
        }

        #endregion

        #region " Data Events "

        public virtual void DataChanged(int id, int data)
        {
            foreach (var w in _widgets)
            {
                if (w is IActiveWidget)
                {
                    if (((IActiveWidget)w).ID == id)
                        ((IActiveWidget)w).Data = data;
                }
                else if (w is IWidgetContainer)
                {
                    ((IWidgetContainer)w).DataChanged(id, data);
                }
            }
        }

        public virtual void ChangeData(int id, int data)
        {
            if (Parent != null)
                Parent.ChangeData(id, data);
        }

        public int GetData(int id)
        {
            if (Parent != null)
                return Parent.GetData(id);
            return -1;
        }

        #endregion
    }
}
