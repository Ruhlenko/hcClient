using System;
using System.Drawing;
using System.Windows.Forms;

namespace hcClient.ui
{
    abstract class WidgetBase
    {
        #region " Properties "

        private IWidgetContainer _parent = null;
        public IWidgetContainer Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        private bool _hovered = false;
        public bool Hovered
        {
            get { return _hovered; }
            set
            {
                if (_hovered != value)
                {
                    _hovered = value;

                    if (_hovered)
                        OnMouseEnter(EventArgs.Empty);
                    else
                        OnMouseLeave(EventArgs.Empty);

                    Invalidate();
                }
            }
        }

        private bool _visible = true;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    if (_parent != null)
                        _parent.Invalidate(_widgetRectangle);
                    OnVisibilityChanged(EventArgs.Empty);
                }
            }
        }

        protected Rectangle _widgetRectangle = Rectangle.Empty;
        public Rectangle WidgetRectangle
        {
            get { return _widgetRectangle; }
        }

        public Point Location
        {
            get { return _widgetRectangle.Location; }
            set
            {
                if (_widgetRectangle.Location != value)
                {
                    Invalidate();
                    _widgetRectangle.Location = value;
                    Invalidate();
                    OnMoved(EventArgs.Empty);
                }
            }
        }

        public Size Size
        {
            get { return _widgetRectangle.Size; }
            set
            {
                if (_widgetRectangle.Size != value)
                {
                    Invalidate();
                    _widgetRectangle.Size = value;
                    Invalidate();
                    OnResize(EventArgs.Empty);
                }
            }
        }

        public int X { get { return _widgetRectangle.X; } }
        public int Y { get { return _widgetRectangle.Y; } }

        public int Left { get { return _widgetRectangle.Left; } }
        public int Right { get { return _widgetRectangle.Right; } }
        public int Top { get { return _widgetRectangle.Top; } }
        public int Bottom { get { return _widgetRectangle.Bottom; } }

        public int Width { get { return _widgetRectangle.Width; } }
        public int Height { get { return _widgetRectangle.Height; } }

        private Region _region = null;
        public Region Region
        {
            get { return _region; }
            set { _region = value; }
        }

        #endregion

        #region " Methods "

        public virtual void Offset(int dx, int dy)
        {
            if (dx != 0 || dy != 0)
            {
                Rectangle oldRectangle = _widgetRectangle;

                _widgetRectangle.Offset(dx, dy);
                if (_region != null)
                    _region.Translate(dx, dy);

                if (_visible && _parent != null)
                {
                    _parent.Invalidate(oldRectangle);
                    _parent.Invalidate(_widgetRectangle);
                }

                OnMoved(EventArgs.Empty);
            }
        }

        public void Move(int x, int y)
        {
            int dx = x - Left;
            int dy = y - Top;
            Offset(dx, dy);
        }

        public void Invalidate()
        {
            if (_visible && _parent != null)
                _parent.Invalidate(_widgetRectangle);
        }

        #endregion

        #region " Painting "

        public abstract void Paint(PaintEventArgs e);

        #endregion

        #region " Events "

        #region " Visibility & Location & Size "

        public event EventHandler VisibilityChanged;
        protected virtual void OnVisibilityChanged(EventArgs e)
        {
            if (VisibilityChanged != null) VisibilityChanged(this, e);
        }

        public event EventHandler Moved;
        protected virtual void OnMoved(EventArgs e)
        {
            if (Moved != null) Moved(this, e);
        }

        public event EventHandler Resize;
        protected virtual void OnResize(EventArgs e)
        {
            if (Resize != null) Resize(this, e);
        }

        #endregion

        #region " Mouse Move, Enter & Leave "

        public event EventHandler<MouseEventArgs> MouseMove;
        public virtual void OnMouseMove(MouseEventArgs e)
        {
            if (MouseMove != null) MouseMove(this, e);
        }

        public event EventHandler MouseEnter;
        public virtual void OnMouseEnter(EventArgs e)
        {
            if (MouseEnter != null) MouseEnter(this, e);
        }

        public event EventHandler MouseLeave;
        public virtual void OnMouseLeave(EventArgs e)
        {
            if (MouseLeave != null) MouseLeave(this, e);
        }

        #endregion

        #region " Mouse Down & Up"

        protected bool _isMouseDownL = false;
        protected bool _isMouseDownR = false;

        public event EventHandler<MouseEventArgs> MouseDown;
        public virtual void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDownL = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _isMouseDownR = true;
            }
            if (MouseDown != null) MouseDown(this, e);
        }

        public event EventHandler<MouseEventArgs> MouseUp;
        public virtual void OnMouseUp(MouseEventArgs e)
        {
            if (MouseUp != null) MouseUp(this, e);
            if (e.Button == MouseButtons.Left)
            {
                if (_isMouseDownL && _region.IsVisible(e.Location))
                    OnClick(EventArgs.Empty);
                _isMouseDownL = false;
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (_isMouseDownR && _region.IsVisible(e.Location))
                    OnRightClick(EventArgs.Empty);
                _isMouseDownR = false;
            }
        }

        #endregion

        #region " Mouse Click "

        public event EventHandler Click;
        public virtual void OnClick(EventArgs e)
        {
            if (Click != null) Click(this, e);
        }

        public event EventHandler RightClick;
        public virtual void OnRightClick(EventArgs e)
        {
            if (RightClick != null) RightClick(this, e);
        }

        #endregion

        #endregion
    }
}
