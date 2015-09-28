using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace hcClient.ui
{
    class AcWidget : WidgetContainer
    {
        #region " Constructor "

        ActiveImageWidget _mode;
        TemperatureWidget _tempr;

        public AcWidget()
        {
            _mode = new ActiveImageWidget
            {
                Images = new Image[] {
                    Properties.Resources.power_white_24,
                    Properties.Resources.auto_color_24,
                    Properties.Resources.heat_color_24,
                    Properties.Resources.dry_color_24,
                    Properties.Resources.fan_white_24,
                    Properties.Resources.cool_color_24
                },
                BasePoint = new Point(32, 32)
            };
            AddWidget(_mode);

            _tempr = new TemperatureWidget
            {
                ShowCelsiusSign = true,
                //HideInvalid = true,
                Font = Style.NormalFont,
                Location = new Point(_mode.Right, _mode.Top),
                Size = new Size(48, 24)
            };
            AddWidget(_tempr);

            Size = new Size(
              _mode.X * 2 + _mode.Width + _tempr.Width,
              _mode.Y * 2 + _mode.Height);
        }

        #endregion

        #region " Data "

        public int IdMode
        {
            get { return _mode.ID; }
            set { _mode.ID = value; }
        }

        private int _idFan = -1;
        private int _dataFan = -1;
        public int IdFan
        {
            get { return _idFan; }
            set { _idFan = value; }
        }

        public int IdTempr
        {
            get { return _tempr.ID; }
            set { _tempr.ID = value; }
        }

        public override void DataChanged(int id, int data)
        {
            //base.DataChanged(id, data);
            if (id == _mode.ID) _mode.Data = data;
            if (id == _idFan) _dataFan = data;
            if (id == _tempr.ID) _tempr.Data = data;
        }

        #endregion

        #region " Painting "

        public override void Paint(PaintEventArgs e)
        {
            base.Paint(e);
            if (Region != null)
            {
                if (_pressed)
                    e.Graphics.FillRegion(new SolidBrush(Style.RegionPressed), Region);
                else
                    e.Graphics.FillRegion(new SolidBrush(Style.Region), Region);
            }
        }

        #endregion

        #region " Events "

        private bool _pressed = false;

        public override void OnMouseDown(MouseEventArgs e)
        {
            _pressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        public override void OnClick(EventArgs e)
        {
            showPopup();
            base.OnClick(e);
        }

        public override void OnRightClick(EventArgs e)
        {
            showPopup();
            base.OnRightClick(e);
        }

        private void showPopup()
        {
            if (Parent != null)
            {
                Parent.PopupWidget(new AcPopup
                {
                    IdMode = _mode.ID,
                    Mode = _mode.Data,
                    IdFan = _idFan,
                    Fan = _dataFan,
                    IdTempr = _tempr.ID,
                    Tempr = _tempr.Data,
                    Visible = false
                }, WidgetPosition.Center);
            }
        }

        #endregion

    }
}
