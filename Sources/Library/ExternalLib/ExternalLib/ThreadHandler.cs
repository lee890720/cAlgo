using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExternalLib
{
    public class ThreadHandler
    {
        private string sMestext;
        private int iDigs;
        private externalForm withEventsField_myForm;
        private externalForm myForm
        {
            get { return withEventsField_myForm; }
            set
            {
                if (withEventsField_myForm != null)
                {
                    withEventsField_myForm.ButtonBuyClicked -= myForm_ButtonBuyClicked;
                    withEventsField_myForm.ButtonSellClicked -= myForm_ButtonSellClicked;
                }
                withEventsField_myForm = value;
                if (withEventsField_myForm != null)
                {
                    withEventsField_myForm.ButtonBuyClicked += myForm_ButtonBuyClicked;
                    withEventsField_myForm.ButtonSellClicked += myForm_ButtonSellClicked;
                }
            }
        }
        public delegate void ButtonSellClickedEventHandler();
        public event ButtonSellClickedEventHandler ButtonSellClicked;
        public delegate void ButtonBuyClickedEventHandler();
        public event ButtonBuyClickedEventHandler ButtonBuyClicked;
        public ThreadHandler(string sMsgTxt, int iDigits)
        {
            sMestext = sMsgTxt;
            iDigs = iDigits;
        }

        public void Work()
        {
            Application.EnableVisualStyles();
            Application.DoEvents();

            myForm = new externalForm();
            myForm.Text = sMestext;
            myForm.ShowDialog();
        }

        public void setButtonText(string cTxt, double dPrice)
        {
            myForm.SetButtonText(cTxt, dPrice);
        }

        private void myForm_ButtonBuyClicked()
        {
            if (ButtonBuyClicked != null)
            {
                ButtonBuyClicked();
            }
        }

        private void myForm_ButtonSellClicked()
        {
            if (ButtonSellClicked != null)
            {
                ButtonSellClicked();
            }
        }
    }
}
