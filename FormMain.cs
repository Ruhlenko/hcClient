using hcClient.ui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace hcClient
{
    enum Mode { Light, Climate, Security, Settings };
    enum Floor { Floor0, Floor1, Floor2 };

    partial class FormMain : Form, IWidgetContainer
    {
        #region " TcpClient "

        TcpClient tcpClient;

        void tcpClient_DataReceived(object o, hcData e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<hcData>(tcpClient_DataReceived), o, e);
                return;
            }
            DataChanged(e.ID, e.Data);
        }

        void tcpClient_ConnectionStatusChanged(object o, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(tcpClient_ConnectionStatusChanged), o, e);
                return;
            }
            //wConnection.State = (int)tcpClient.ConnectionStatus;
        }

        #endregion

        #region " Init "

        public FormMain()
        {

            InitializeComponent();

            if (Screen.PrimaryScreen.Bounds.Width == 800 &&
                Screen.PrimaryScreen.Bounds.Height == 600)
            {
                this.ClientSize = new Size(800, 600);
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Normal;
            }

            #region " TcpClient "

            tcpClient = new TcpClient();
            tcpClient.ConnectionStatusChanged += new EventHandler(tcpClient_ConnectionStatusChanged);
            tcpClient.DataReceived += new EventHandler<hcData>(tcpClient_DataReceived);

            #endregion

            this.SuspendLayout();

            this.BackColor = Style.Background;

            Point _mainPanelLocation = new Point(
                0,
                Style.HeaderButtonsSize.Height + Style.HeaderPadding.Vertical);

            initLight0(_mainPanelLocation);
            initLight1(_mainPanelLocation);
            initLight2(_mainPanelLocation);

            initSecurity0(_mainPanelLocation);
            initSecurity1(_mainPanelLocation);
            initSecurity2(_mainPanelLocation);

            initHeader();
            initFloors();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            var iniReader = new IniFile(Defaults.SettingsFileName);

            tcpClient.RemoteAddress = iniReader.ReadValue("Network", "Server", Defaults.LocalServer);
            tcpClient.RemotePort = UInt16.Parse(iniReader.ReadValue("Network", "Port", Defaults.TcpPort.ToString()));
            tcpClient.ID = Byte.Parse(iniReader.ReadValue("Network", "ID", "0"));
            tcpClient.Connect();
        }

        #endregion

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

        #endregion

        #region " Data Events "

        public void DataChanged(int id, int data)
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

        public void ChangeData(int id, int data)
        {
            tcpClient.SetData(id, data);
        }

        public int GetData(int id)
        {
            return tcpClient.GetData(id);
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

        WidgetContainer _panelLight0;
        WidgetContainer _panelLight1;
        WidgetContainer _panelLight2;

        void initLight0(Point location)
        {
            _panelLight0 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_0,
                Size = Style.MainPanelSize,
                Visible = false
            };

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 85,
                Location = new Point(405, 140),
                Size = new Size(48, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(24, 24) },
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 86,
                Location = new Point(365, 91),
                Size = new Size(128, 147),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(8, 8),
                    new Point(51, 8),
                    new Point(94, 8),
                    new Point(116, 34),
                    new Point(116, 86),
                    new Point(116, 138),
                    new Point(73, 138),
                    new Point(30, 138),
                    new Point(8, 112),
                    new Point(8, 60),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(365,91), new Point(429,91), new Point(429,115), new Point(389,115),
                    new Point(389,214), new Point(429,214), new Point(429,238), new Point(365,238),
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 }))
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 87,
                Location = new Point(365, 91),
                Size = new Size(128, 147),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(30, 8),
                    new Point(73, 8),
                    new Point(116, 8),
                    new Point(116, 60),
                    new Point(116, 112),
                    new Point(94, 138),
                    new Point(51, 138),
                    new Point(8, 138),
                    new Point(8, 86),
                    new Point(8, 34),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(430,91), new Point(493,91), new Point(493,238), new Point(430,238),
                    new Point(430,214), new Point(469,214), new Point(469,115), new Point(430,115),
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 }))
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 88,
                Location = new Point(392, 247),
                Size = new Size(78, 200),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(8, 12), new Point(28, 12), new Point(47, 12), new Point(67, 12),
                    new Point(8, 100), new Point(28, 100), new Point(47, 100), new Point(67, 100),
                    new Point(8, 188), new Point(28, 188), new Point(47, 188), new Point(67, 188),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(392,247), new Point(470,247), new Point(470,269), new Point(392,269),
                    new Point(392,336), new Point(470,336), new Point(470,358), new Point(392,358),
                    new Point(392,425), new Point(470,425), new Point(470,447), new Point(392,447),
                }, new byte[] { 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 }))
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 89,
                Location = new Point(369, 247),
                Size = new Size(124, 200),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(12, 12),
                    new Point(12, 34),
                    new Point(12, 56),
                    new Point(12, 78),
                    new Point(12, 100),
                    new Point(12, 122),
                    new Point(12, 144),
                    new Point(12, 166),
                    new Point(12, 188),
                    new Point(111, 12),
                    new Point(111, 34),
                    new Point(111, 56),
                    new Point(111, 78),
                    new Point(111, 100),
                    new Point(111, 122),
                    new Point(111, 144),
                    new Point(111, 166),
                    new Point(111, 188),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(369,247), new Point(391,247), new Point(391,447), new Point(369,447),
                    new Point(471,247), new Point(493,247), new Point(493,447), new Point(471,447),
                }, new byte[] { 1, 1, 1, 1, 0, 1, 1, 1 }))
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 90,
                Location = new Point(392, 270),
                Size = new Size(78, 154),
                Images = new Image[] { 
                    Properties.Resources.led_01_0,
                    Properties.Resources.led_01_1
                },
                BasePoints = new Point[] {
                    new Point(39, 32),
                    new Point(39, 121),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(392,270), new Point(470,270), new Point(470,335), new Point(392,335),
                    new Point(392,359), new Point(470,359), new Point(470,424), new Point(392,424),
                }, new byte[] { 1, 1, 1, 1, 0, 1, 1, 1 })),
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 91,
                Location = new Point(314, 105),
                Size = new Size(25, 158),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(12, 12),
                    new Point(12, 39),
                    new Point(12, 66),
                    new Point(12, 92),
                    new Point(12, 119),
                    new Point(12, 146),
                },
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 92,
                Location = new Point(183, 105),
                Size = new Size(130, 158),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(12, 12),
                    new Point(38, 12),
                    new Point(65, 12),
                    new Point(91, 12),
                    new Point(117, 12),
                    new Point(12, 146),
                    new Point(38, 146),
                    new Point(65, 146),
                    new Point(91, 146),
                    new Point(117, 146),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(183,105),
                    new Point(313,105),
                    new Point(313,129),
                    new Point(183,129),
                    new Point(183,239),
                    new Point(313,239),
                    new Point(313,263),
                    new Point(183,263),
                }, new byte[] { 1, 1, 1, 1, 0, 1, 1, 1 }))
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 93,
                Location = new Point(157, 105),
                Size = new Size(25, 158),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(12, 12),
                    new Point(12, 39),
                    new Point(12, 66),
                    new Point(12, 92),
                    new Point(12, 119),
                    new Point(12, 146),
                },
            });

            _panelLight0.AddWidget(new ActiveImageWidget
            {
                ID = 94,
                Images = new Image[] {
                    Properties.Resources.lamp_w_16_0,
                    Properties.Resources.lamp_w_16_1
                },
                BasePoint = new Point(293, 357),
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 95,
                Location = new Point(293, 435),
                Size = new Size(63, 31),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] { new Point(31, 17) },
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 96,
                Location = new Point(163, 280),
                Size = new Size(121, 89),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] { new Point(60, 44) },
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 97,
                Location = new Point(105, 290),
                Size = new Size(36, 169),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] { new Point(18, 84) },
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 98,
                Location = new Point(163, 378),
                Size = new Size(121, 69),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] { new Point(60, 34) },
            });

            _panelLight0.AddWidget(new ActiveImageWidget
            {
                ID = 82,
                Images = new Image[] {
                    Properties.Resources.lamp_e_16_0,
                    Properties.Resources.lamp_e_16_1
                },
                BasePoint = new Point(356, 321),
            });

            _panelLight0.AddWidget(new ActiveImageWidget
            {
                ID = 82,
                Images = new Image[] {
                    Properties.Resources.lamp_e_16_0,
                    Properties.Resources.lamp_e_16_1
                },
                BasePoint = new Point(356, 357),
            });

            _panelLight0.AddWidget(new ActiveImageWidget
            {
                ID = 82,
                Images = new Image[] {
                    Properties.Resources.lamp_e_16_0,
                    Properties.Resources.lamp_e_16_1
                },
                BasePoint = new Point(356, 393),
            });

            _panelLight0.AddWidget(new LampArrayWidget
            {
                ID = 118,
                Location = new Point(287, 280),
                Size = new Size(75, 154),
            });

            _panelLight0.Move(location.X, location.Y);
            this.AddWidget(_panelLight0);
        }

        void initLight1(Point location)
        {
            _panelLight1 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_1,
                Size = Style.MainPanelSize,
                Visible = false
            };

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 57,
                Location = new Point(327, 335),
                Size = new Size(36, 131),
                Images = new Image[] { 
                    Properties.Resources.lamp_e_16_0,
                    Properties.Resources.lamp_e_16_1
                },
                BasePoints = new Point[] {
                    new Point(29, 22),
                    new Point(29, 58),
                    new Point(29, 94),
                },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 66,
                Location = new Point(391, 146),
                Size = new Size(48, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(24, 24) },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 67,
                Location = new Point(352, 107),
                Size = new Size(126, 126),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(35, 14),
                    new Point(63, 6),
                    new Point(92, 14),
                    new Point(112, 35),
                    new Point(120, 63),
                    new Point(112, 92),
                    new Point(92, 112),
                    new Point(63, 120),
                    new Point(35, 112),
                    new Point(14, 92),
                    new Point(6, 63),
                    new Point(14, 35),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(355,110), new Point(475,110), new Point(475,230), new Point(355,230),
                    new Point(355,145), new Point(390,145),
                    new Point(390,195), new Point(440,195), new Point(440,145), new Point(355,145)
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }))
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 70,
                Location = new Point(290, 90),
                Size = new Size(36, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(18, 24) },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 71,
                Location = new Point(269, 90),
                Size = new Size(78, 164),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(9, 15), new Point(68, 15),
                    new Point(9, 149), new Point(38, 140), new Point(68, 149),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(269,90), new Point(289,90), new Point(289,120), new Point(269,120),
                    new Point(327,90), new Point(347,90), new Point(347,120), new Point(327,120),
                    new Point(269,215), new Point(347,215), new Point(347,254), new Point(269,254),
                }, new byte[] { 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1 }))
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 72,
                Location = new Point(184, 151),
                Size = new Size(48, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(24, 24) },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 73,
                Location = new Point(139, 106),
                Size = new Size(138, 138),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(38, 14),
                    new Point(69, 6),
                    new Point(101, 14),
                    new Point(124, 38),
                    new Point(132, 69),
                    new Point(124, 101),
                    new Point(101, 124),
                    new Point(69, 132),
                    new Point(38, 124),
                    new Point(14, 101),
                    new Point(6, 69),
                    new Point(14, 38),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(142,109),
                    new Point(268,109), new Point(268,121), new Point(274,121),
                    new Point(274,214), new Point(268,214), new Point(268,241),
                    new Point(142,241),
                    new Point(142,150), new Point(183,150),
                    new Point(183,200), new Point(233,200), new Point(233,150), new Point(142,150)
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }))
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 74,
                Location = new Point(426, 280),
                Size = new Size(36, 53),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] { new Point(18, 26) },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 75,
                Location = new Point(414, 280),
                Size = new Size(79, 53),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(14, 12),
                    new Point(14, 40),
                    new Point(66, 12),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(463,280), new Point(493,280), new Point(493,333), new Point(463,333),
                }, new byte[] { 1, 1, 1, 1 }))
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 77,
                Location = new Point(411, 376),
                Size = new Size(36, 36),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(18, 18) },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 78,
                Location = new Point(385, 361),
                Size = new Size(88, 66),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(6, 6),
                    new Point(31, 6),
                    new Point(57, 6),
                    new Point(82, 6),
                    new Point(82, 33),
                    new Point(82, 60),
                    new Point(57, 60),
                    new Point(31, 60),
                    new Point(6, 60),
                    new Point(6, 33),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(385,361), new Point(473,361), new Point(473,427), new Point(385,427), new Point(385, 375),
                    new Point(410,375), new Point(410,413), new Point(448,413), new Point(448,375), new Point(385, 375),
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })),
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 79,
                Location = new Point(365, 342),
                Size = new Size(128, 105),
                Images = new Image[] { 
                    Properties.Resources.led_11_0,
                    Properties.Resources.led_11_1
                },
                BasePoints = new Point[] { new Point(64, 52) },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(365,342), new Point(493,342), new Point(493,447), new Point(365,447), new Point(365, 360),
                    new Point(384,360), new Point(384,428), new Point(474,428), new Point(474,360), new Point(365, 360),
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }))
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 80,
                Location = new Point(297, 285),
                Size = new Size(36, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(18, 24) },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 81,
                Location = new Point(239, 285),
                Size = new Size(151, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(14, 24),
                    new Point(43, 24),
                    new Point(109, 24),
                    new Point(138, 24),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(239,285), new Point(296,285), new Point(296,333), new Point(239,333),
                    new Point(334,285), new Point(391,285), new Point(391,333), new Point(334,333),
                }, new byte[] { 1, 1, 1, 1, 0, 1, 1, 1 }))
            });

            _panelLight1.AddWidget(new ActiveImageWidget
            {
                ID = 82,
                Images = new Image[] { 
                    Properties.Resources.lamp_w_16_0,
                    Properties.Resources.lamp_w_16_1
                },
                BasePoint = new Point(293, 393),
            });

            _panelLight1.AddWidget(new ActiveImageWidget
            {
                ID = 82,
                Images = new Image[] { 
                    Properties.Resources.lamp_w_16_0,
                    Properties.Resources.lamp_w_16_1
                },
                BasePoint = new Point(293, 429),
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 83,
                Location = new Point(163, 280),
                Size = new Size(51, 89),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] { new Point(26, 45) },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 84,
                Location = new Point(163, 378),
                Size = new Size(121, 69),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] { new Point(61, 35) },
            });

            _panelLight1.AddWidget(new LampArrayWidget
            {
                ID = 118,
                Location = new Point(286, 371),
                Size = new Size(36, 95),
            });

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

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 40,
                Location = new Point(405, 157),
                Size = new Size(48, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(24, 24) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 41,
                Location = new Point(363, 89),
                Size = new Size(132, 184),
                Images = new Image[] {
                    Properties.Resources.led_21_0,
                    Properties.Resources.led_21_1
                },
                BasePoints = new Point[] { new Point(76, 90) },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(380, 132),
                    new Point(478, 132),
                    new Point(478, 230),
                    new Point(380, 230),
                    new Point(380, 156),
                    new Point(404, 156),
                    new Point(404, 206),
                    new Point(454, 206),
                    new Point(454, 156),
                    new Point(380, 156),
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })),
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 42,
                Location = new Point(365, 91),
                Size = new Size(128, 180),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(108, 43),
                    new Point(89, 19),
                    new Point(57, 10),
                    new Point(26, 22),
                    new Point(16, 52),
                    new Point(12, 88),
                    new Point(18, 130),
                    new Point(40, 160),
                    new Point(72, 166),
                    new Point(97, 140),
                    new Point(113, 106),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(365, 91),
                    new Point(493, 91),
                    new Point(493, 131),
                    new Point(379, 131),
                    new Point(379, 231),
                    new Point(493, 231),
                    new Point(493, 271),
                    new Point(365, 271),
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1 })),
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 43,
                Location = new Point(479, 136),
                Size = new Size(18, 36),
                Images = new Image[] { 
                    Properties.Resources.lamp_e_16_0,
                    Properties.Resources.lamp_e_16_1
                },
                BasePoints = new Point[] { new Point(14, 26) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 44,
                Location = new Point(479, 173),
                Size = new Size(18, 36),
                Images = new Image[] { 
                    Properties.Resources.lamp_e_16_0,
                    Properties.Resources.lamp_e_16_1
                },
                BasePoints = new Point[] { new Point(14, 10) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 45,
                Location = new Point(411, 280),
                Size = new Size(82, 53),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(42, 28) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 46,
                Location = new Point(436, 379),
                Size = new Size(48, 36),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(24, 18) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 47,
                Location = new Point(365, 299),
                Size = new Size(128, 148),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(18, 10),
                    new Point(18, 37),
                    new Point(18, 63),
                    new Point(18, 90),
                    new Point(15, 132),
                    new Point(54, 132),
                    new Point(83, 132),
                    new Point(111, 132),
                    new Point(83, 58),
                    new Point(111, 58),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(365,299),
                    new Point(402,299),
                    new Point(402,414),
                    new Point(393,414),
                    new Point(393,447),
                    new Point(365,447),
                    new Point(402,416),
                    new Point(493,416),
                    new Point(493,447),
                    new Point(438,447),
                    new Point(438,432),
                    new Point(402,432),
                    new Point(427,350),
                    new Point(493,350),
                    new Point(493,372),
                    new Point(427,372),
                }, new byte[] { 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1 }))
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 48,
                Location = new Point(401, 433),
                Size = new Size(36, 18),
                Images = new Image[] { 
                    Properties.Resources.lamp_s_16_0,
                    Properties.Resources.lamp_s_16_1
                },
                BasePoints = new Point[] { new Point(18, 14) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 50,
                Location = new Point(296, 139),
                Size = new Size(36, 36),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(18, 18) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 51,
                Location = new Point(254, 61),
                Size = new Size(102, 51),
                Images = new Image[]
                {
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] 
                {
                    new Point(14, 19),
                    new Point(40, 13),
                    new Point(66, 19),
                    new Point(92, 38),
                },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 52,
                Location = new Point(269, 121),
                Size = new Size(87, 107),
                Images = new Image[]
                {
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(13, 10),
                    new Point(10, 36),
                    new Point(6, 62),
                    new Point(23, 81),
                    new Point(45, 84),
                    new Point(61, 62),
                    new Point(67, 93),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(269,121),
                    new Point(295,121),
                    new Point(295,176),
                    new Point(356,176),
                    new Point(356,228),
                    new Point(269,228),
                }, new byte[] { 1, 1, 1, 1, 1, 1 }))
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 53,
                Location = new Point(250, 119),
                Size = new Size(18, 36),
                Images = new Image[]
                {
                    Properties.Resources.lamp_w_16_0,
                    Properties.Resources.lamp_w_16_1
                },
                BasePoints = new Point[] { new Point(4, 18) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 54,
                Location = new Point(254, 237),
                Size = new Size(102, 96),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(23, 27),
                    new Point(52, 27),
                    new Point(81, 27),
                    new Point(23, 69),
                    new Point(52, 69),
                    new Point(81, 69),
                },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 55,
                Location = new Point(297, 335),
                Size = new Size(59, 74),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(9, 22),
                    new Point(30, 18),
                    new Point(51, 22),
                    new Point(9, 62),
                    new Point(30, 58),
                    new Point(51, 62),
                },
                Region = new Region(new Rectangle(307, 335, 49, 74)),
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 56,
                Location = new Point(307, 410),
                Size = new Size(36, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(18, 24) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 57,
                Location = new Point(282, 335),
                Size = new Size(24, 131),
                Images = new Image[] { 
                    Properties.Resources.lamp_w_16_0,
                    Properties.Resources.lamp_w_16_1
                },
                BasePoints = new Point[] {
                    new Point(11, 22),
                    new Point(11, 58),
                    new Point(11, 94),
                },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 58,
                Location = new Point(157, 151),
                Size = new Size(48, 48),
                Images = new Image[] { 
                    Properties.Resources.lamp_24_0,
                    Properties.Resources.lamp_24_1
                },
                BasePoints = new Point[] { new Point(24, 24) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 59,
                Location = new Point(209, 91),
                Size = new Size(36, 163),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(18, 18),
                    new Point(18, 50),
                    new Point(18, 83),
                    new Point(18, 115),
                    new Point(18, 147),
                },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(209,91), new Point(245,91),
                    new Point(245,110), new Point(230,110), new Point(230,148), new Point(245,148),
                    new Point(245,200), new Point(230,200), new Point(230,238), new Point(245,238),
                    new Point(245,254), new Point(209,254),
                }, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }))

            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 60,
                Location = new Point(153, 255),
                Size = new Size(92, 24),
                Images = new Image[] { 
                    Properties.Resources.lamp_8_0,
                    Properties.Resources.lamp_8_1
                },
                BasePoints = new Point[] {
                    new Point(10, 12),
                    new Point(56, 12),
                    new Point(76, 12),
                },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 61,
                Location = new Point(231, 111),
                Size = new Size(18, 36),
                Images = new Image[] { 
                    Properties.Resources.lamp_e_16_0,
                    Properties.Resources.lamp_e_16_1
                },
                BasePoints = new Point[] { new Point(14, 18) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 62,
                Location = new Point(231, 201),
                Size = new Size(24, 36),
                Images = new Image[] { 
                    Properties.Resources.lamp_e_16_0,
                    Properties.Resources.lamp_e_16_1
                },
                BasePoints = new Point[] { new Point(14, 18) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 63,
                Location = new Point(197, 328),
                Size = new Size(36, 36),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] { new Point(18, 18) },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 64,
                Location = new Point(234, 342),
                Size = new Size(47, 105),
                Images = new Image[] { 
                    Properties.Resources.lamp_16_0,
                    Properties.Resources.lamp_16_1
                },
                BasePoints = new Point[] {
                    new Point(29, 22),
                    new Point(19, 57),
                    new Point(13, 93),
                },
            });

            _panelLight2.AddWidget(new LampArrayWidget
            {
                ID = 65,
                Location = new Point(160, 294),
                Size = new Size(92, 156),
                Images = new Image[] { 
                    Properties.Resources.led_22_0,
                    Properties.Resources.led_22_1
                },
                BasePoints = new Point[] { new Point(44, 66) },
                Region = new Region(new GraphicsPath(new Point[] { 
                    new Point(179,301), new Point(245,301), new Point(245,327), new Point(179,327),
                    new Point(163,365), new Point(233,365), new Point(233,422), new Point(163,422),
                }, new byte[] { 1, 1, 1, 1, 0, 1, 1, 1 }))
            });

            _panelLight2.Move(location.X, location.Y);
            this.AddWidget(_panelLight2);
        }

        #endregion

        #region " Security "

        WidgetContainer _panelSecurity0;
        WidgetContainer _panelSecurity1;
        WidgetContainer _panelSecurity2;

        void initSecurity0(Point location)
        {
            _panelSecurity0 = new WidgetContainer
            {
                BackgroundImage = Properties.Resources.floor_0,
                Size = Style.MainPanelSize,
                Visible = false
            };

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 24,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(429, 164),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 25,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(433, 242),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 26,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(429, 347),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 27,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(345, 259),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 28,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(237, 181),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 29,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(322, 275),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 30,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(324, 298),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 31,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(223, 324),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 32,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(266, 275),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 33,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(150, 327),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 34,
                Images = new Image[] {
                    Properties.Resources.movement_24_0,
                    Properties.Resources.movement_24_1
                },
                BasePoint = new Point(223, 412),
            });

            _panelSecurity0.AddWidget(new ActiveImageWidget
            {
                ID = 35,
                Images = new Image[] {
                    Properties.Resources.door_24_0,
                    Properties.Resources.door_24_1
                },
                BasePoint = new Point(198, 373),
            });

            _panelSecurity0.Move(location.X, location.Y);
            this.AddWidget(_panelSecurity0);
        }

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
            _panelLight0.Visible = (_mode == Mode.Light && _floor == Floor.Floor0);
            _panelLight1.Visible = (_mode == Mode.Light && _floor == Floor.Floor1);
            _panelLight2.Visible = (_mode == Mode.Light && _floor == Floor.Floor2);

            _panelSecurity0.Visible = (_mode == Mode.Security && _floor == Floor.Floor0);
            _panelSecurity1.Visible = (_mode == Mode.Security && _floor == Floor.Floor1);
            _panelSecurity2.Visible = (_mode == Mode.Security && _floor == Floor.Floor2);
        }
    }
}
