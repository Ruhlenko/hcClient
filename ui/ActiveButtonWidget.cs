using System;
using System.Collections.Generic;
using System.Text;

namespace hcClient.ui
{
    class ActiveButtonWidget : ButtonWidget, IActiveWidget
    {
        private int id = 0;
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        public int Data
        {
            get { return (Active ? 1 : 0); }
            set { Active = (value > 0); }
        }

        public override void OnClick(EventArgs e)
        {
            if (!this.Disabled && Parent != null)
                Parent.ChangeData(ID, (Active ? 0 : 1));
        }

    }
}
