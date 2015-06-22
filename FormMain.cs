using hcClient.ui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace hcClient
{
    partial class FormMain : Form, IWidgetContainer
    {
        public FormMain()
        {
            InitializeComponent();

            this.SuspendLayout();

            this.BackColor = Style.Background;

            this.AddWidget(new ButtonWidget
            {
                BackColor = Style.Button,
                Location = new Point(0, 0),
                Size = new Size(60, 60),
                Image = Properties.Resources.Home_48,
                Disabled = true,
                //Text = "Test",
                //Font = this.Font
            });

            this.AddWidget(new ButtonWidget
            {
                BackColor = Style.ButtonOff,
                Location = new Point(0, 63),
                Size = new Size(160, 40),
                Text = "Test2",
                Font = this.Font
            });

            this.AddWidget(new ButtonWidget
            {
                BackColor = Style.ButtonActive,
                Font = this.Font,
                Location = new Point(0, 106),
                Size = new Size(160, 40),
                Text = "Test3"
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        #region " IWidgetContainer "

        private List<WidgetBase> _widgets = new List<WidgetBase>();
        private WidgetBase _hoveredWidget = null;
        private WidgetBase _mouseDownWidget = null;
        private WidgetBase _popupWidget = null;
        private bool _popupWidgetMustClosed = false;

        public void AddWidget(WidgetBase widget)
        {
            if (widget == null) return;

            _widgets.Add(widget);
            widget.Parent = this;
            Invalidate(widget.WidgetRectangle);
        }

        public void RemoveWidget(WidgetBase widget)
        {
            if (widget == null) return;

            _widgets.Remove(widget);
            if (_popupWidget == widget) _popupWidget = null;
            widget.Parent = null;
            Invalidate(widget.WidgetRectangle);
        }

        public void PopupWidget(WidgetBase widget, WidgetPosition position)
        {
            throw new NotImplementedException();
        }

        public void ClosePopup()
        {
            throw new NotImplementedException();
        }

        public void DataChanged(int id, int data)
        {
            throw new NotImplementedException();
        }

        public void ChangeData(int id, int data)
        {
            throw new NotImplementedException();
        }

        public int GetData(int id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region " Mouse Events "

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_popupWidget != null)
                _popupWidget.OnMouseMove(e);

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

                //if (_popupWidgetMustClosed) closePopupWidget();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            //if (_popupWidget != null && !_popupWidget.Region.IsVisible(e.Location))
            //    closePopupWidget();

            if (_hoveredWidget != null)
            {
                _mouseDownWidget = _hoveredWidget;
                _hoveredWidget.OnMouseDown(e);
            }

            //if (_popupWidgetMustClosed) closePopupWidget();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_hoveredWidget != null)
            {
                _mouseDownWidget = null;
                _hoveredWidget.OnMouseUp(e);
            }

            //if (_popupWidgetMustClosed) closePopupWidget();
        }

        #endregion

        #region " Painting "

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            foreach (var w in _widgets)
            {
                if (w.Visible && w.WidgetRectangle.IntersectsWith(e.ClipRectangle))
                    w.Paint(e);
            }
        }

        #endregion

    }
}
