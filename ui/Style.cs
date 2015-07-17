using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace hcClient.ui
{
    static class Style
    {
        public static Size HeaderButtonsSize = new Size(60, 60);
        public static Padding HeaderPadding = new Padding(6);

        public static Size FloorsButtonSize = new Size(126, 33);
        public static Padding FloorsPadding = new Padding(6, 0, 6, 6);

        public static Size MainPanelSize = new Size(608, 528);

        public static Color Background = Color.FromArgb(0x1D, 0x21, 0x28);
        public static Color TextColor = Color.FromArgb(0xFF, 0xFF, 0xFF);

        public static Font Font = new Font("PT Sans", 12);

        public static Color Button = Color.FromArgb(0x25, 0x73, 0xDC); // Blue
        public static Color ButtonActive = Color.FromArgb(0xFF, 0x9C, 0x00); // Oranje
        public static Color ButtonOff = Color.FromArgb(0x38, 0x3C, 0x44); // Grey
        public static Color ButtonOn = ButtonActive;

        public static Color RegionPressed = Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF);

        public static Color Disabled(Color src)
        {
            int grey = (int)(src.R * 0.2126 + src.G * 0.7152 + src.B * 0.0722);
            return Color.FromArgb(src.A, grey, grey, grey);
        }

        public static Color Pressed(Color src)
        {
            // Effect strength x = 0.5
            // if src >  0.5 -> dst = src * (1 - x) + x
            // if src <= 0.5 -> dst = src * (1 + x)

            int r = (int)(src.R > 128 ? (src.R * 0.5 + 128) : (src.R * 1.5 + 0.5));
            int g = (int)(src.G > 128 ? (src.G * 0.5 + 128) : (src.G * 1.5 + 0.5));
            int b = (int)(src.B > 128 ? (src.B * 0.5 + 128) : (src.B * 1.5 + 0.5));

            return Color.FromArgb(src.A, r, g, b);
        }
    }
}
