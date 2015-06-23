namespace hcClient.ui
{
    class ActiveImageWidget : MultiStateImageWidget, IActiveWidget
    {
        private int id = 0;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public int Data
        {
            get { return State; }
            set { State = value; }
        }
    }
}
