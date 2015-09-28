using System.Drawing;
using System.Windows.Forms;

namespace hcClient.ui
{
    class Popup : WidgetContainer
    {
        private Color _backColor = Style.PopupBackground;
        public Color BackColor
        {
            get { return _backColor; }
            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    createBackgroundImage();
                }
            }
        }

        private int _border = 0;
        public int Border
        {
            get { return _border; }
            set
            {
                if (_border != value)
                {
                    _border = value;
                    createBackgroundImage();
                }
            }
        }

        public override void Paint(PaintEventArgs e)
        {
            if (BackgroundImage == null)
                createBackgroundImage();

            base.Paint(e);
        }

        private void createBackgroundImage()
        {
            if (Size.IsEmpty) return;

            var bmp = new Bitmap(Width, Height);
            var g = Graphics.FromImage(bmp);

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            rect.Inflate(-_border, -_border);
            g.FillRectangle(new SolidBrush(_backColor), rect);

            //var nw = Properties.Resources.Bg_NW;
            //var n = Properties.Resources.Bg_N;
            //var ne = Properties.Resources.Bg_NE;
            //var w = Properties.Resources.Bg_W;
            //var e = Properties.Resources.Bg_E;
            //var sw = Properties.Resources.Bg_SW;
            //var s = Properties.Resources.Bg_S;
            //var se = Properties.Resources.Bg_SE;

            //using (var br = new TextureBrush(n))
            //{
            //    br.TranslateTransform(nw.Width, 0);
            //    g.FillRectangle(br, nw.Width, 0, bmp.Width - nw.Width - ne.Width, n.Height);
            //}
            //using (var br = new TextureBrush(w))
            //{
            //    br.TranslateTransform(0, nw.Height);
            //    g.FillRectangle(br, 0, nw.Height, w.Width, bmp.Height - nw.Height - sw.Height);
            //}
            //using (var br = new TextureBrush(e))
            //{
            //    br.TranslateTransform(bmp.Width - e.Width, ne.Height);
            //    g.FillRectangle(br, bmp.Width - e.Width, ne.Height, e.Width, bmp.Height - nw.Height - sw.Height);
            //}
            //using (var br = new TextureBrush(s))
            //{
            //    br.TranslateTransform(sw.Width, bmp.Height - s.Height);
            //    g.FillRectangle(br, sw.Width, bmp.Height - s.Height, bmp.Width - sw.Width - se.Width, s.Height);
            //}

            //g.DrawImage(nw, 0, 0);
            //g.DrawImage(ne, bmp.Width - ne.Width, 0);
            //g.DrawImage(sw, 0, bmp.Height - sw.Height);
            //g.DrawImage(se, bmp.Width - se.Width, bmp.Height - se.Height);

            BackgroundImage = bmp;
        }

    }
}
