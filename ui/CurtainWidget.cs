using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace hcClient.ui
{
    class CurtainWidget : ButtonWidget, IActiveWidget
    {
        public CurtainWidget()
        {
            this.BackColor = Color.Transparent;

            this.Images = new Image[] {
                Properties.Resources.curtains_32_0,
                Properties.Resources.curtains_32_1,
                Properties.Resources.curtains_32_2,
                Properties.Resources.curtains_32_3,
                Properties.Resources.curtains_32_4
            };
        }



        private Image[] _images = null;
        public Image[] Images
        {
            get { return _images; }
            set
            {
                _images = value;
                updateImage();
            }
        }

        private int _id = 0;
        public int ID
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
                    updateImage();
                    Invalidate();
                }
            }
        }

        private void updateImage()
        {
            if (_images == null || _images.Length == 0)
            {
                Image = null;
                return;
            }

            if (_data < 0)
            {
                Image = _images[0]; // maybe null
            }
            else if (_data >= _images.Length)
            {
                Image = _images[_images.Length - 1];
            }
            else
            {
                Image = _images[_data];
            }
        }



        public override void OnClick(EventArgs e)
        {
            showPopup();
            base.OnClick(e);
        }



        private void showPopup()
        {
            if (Parent != null)
            {
                Parent.PopupWidget(new CurtainPopup
                {
                    Id = this.ID,
                    Data = this.Data,
                    Visible = false
                }, WidgetPosition.CursorRight);
            }
        }


        public override void Paint(PaintEventArgs e)
        {
            if (Region != null)
            {
                if (Pressed)
                    e.Graphics.FillRegion(new SolidBrush(Style.RegionPressed), Region);
                else
                    e.Graphics.FillRegion(new SolidBrush(Style.Region), Region);
            }

            base.Paint(e);
        }
    }
}
