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
                Location = new Point(
                    this.ClientSize.Width - Style.TopBarButtonsSize.Width - Style.TopBarButtonSpacing,
                    Style.TopBarButtonSpacing),
                Size = Style.TopBarButtonsSize,
                Image = Properties.Resources.security_48,
            });

            this.AddWidget(new ButtonWidget
            {
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].X  - Style.TopBarButtonsSize.Width - Style.TopBarButtonSpacing,
                    Style.TopBarButtonSpacing),
                Size = Style.TopBarButtonsSize,
                Image = Properties.Resources.home_48,
            });

            Point _mainPanelLocation = new Point(
                (this.ClientSize.Width - Style.MainPanelSize.Width) / 2,
                Style.TopBarButtonsSize.Height + 2 * Style.TopBarButtonSpacing);

            initSecurity1(_mainPanelLocation);
            initSecurity2(_mainPanelLocation);

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

        #region " Security "

        WidgetContainer panelSecurity1;
        WidgetContainer panelSecurity2;

        void initSecurity1(Point location)
        {
            panelSecurity1 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_1,
                Size = Style.MainPanelSize,
                Visible = false
            };

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 14,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(188, 324),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 15,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(218, 326),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 16,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(223, 412),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 17,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(254, 373),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 18,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(254, 181),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 19,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(453, 306),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 20,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(409, 314),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 21,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(429, 394),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 22,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(379, 337),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 23,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(429, 181),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 37,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(150, 326),
            });

            panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 39,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(503, 226),
            });

            panelSecurity1.Move(location.X, location.Y);
            this.AddWidget(panelSecurity1);
        }

        void initSecurity2(Point location)
        {
            panelSecurity2 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_2,
                Size = Style.MainPanelSize,
                Visible = true
            };

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 0,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(305, 285),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 1,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(223, 374),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 2,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(249, 316),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 3,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(181, 181),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 4,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(249, 250),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 5,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(305, 174),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 6,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(336, 232),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 7,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(429, 181),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 8,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(360, 250),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 9,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(452, 306),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 10,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(445, 275),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 11,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(447, 398),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 12,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(383, 347),
            });

            panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 13,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(360, 321),
            });

            panelSecurity2.Move(location.X, location.Y);
            this.AddWidget(panelSecurity2);
        }

        #endregion
    }
}
