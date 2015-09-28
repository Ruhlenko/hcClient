using System;
using System.Collections.Generic;
using System.Text;

namespace hcClient.ui
{
    class TemperatureWidget : TextWidget, IActiveWidget
    {
        public TemperatureWidget()
        {
            updateText();
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
                    updateText();
                }
            }
        }

        private bool _showCelsiusSign = false;
        public bool ShowCelsiusSign
        {
            get { return _showCelsiusSign; }
            set
            {
                if (_showCelsiusSign != value)
                {
                    _showCelsiusSign = value;
                    updateText();
                }
            }
        }

        private bool _hideInvalid = false;
        public bool HideInvalid
        {
            get { return _hideInvalid; }
            set
            {
                if (_hideInvalid != value)
                {
                    _hideInvalid = value;
                    updateText();
                }
            }
        }

        private void updateText()
        {
            if (_data <= 0 || _data >= 100)
            {
                if (_hideInvalid) Visible = false;

                if (_showCelsiusSign)
                    Text = "--- °C";
                else
                    Text = "---";
            }
            else
            {
                if (_showCelsiusSign)
                    Text = String.Format("{0} °C", _data);
                else
                    Text = _data.ToString();

                if (_hideInvalid) Visible = true;
            }
        }
    }
}
