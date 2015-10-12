using System.Drawing;

namespace hcClient.ui
{
    static class Style
    {
        public static Size HeaderButtonsSize = new Size(60, 60);
        public static int HeaderPadding = 6;

        public static Size SideButtonSize = new Size(126, 40);

        public static Size ControlButtonSize = new Size(60, 40);
        public static int ControlPadding = 6;

        public static Size MainPanelSize = new Size(668, 528);

        public static int PopupBorder = 24;

        public static Color Background = Color.FromArgb(0x1D, 0x21, 0x28);
        public static Color TextColor = Color.FromArgb(0xFF, 0xFF, 0xFF);
        public static Color PopupBackground = Color.FromArgb(0x3C, 0x44, 0x52);

        public static Font NormalFont = new Font("PT Sans", 12);
        public static Font PopupFont = new Font("PT Sans", 14);
        public static Font PlusMinusFont = new Font("PT Sans", 24);

        public static Color Button = Color.FromArgb(0x25, 0x73, 0xDC); // Blue
        public static Color ButtonActive = Color.FromArgb(0xFF, 0x9C, 0x00); // Oranje
        public static Color ButtonOff = Color.FromArgb(0x38, 0x3C, 0x44); // Grey
        public static Color ButtonOn = ButtonActive;

        public static int   LevelBorder = 12;

        public static Color Region = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);
        public static Color RegionPressed = Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF);

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

        //public static Point CenterOf(int left, int top, int right, int bottom)
        //{
        //    return new Point((left + right) / 2, (top + bottom) / 2);
        //}
    }
}
