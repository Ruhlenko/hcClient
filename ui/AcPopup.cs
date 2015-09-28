using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace hcClient.ui
{
    enum AcModes { Off = 0, Auto = 1, Heat = 2, Dry = 3, Fan = 4, Cool = 5 };

    class AcPopup : Popup
    {
        #region " Constructor "

        TemperatureWidget _txtTempr;
        ButtonWidget _btnTemprMinus;
        LevelWidget _levelTempr;
        ButtonWidget _btnTemprPlus;

        ButtonWidget _btnFanSpeed;
        ActiveImageWidget _iconFanSpeed;

        ButtonWidget _btnOff;
        ButtonWidget _btnAuto;
        ButtonWidget _btnCool;
        ButtonWidget _btnHeat;
        ButtonWidget _btnFan;
        ButtonWidget _btnDry;

        public AcPopup()
        {
            _txtTempr = new TemperatureWidget
            {
                Font = Style.PopupFont,
                TextAlign = ContentAlignment.MiddleLeft,
                ShowCelsiusSign = true,
                //HideInvalid = true,
                Location = new Point(Style.PopupBorder, Style.PopupBorder),
                Size = new Size(
                    Style.ControlButtonSize.Width * 2 + Style.ControlPadding,
                    Style.ControlButtonSize.Height),
            };
            AddWidget(_txtTempr);

            _btnTemprMinus = new ButtonWidget
            {
                Text = "−",
                Font = Style.PlusMinusFont,
                ForeColor = Style.TextColor,
                Location = new Point(
                    Style.PopupBorder,
                    Style.PopupBorder + Style.ControlButtonSize.Height + Style.ControlPadding * 2),
                Size = Style.ControlButtonSize,
            };
            _btnTemprMinus.Click += _btnTemprMinus_Click;
            AddWidget(_btnTemprMinus);

            _levelTempr = new LevelWidget
            {
                Minimum = 16,
                Maximum = 30,
                Step = 1,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].Right + Style.ControlPadding,
                    this._widgets[this._widgets.Count - 1].Y),
                Size = new Size(
                    Style.ControlButtonSize.Width * 4 + Style.ControlPadding * 3,
                    Style.ControlButtonSize.Height),
            };
            _levelTempr.MouseDown += _levelTempr_MouseEvents;
            _levelTempr.MouseMove += _levelTempr_MouseEvents;
            _levelTempr.MouseUp += _levelTempr_MouseEvents;
            AddWidget(_levelTempr);

            _btnTemprPlus = new ButtonWidget
            {
                Text = "+",
                Font = Style.PlusMinusFont,
                ForeColor = Style.TextColor,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].Right + Style.ControlPadding,
                    this._widgets[this._widgets.Count - 1].Y),
                Size = Style.ControlButtonSize,
            };
            _btnTemprPlus.Click += _btnTemprPlus_Click;
            AddWidget(_btnTemprPlus);

            _btnFanSpeed = new ButtonWidget
            {
                Location = new Point(
                    Style.PopupBorder + (Style.ControlButtonSize.Width + Style.ControlPadding) * 4,
                    Style.PopupBorder),
                Size = new Size(
                    Style.ControlButtonSize.Width * 2 + Style.ControlPadding,
                    Style.ControlButtonSize.Height)
            };
            _btnFanSpeed.Click += _btnFanSpeed_Click;
            AddWidget(_btnFanSpeed);

            _iconFanSpeed = new ActiveImageWidget
            {
                Images = new Image[]
                {
                    Properties.Resources.fan_speed_24_0,
                    Properties.Resources.fan_speed_24_1,
                    Properties.Resources.fan_speed_24_2,
                    Properties.Resources.fan_speed_24_3,
                    Properties.Resources.fan_speed_24_4
                },
                BasePoint = new Point(
                    _btnFanSpeed.X + _btnFanSpeed.Width / 2,
                    _btnFanSpeed.Y + _btnFanSpeed.Height / 2),
            };
            AddWidget(_iconFanSpeed);

            _btnOff = new ButtonWidget
            {
                Image = Properties.Resources.power_white_24,
                Location = new Point(
                    Style.PopupBorder,
                    Style.PopupBorder + (Style.ControlButtonSize.Height + Style.ControlPadding * 2) * 2),
                Size = Style.ControlButtonSize,
            };
            _btnOff.Click += _btnMode_Click;
            AddWidget(_btnOff);

            _btnAuto = new ButtonWidget
            {
                Text = "Auto",
                Font = Style.PopupFont,
                ForeColor = Style.TextColor,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].Right + Style.ControlPadding,
                    this._widgets[this._widgets.Count - 1].Y),
                Size = Style.ControlButtonSize,
            };
            _btnAuto.Click += _btnMode_Click;
            AddWidget(_btnAuto);

            _btnCool = new ButtonWidget
            {
                Image = Properties.Resources.cool_white_24,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].Right + Style.ControlPadding,
                    this._widgets[this._widgets.Count - 1].Y),
                Size = Style.ControlButtonSize,
            };
            _btnCool.Click += _btnMode_Click;
            AddWidget(_btnCool);

            _btnHeat = new ButtonWidget
            {
                Image = Properties.Resources.heat_white_24,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].Right + Style.ControlPadding,
                    this._widgets[this._widgets.Count - 1].Y),
                Size = Style.ControlButtonSize,
            };
            _btnHeat.Click += _btnMode_Click;
            AddWidget(_btnHeat);

            _btnFan = new ButtonWidget
            {
                Image = Properties.Resources.fan_white_24,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].Right + Style.ControlPadding,
                    this._widgets[this._widgets.Count - 1].Y),
                Size = Style.ControlButtonSize,
            };
            _btnFan.Click += _btnMode_Click;
            AddWidget(_btnFan);

            _btnDry = new ButtonWidget
            {
                Image = Properties.Resources.dry_white_24,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].Right + Style.ControlPadding,
                    this._widgets[this._widgets.Count - 1].Y),
                Size = Style.ControlButtonSize,
            };
            _btnDry.Click += _btnMode_Click;
            AddWidget(_btnDry);

            Size = new Size(
                Style.PopupBorder * 2 + Style.ControlButtonSize.Width * 6 + Style.ControlPadding * 5,
                Style.PopupBorder * 2 + Style.ControlButtonSize.Height * 3 + Style.ControlPadding * 4);
        }

        void updateModeButtons()
        {
            _btnOff.Active = (_dataMode == (int)AcModes.Off);
            _btnAuto.Active = (_dataMode == (int)AcModes.Auto);
            _btnCool.Active = (_dataMode == (int)AcModes.Cool);
            _btnHeat.Active = (_dataMode == (int)AcModes.Heat);
            _btnFan.Active = (_dataMode == (int)AcModes.Fan);
            _btnDry.Active = (_dataMode == (int)AcModes.Dry);
        }

        #endregion

        #region " Data "

        private int _idMode = -1;
        public int IdMode
        {
            get { return _idMode; }
            set { _idMode = value; }
        }

        private int _dataMode = -1;
        public int Mode
        {
            get { return _dataMode; }
            set
            {
                _dataMode = value;
                updateModeButtons();
            }
        }

        private int _idFan = -1;
        public int IdFan
        {
            get { return _idFan; }
            set { _idFan = value; }
        }

        private int _dataFan = -1;
        public int Fan
        {
            get { return _dataFan; }
            set
            {
                _dataFan = value;
                _iconFanSpeed.Data = value;
            }
        }

        private int _idTempr = -1;
        public int IdTempr
        {
            get { return _idTempr; }
            set { _idTempr = value; }
        }

        public int Tempr
        {
            get { return _txtTempr.Data; }
            set
            {
                _txtTempr.Data = value;
                _levelTempr.Value = value;
            }
        }

        public override void DataChanged(int id, int data)
        {
            //base.DataChanged(id, data);
            if (id == _idMode) Mode = data;
            if (id == _idFan) Fan = data;
            if (id == _idTempr) Tempr = data;
        }

        #endregion

        #region " Events "

        void _btnMode_Click(object sender, EventArgs e)
        {
            int value;

            if (sender == _btnAuto)
                value = (int)AcModes.Auto;
            else if (sender == _btnCool)
                value = (int)AcModes.Cool;
            else if (sender == _btnHeat)
                value = (int)AcModes.Heat;
            else if (sender == _btnFan)
                value = (int)AcModes.Fan;
            else if (sender == _btnDry)
                value = (int)AcModes.Dry;
            else
                value = (int)AcModes.Off;

            if (value != _dataMode)
                ChangeData(_idMode, value);
        }

        void _btnFanSpeed_Click(object sender, EventArgs e)
        {
            if (!_btnFanSpeed.Disabled)
            {
                int value = _dataFan + 1;
                if (value > 4) value = 0;

                ChangeData(_idFan, value);
            }
        }

        void _btnTemprMinus_Click(object sender, EventArgs e)
        {
            if (!_btnTemprMinus.Disabled)
            {
                int value = _levelTempr.Value - 1;
                if (value < 16) value = 16;

                ChangeData(_idTempr, value);
                _levelTempr.Value = value;
            }
        }

        void _btnTemprPlus_Click(object sender, EventArgs e)
        {
            if (!_btnTemprPlus.Disabled)
            {
                int value = _levelTempr.Value + 1;
                if (value > 30) value = 30;

                ChangeData(_idTempr, value);
                _levelTempr.Value = value;
            }
        }

        void _levelTempr_MouseEvents(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int value = _levelTempr.PointToValue(e.Location);
                if (value != _levelTempr.Value)
                {
                    ChangeData(_idTempr, value);
                    _levelTempr.Value = value;
                }
            }
        }

        #endregion
    }
}
