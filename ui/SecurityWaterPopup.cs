using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace hcClient.ui
{
    enum SecurityWaterState { Undefined = -1, Enabled = 0, Closed = 1, Paused = 2, Disabled = 3 };

    class SecurityWaterPopup : Popup
    {
        #region " Constructor "

        MultiStateImageWidget _imgFaucet;
        MultiStateImageWidget _imgWater;
        TextWidget _txtState;

        ButtonWidget _btnClose;
        ButtonWidget _btnOpen;

        ButtonWidget _btnEnable;
        ButtonWidget _btnPause;
        ButtonWidget _btnDisable;

        public SecurityWaterPopup()
        {
            this.Size = new Size(
                Style.PopupBorder * 2 + Style.ControlButtonSize.Width * 4 + Style.ControlPadding * 3,
                Style.PopupBorder * 2 + 64 + Style.ControlButtonSize.Height * 7 + Style.ControlPadding * 8);

            Size buttonsSize = new Size(
                Style.ControlButtonSize.Width * 4 + Style.ControlPadding * 3,
                Style.ControlButtonSize.Height);

            _imgFaucet = new MultiStateImageWidget
            {
                BasePoint = new Point(Style.PopupBorder + 32, Style.PopupBorder + 32),
                Images = new Image[] {
                    Properties.Resources.faucet_64_0,
                    Properties.Resources.faucet_64_1,
                    Properties.Resources.faucet_64_0,
                    Properties.Resources.faucet_64_0
                },
            };
            AddWidget(_imgFaucet);

            _imgWater = new MultiStateImageWidget
            {
                BasePoint = new Point(Style.PopupBorder + 64 + Style.ControlPadding + 32, Style.PopupBorder + 32),
                Images = new Image[] {
                    null,
                    Properties.Resources.water_48_1,
                },
            };
            AddWidget(_imgWater);

            _txtState = new TextWidget
            {
                Location = new Point(Style.PopupBorder, Style.PopupBorder + 64 + Style.ControlPadding),
                Size = new Size(
                    this.Size.Width - Style.PopupBorder * 2,
                    Style.ControlButtonSize.Height * 2 + Style.ControlPadding),
                Font = Style.NormalFont,
                TextAlign = Alignment.TopLeft,
            };
            AddWidget(_txtState);

            _btnOpen = new ButtonWidget
            {
                Location = new Point(Style.PopupBorder, _txtState.Bottom + Style.ControlPadding),
                Size = buttonsSize,
                Font = Style.NormalFont,
                Text = "Открыть воду"
            };
            _btnOpen.Click += btnOpen_Click;
            AddWidget(_btnOpen);

            _btnClose = new ButtonWidget
            {
                Location = new Point(_btnOpen.X, _btnOpen.Bottom + Style.ControlPadding),
                Size = buttonsSize,
                Font = Style.NormalFont,
                Text = "Закрыть воду"
            };
            _btnClose.Click += btnClose_Click;
            AddWidget(_btnClose);

            _btnEnable = new ButtonWidget
            {
                Location = new Point(_btnClose.X, _btnClose.Bottom + Style.ControlPadding * 2),
                Size = buttonsSize,
                Font = Style.NormalFont,
                Text = "Включить защиту"
            };
            _btnEnable.Click += btnEnable_Click;
            AddWidget(_btnEnable);

            _btnPause = new ButtonWidget
            {
                Location = new Point(_btnEnable.X, _btnEnable.Bottom + Style.ControlPadding),
                Size = buttonsSize,
                Font = Style.NormalFont,
                Text = "Отключить защиту на 1 час"
            };
            _btnPause.Click += btnPause_Click;
            AddWidget(_btnPause);

            _btnDisable = new ButtonWidget
            {
                Location = new Point(_btnPause.X, _btnPause.Bottom + Style.ControlPadding),
                Size = buttonsSize,
                Font = Style.NormalFont,
                Text = "Отключить защиту"
            };
            _btnDisable.Click += btnDisable_Click;
            AddWidget(_btnDisable);

            updateText();
        }

        #endregion

        #region " Properties "

        private int _idControl = -1;
        public int IdControl
        {
            get { return _idControl; }
            set { _idControl = value; }
        }

        private int _dataControl = -1;
        public int DataControl
        {
            get { return _dataControl; }
            set
            {
                if (_dataControl != value)
                {
                    _dataControl = value;
                    _imgFaucet.State = _dataControl;
                    updateText();
                }
            }
        }

        private int _idWater = -1;
        public int IdWater
        {
            get { return _idWater; }
            set { _idWater = value; }
        }

        private int _dataWater = -1;
        public int DataWater
        {
            get { return _dataWater; }
            set
            {
                if (_dataWater != value)
                {
                    _dataWater = value;
                    _imgWater.State = _dataWater;
                    updateText();
                }
            }
        }

        private int _idTimer = -1;
        public int IdTimer
        {
            get { return _idTimer; }
            set { _idTimer = value; }
        }

        private int _dataTimer = -1;
        public int DataTimer
        {
            get { return _dataTimer; }
            set
            {
                _dataTimer = value;
                updateText();
            }
        }

        #endregion

        #region " Data "

        public override void DataChanged(int id, int data)
        {
            base.DataChanged(id, data);

            if (id == _idControl)
            {
                DataControl = data;
            }
            else if (id == _idWater)
            {
                DataWater = data;
            }
            else if (id == _idTimer)
            {
                DataTimer = data;
            }
        }

        #endregion

        #region " Methods "

        private void updateText()
        {
            switch (_dataControl)
            {
            case (int)SecurityWaterState.Enabled:
                _txtState.Text = "Подача воды: открыто\n\nЗащита от протечек: включена";

                _btnOpen.Disabled = true;
                _btnClose.Disabled = false;

                _btnEnable.Disabled = true;
                _btnPause.Disabled = false;
                _btnDisable.Disabled = false;
                break;

            case (int)SecurityWaterState.Closed:
                _txtState.Text = "Подача воды: закрыто\n\nЗащита от протечек: включена";

                _btnOpen.Disabled = (_dataWater != 0);
                _btnClose.Disabled = true;

                _btnEnable.Disabled = true;
                _btnPause.Disabled = false;
                _btnDisable.Disabled = false;
                break;

            case (int)SecurityWaterState.Paused:
                _txtState.Text = String.Format(
                    "Подача воды: открыто\n\nЗащита от протечек: отключена\n(включение через {0} мин.)",
                    _dataTimer / 60);

                _btnOpen.Disabled = true;
                _btnClose.Disabled = false;

                _btnEnable.Disabled = false;
                _btnPause.Disabled = true;
                _btnDisable.Disabled = false;
                break;

            case (int)SecurityWaterState.Disabled:
                _txtState.Text = "Подача воды: открыто\n\nЗащита от протечек: отключена";

                _btnOpen.Disabled = true;
                _btnClose.Disabled = false;

                _btnEnable.Disabled = false;
                _btnPause.Disabled = false;
                _btnDisable.Disabled = true;
                break;

            default:
                _txtState.Text = "Подача воды: ---\n\nЗащита от протечек: ---";

                _btnOpen.Disabled = true;
                _btnClose.Disabled = true;

                _btnEnable.Disabled = true;
                _btnPause.Disabled = true;
                _btnDisable.Disabled = true;
                break;
            }
        }

        void btnOpen_Click(object sender, EventArgs e)
        {
            if (!_btnOpen.Disabled)
            {
                ChangeData(IdControl, (int)SecurityWaterState.Enabled);
            }
        }

        void btnClose_Click(object sender, EventArgs e)
        {
            if (!_btnClose.Disabled)
            {
                ChangeData(IdControl, (int)SecurityWaterState.Closed);
            }
        }

        void btnEnable_Click(object sender, EventArgs e)
        {
            if (!_btnEnable.Disabled)
            {
                ChangeData(IdControl, (int)SecurityWaterState.Enabled);
            }
        }

        void btnPause_Click(object sender, EventArgs e)
        {
            if (!_btnPause.Disabled)
            {
                ChangeData(IdControl, (int)SecurityWaterState.Paused);
            }
        }

        void btnDisable_Click(object sender, EventArgs e)
        {
            if (!_btnDisable.Disabled)
            {
                ChangeData(IdControl, (int)SecurityWaterState.Disabled);
            }
        }

        #endregion

    }
}
