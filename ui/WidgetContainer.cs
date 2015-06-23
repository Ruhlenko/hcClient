using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace hcClient.ui
{
    class WidgetContainer : WidgetBase, IWidgetContainer
    {
        #region " Widgets "

        private List<WidgetBase> _widgets = new List<WidgetBase>();
        protected WidgetBase _hoveredWidget = null;
        protected WidgetBase _mouseDownWidget = null;

        public virtual void AddWidget(WidgetBase widget)
        {
            _widgets.Add(widget);
            widget.Parent = this;
            Invalidate(widget.WidgetRectangle);
        }

        public void RemoveWidget(WidgetBase widget)
        {
            _widgets.Remove(widget);
            widget.Parent = null;
            Invalidate(widget.WidgetRectangle);
        }

        public void PopupWidget(WidgetBase widget, WidgetPosition position)
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
        private Rectangle _backImageRect = Rectangle.Empty;
        public Image BackgroundImage
        {
            get { return _backImage; }
            set
            {
                _backImage = value;
                updateBackImageRect();
                Invalidate();
            }
        }

        #endregion

        #region " Methods "

        private void updateBackImageRect()
        {
            if (_backImage == null)
            {
                _backImageRect = Rectangle.Empty;
                return;
            }
            _backImageRect.Size = _backImage.Size;
            _backImageRect.X = Left + (Width - _backImageRect.Width) / 2;
            _backImageRect.Y = Top + (Height - _backImageRect.Height) / 2;
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
            {
                _mouseDownWidget.OnMouseMove(e);
            }
            else
            {
                WidgetBase newHoveredWidget = null;

                foreach (var w in _widgets)
                {
                    if (w.Visible && w.Region != null)
                    {
                        if (w.Region.IsVisible(e.Location))
                            newHoveredWidget = w;
                    }
                }

                if (_hoveredWidget != newHoveredWidget)
                {
                    if (_hoveredWidget != null) _hoveredWidget.Hovered = false;
                    if (newHoveredWidget != null) newHoveredWidget.Hovered = true;
                    _hoveredWidget = newHoveredWidget;
                }

                if (_hoveredWidget != null) _hoveredWidget.OnMouseMove(e);
            }
        }

        public override void OnMouseLeave(EventArgs e)
        {
            if (_hoveredWidget != null)
            {
                _hoveredWidget.Hovered = false;
                _hoveredWidget = null;
            }
            base.OnMouseLeave(e);
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (_hoveredWidget != null)
            {
                _mouseDownWidget = _hoveredWidget;
                _hoveredWidget.OnMouseDown(e);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_hoveredWidget != null)
            {
                _mouseDownWidget = null;
                _hoveredWidget.OnMouseUp(e);
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
