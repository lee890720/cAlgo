using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ExternalLib
{
    public partial class externalForm : Form
    {
        public delegate void ButtonSellClickedEventHandler();
        public event ButtonSellClickedEventHandler ButtonSellClicked;
        public delegate void ButtonBuyClickedEventHandler();
        public event ButtonBuyClickedEventHandler ButtonBuyClicked;
        public delegate void dlSetButtonText(string cTxt, double dPrice);

        public externalForm()
        {
            InitializeComponent();
        }


        public void SetButtonText(string cTxt,double dPrice)
        {
            foreach (Control ctrl in this.Controls)
            {
                if(ctrl is Button)
                {
                    if (ctrl.Name == cTxt)
                    {
                        if (ctrl.InvokeRequired)
                            ctrl.Invoke(new dlSetButtonText(SetButtonText), cTxt, dPrice);
                        else
                            ctrl.Text = dPrice.ToString();
                    }

                }
            }
        }

        private void buttonSell_Click(object sender, EventArgs e)
        {
            if (ButtonSellClicked != null)
                ButtonSellClicked();
        }

        private void buttonBuy_Click(object sender, EventArgs e)
        {
            if (ButtonBuyClicked != null)
                ButtonBuyClicked();
        }
    }
}
