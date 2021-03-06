﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace hcClient.ui
{
    class LevelWidget : Widget
    {
        #region " Properties "

        private Color _barActiveColor = Style.Button;
        public Color BarActiveColor
        {
            get { return _barActiveColor; }
            set
            {
                if (_barActiveColor != value)
                {
                    _barActiveColor = value;
                    Invalidate();
                }
            }
        }

        private Color _barInactiveColor = Style.Disabled(Style.Button);
        public Color BarInactiveColor
        {
            get { return _barInactiveColor; }
            set
            {
                if (_barInactiveColor != value)
                {
                    _barInactiveColor = value;
                    Invalidate();
                }
            }
        }

        private Color _backColor = Style.ButtonOff;
        public Color BackColor
        {
            get { return _backColor; }
            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    Invalidate();
                }
            }
        }

        private int _border = Style.LevelBorder;
        public int Border
        {
            get { return _border; }
            set
            {
                if (_border != value)
                {
                    _border = value;
                    Invalidate();
                }
            }
        }

        private int _maximum = 100;
        public int Maximum
        {
            get { return _maximum; }
            set { SetRange(_minimum, value); }
        }

        private int _minimum = 0;
        public int Minimum
        {
            get { return _minimum; }
            set { SetRange(value, _maximum); }
        }

        private Orientation _orientation = Orientation.Horizontal;
        public Orientation Orientation
        {
            get { return _orientation; }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    Invalidate();
                }
            }
        }

        private int _step = 10;
        public int Step
        {
            get { return _step; }
            set { _step = value; }
        }

        private int _value = 0;
        public int Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    Invalidate();
                }
            }
        }

        #endregion

        #region " Overrided Methods "

        protected override void OnMoved(EventArgs e)
        {
            base.OnMoved(e);
            Region = new Region(_widgetRectangle);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Region = new Region(_widgetRectangle);
        }

        #endregion

        #region " Public Methods "

        public void SetRange(int minValue, int maxValue)
        {
            if (minValue != _minimum || maxValue != _maximum)
            {
                if (minValue > maxValue) maxValue = minValue;
                _minimum = minValue;
                _maximum = maxValue;
                Invalidate();
            }
        }

        public int PointToValue(Point p)
        {
            Point _p = p;
            _p.Offset(-Left, -Top);
            return PositionToValue(_p, Size, _orientation, _minimum, _maximum, _step);
        }

        #endregion

        #region " Static Methods "

        private static int PositionToValue(Point p, Size s, Orientation o, int min, int max, int step)
        {
            int _value = 0;
            switch (o)
            {
                case Orientation.Horizontal:
                    _value = p.X * (max - min) / s.Width + min;
                    break;
                case Orientation.Vertical:
                    _value = (s.Height - p.Y) * (max - min) / s.Height + min;
                    break;
            }
            _value = ((_value + (step >> 1)) / step) * step;
            return _value;
        }

        private static float ValueToPosition(int value, int min, int max)
        {
            if (value < min) return 0f;
            if (value > max) return 1f;
            if (min == max) return 0f;
            return (float)(value - min) / (float)(max - min);
        }

        #endregion

        #region " Painting "

        public override void Paint(PaintEventArgs e)
        {
            var rect = _widgetRectangle;
            e.Graphics.FillRectangle(new SolidBrush(BackColor), rect);

            rect.Inflate(-_border, -_border);
            e.Graphics.FillRectangle(new SolidBrush(BarInactiveColor), rect);

            float position = ValueToPosition(Value, Minimum, Maximum);
            switch (_orientation)
            {
                case Orientation.Horizontal:
                    rect.Width = (int)(rect.Width * position + 0.5f);
                    break;
                case Orientation.Vertical:
                    int pos = (int)(rect.Height * position + 0.5f);
                    rect.Y += rect.Height - pos;
                    rect.Height = pos;
                    break;
            }
            e.Graphics.FillRectangle(new SolidBrush(BarActiveColor), rect);
        }

        #endregion
    }
}
