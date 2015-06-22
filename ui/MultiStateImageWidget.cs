using System.Drawing;

namespace hcClient.ui
{
    class MultiStateImageWidget : AnimatedImageWidget
    {
        public MultiStateImageWidget()
        {
            updateImage();
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

        private int _state = -1;
        public int State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    updateImage();
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

            if (_state < 0)
            {
                Image = _images[0]; // maybe null
            }
            else if (_state >= _images.Length)
            {
                Image = _images[_images.Length - 1];
            }
            else
            {
                Image = _images[_state];
            }
        }

    }
}
