using System.Collections.Generic;
using System.Windows.Forms;

namespace FormLib
{
    public class ThreadHandler
    {
        private PositionsForm _eventform;
        private PositionsForm _pform
        {
            get { return _eventform; }
            set
            {
                if (_eventform != null)
                {
                    _eventform.open_Click -= _pform_open_Click;
                    _eventform.close_Click -= _pform_close_Click;

                }
                _eventform = value;
                if (_eventform != null)
                {
                    _eventform.open_Click += _pform_open_Click;
                    _eventform.close_Click += _pform_close_Click;
                }
            }
        }
        private string _id;
        public delegate void Open_Click();
        public event Open_Click open_Click;
        public delegate void Close_Click();
        public event Close_Click close_Click;
        public ThreadHandler(string id)
        {
            _id = id;
        }

        public void PositionsFormWork()
        {
            Application.EnableVisualStyles();
            Application.DoEvents();

            _pform = new PositionsForm();
            _pform.Text = _id;
            _pform.ShowDialog();
        }

        private void _pform_open_Click()
        {
            if (open_Click != null)
                open_Click();
        }

        private void _pform_close_Click()
        {
            if (close_Click != null)
                close_Click();
        }

        public void setAccountInfo(string b, string e,string m,string n)
        {
            _pform.SetAccountInfo(b, e, m, n);
        }
        public void setPos(List<string> str)
        {
            _pform.SetPos(str);
        }

        public List<string> positionParam()
        {
            return _pform.PositionParam();
        }
    }
}
