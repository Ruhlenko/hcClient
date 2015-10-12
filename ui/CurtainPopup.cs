using System;
using System.Drawing;

namespace hcClient.ui
{
    enum CurtainModes { Stop = 0, Close = 1, Open = 2, Closed = 3, Opened = 4 };

    class CurtainPopup : Popup
    {
        #region " Constructor "

        MultiStateImageWidget _icon;
        ButtonWidget _btnOpen;
        ButtonWidget _btnStop;
        ButtonWidget _btnClose;

        public CurtainPopup()
        {
            _icon = new MultiStateImageWidget
            {
                Images = new Image[] { 
                    Properties.Resources.curtains_32_0,
                    Properties.Resources.curtains_32_1,
                    Properties.Resources.curtains_32_2,
                    Properties.Resources.curtains_32_3,
                    Properties.Resources.curtains_32_4
                },
                BasePoint = new Point(
                    Style.PopupBorder + Style.ControlButtonSize.Width / 2,
                    Style.PopupBorder + Style.ControlButtonSize.Height / 2)

            };
            AddWidget(_icon);

            _btnOpen = new ButtonWidget
            {
                Image = Properties.Resources.open_24,
                Location = new Point(
                    Style.PopupBorder,
                    Style.PopupBorder + Style.ControlButtonSize.Height + Style.ControlPadding),
                Size = Style.ControlButtonSize,
            };
            _btnOpen.Click += _btnOpen_Click;
            AddWidget(_btnOpen);

            _btnStop = new ButtonWidget
            {
                Image = Properties.Resources.stop_24,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].X,
                    this._widgets[this._widgets.Count - 1].Bottom + Style.ControlPadding),
                Size = Style.ControlButtonSize,
            };
            _btnStop.Click += _btnStop_Click;
            AddWidget(_btnStop);

            _btnClose = new ButtonWidget
            {
                Image = Properties.Resources.close_24,
                Location = new Point(
                    this._widgets[this._widgets.Count - 1].X,
                    this._widgets[this._widgets.Count - 1].Bottom + Style.ControlPadding),
                Size = Style.ControlButtonSize,
            };
            _btnClose.Click += _btnClose_Click;
            AddWidget(_btnClose);

            Size = new Size(
                Style.PopupBorder * 2 + Style.ControlButtonSize.Width,
                Style.PopupBorder * 2 + Style.ControlButtonSize.Height * 4 + Style.ControlPadding * 3);
        }

        #endregion

        #region " Data "

        private int _id = -1;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _data = -1;
        public int Data
        {
            get { return _data; }
            set
            {
                if (_data != value)
                {
                    _data = value;
                    _icon.State = _data;
                    _btnOpen.Active = (_data == (int)CurtainModes.Open);
                    _btnClose.Active = (_data == (int)CurtainModes.Close);
                }
            }
        }

        public override void DataChanged(int id, int data)
        {
            //base.DataChanged(id, data);
            if (id == _id) Data = data;
        }

        #endregion

        #region " Events "

        void _btnOpen_Click(object sender, EventArgs e)
        {
            ChangeData(_id, (int)CurtainModes.Open);
        }

        void _btnStop_Click(object sender, EventArgs e)
        {
            ChangeData(_id, (int)CurtainModes.Stop);
        }

        void _btnClose_Click(object sender, EventArgs e)
        {
            ChangeData(_id, (int)CurtainModes.Close);
        }

        #endregion
    }
}
