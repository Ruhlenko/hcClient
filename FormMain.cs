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
    enum Mode { Light, Climate, Security, Settings };
    enum Floor { Floor0, Floor1, Floor2 };

    partial class FormMain : Form, IWidgetContainer
    {

        public FormMain()
        {
            InitializeComponent();

            this.SuspendLayout();

            this.BackColor = Style.Background;

            Point _mainPanelLocation = new Point(
                0,
                Style.HeaderButtonsSize.Height + Style.HeaderPadding.Vertical);

            initLight1(_mainPanelLocation);
            initLight2(_mainPanelLocation);

            initSecurity1(_mainPanelLocation);
            initSecurity2(_mainPanelLocation);

            initHeader();
            initFloors();

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

        #region " Header "

        Mode _mode = Mode.Light;

        ButtonWidget _btnLight;
        ButtonWidget _btnClimate;
        ButtonWidget _btnSecurity;
        ButtonWidget _btnSettings;

        void initHeader()
        {
            int buttonStep = Style.HeaderButtonsSize.Width + Style.HeaderPadding.Right;

            _btnSettings = new ButtonWidget
            {
                Location = new Point(
                    this.ClientSize.Width - buttonStep,
                    Style.HeaderPadding.Top),
                Size = Style.HeaderButtonsSize,
                Image = Properties.Resources.settings_48,
                Disabled = true
            };
            _btnSettings.Click += _btnSettings_Click;
            this.AddWidget(_btnSettings);

            _btnSecurity = new ButtonWidget
            {
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].X - buttonStep,
                    Style.HeaderPadding.Top),
                Size = Style.HeaderButtonsSize,
                Image = Properties.Resources.security_48,
            };
            _btnSecurity.Click += _btnSecurity_Click;
            this.AddWidget(_btnSecurity);

            _btnClimate = new ButtonWidget
            {
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].X - buttonStep,
                    Style.HeaderPadding.Top),
                Size = Style.HeaderButtonsSize,
                Image = Properties.Resources.climate_48,
                Disabled = true
            };
            _btnClimate.Click += _btnClimate_Click;
            this.AddWidget(_btnClimate);

            _btnLight = new ButtonWidget
            {
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].X - buttonStep,
                    Style.HeaderPadding.Top),
                Size = Style.HeaderButtonsSize,
                Image = Properties.Resources.light_48,
            };
            _btnLight.Click += _btnLight_Click;
            this.AddWidget(_btnLight);

            updateHeader();
        }

        void _btnLight_Click(object sender, EventArgs e)
        {
            _mode = Mode.Light;
            updateHeader();
        }

        void _btnClimate_Click(object sender, EventArgs e)
        {
            _mode = Mode.Climate;
            updateHeader();
        }

        void _btnSecurity_Click(object sender, EventArgs e)
        {
            _mode = Mode.Security;
            updateHeader();
        }

        void _btnSettings_Click(object sender, EventArgs e)
        {
            _mode = Mode.Settings;
            updateHeader();
        }

        void updateHeader()
        {
            _btnLight.Active = (_mode == Mode.Light);
            _btnClimate.Active = (_mode == Mode.Climate);
            _btnSecurity.Active = (_mode == Mode.Security);
            _btnSettings.Active = (_mode == Mode.Settings);

            updateMainPanel();
        }

        #endregion

        #region " Floor "

        Floor _floor = Floor.Floor1;

        ButtonWidget _btnFloor0;
        ButtonWidget _btnFloor1;
        ButtonWidget _btnFloor2;

        void initFloors()
        {
            Point buttonsStartLocation = new Point(
                this.ClientSize.Width - Style.FloorsButtonSize.Width - Style.FloorsPadding.Right,
                Style.HeaderButtonsSize.Height + Style.HeaderPadding.Vertical);

            _btnFloor2 = new ButtonWidget
            {
                Location = buttonsStartLocation,
                Size = Style.FloorsButtonSize,
                Font = Style.Font,
                Text = "Второй этаж",
            };
            _btnFloor2.Click += _btnFloor2_Click;
            this.AddWidget(_btnFloor2);

            _btnFloor1 = new ButtonWidget
            {
                Location = new Point(
                    buttonsStartLocation.X,
                    this._widgets[this._widgets.Count - 1].Bottom + Style.FloorsPadding.Vertical),
                Size = Style.FloorsButtonSize,
                Font = Style.Font,
                Text = "Первый этаж",
            };
            _btnFloor1.Click += _btnFloor1_Click;
            this.AddWidget(_btnFloor1);

            _btnFloor0 = new ButtonWidget
            {
                Location = new Point(
                    buttonsStartLocation.X,
                    this._widgets[this._widgets.Count - 1].Bottom + Style.FloorsPadding.Vertical),
                Size = Style.FloorsButtonSize,
                Font = Style.Font,
                Text = "Подвал",
                Disabled = true
            };
            _btnFloor0.Click += _btnFloor0_Click;
            this.AddWidget(_btnFloor0);

            updateFloorButtons();
        }

        void _btnFloor0_Click(object sender, EventArgs e)
        {
            _floor = Floor.Floor0;
            updateFloorButtons();
        }

        void _btnFloor1_Click(object sender, EventArgs e)
        {
            _floor = Floor.Floor1;
            updateFloorButtons();
        }

        void _btnFloor2_Click(object sender, EventArgs e)
        {
            _floor = Floor.Floor2;
            updateFloorButtons();
        }

        void updateFloorButtons()
        {
            _btnFloor0.Active = (_floor == Floor.Floor0);
            _btnFloor1.Active = (_floor == Floor.Floor1);
            _btnFloor2.Active = (_floor == Floor.Floor2);

            updateMainPanel();
        }

        #endregion

        #region " Light "

        WidgetContainer _panelLight1;
        WidgetContainer _panelLight2;

        void initLight1(Point location)
        {
            _panelLight1 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_1,
                Size = Style.MainPanelSize,
                Visible = false
            };

            _panelLight1.Move(location.X, location.Y);
            this.AddWidget(_panelLight1);
        }

        void initLight2(Point location)
        {
            _panelLight2 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_2,
                Size = Style.MainPanelSize,
                Visible = false
            };

            _panelLight2.Move(location.X, location.Y);
            this.AddWidget(_panelLight2);
        }

        #endregion

        #region " Security "

        WidgetContainer _panelSecurity1;
        WidgetContainer _panelSecurity2;

        void initSecurity1(Point location)
        {
            _panelSecurity1 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_1,
                Size = Style.MainPanelSize,
                Visible = false
            };

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 14,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(188, 324),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 15,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(218, 326),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 16,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(223, 412),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 17,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(254, 373),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 18,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(254, 181),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 19,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(453, 306),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 20,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(409, 314),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 21,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(429, 394),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 22,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(379, 337),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 23,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(429, 181),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 37,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(150, 326),
            });

            _panelSecurity1.AddWidget(new ActiveImageWidget
            {
                ID = 39,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(503, 226),
            });

            _panelSecurity1.Move(location.X, location.Y);
            this.AddWidget(_panelSecurity1);
        }

        void initSecurity2(Point location)
        {
            _panelSecurity2 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_2,
                Size = Style.MainPanelSize,
                Visible = true
            };

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 0,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(305, 285),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 1,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(223, 374),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 2,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(249, 316),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 3,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(181, 181),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 4,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(249, 250),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 5,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(305, 174),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 6,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(336, 232),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 7,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(429, 181),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 8,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(360, 250),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 9,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(452, 306),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 10,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(445, 275),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 11,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(447, 398),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 12,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(383, 347),
            });

            _panelSecurity2.AddWidget(new ActiveImageWidget
            {
                ID = 13,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(360, 321),
            });

            _panelSecurity2.Move(location.X, location.Y);
            this.AddWidget(_panelSecurity2);
        }

        #endregion

        void updateMainPanel()
        {
            _panelLight1.Visible = (_mode == Mode.Light && _floor == Floor.Floor1);
            _panelLight2.Visible = (_mode == Mode.Light && _floor == Floor.Floor2);

            _panelSecurity1.Visible = (_mode == Mode.Security && _floor == Floor.Floor1);
            _panelSecurity2.Visible = (_mode == Mode.Security && _floor == Floor.Floor2);
        }
    }
}
