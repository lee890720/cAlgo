using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace cAlgo
{
    public partial class Main : Form
    {
        public static string _getbuttontext;
        public static double _Init_Volume;
        public static string _FirstSymbol;
        public static string _SecondSymbol;
        public static int _Period;
        public static int _Distance;
        public static int _timer;
        public static double _Ratio;
        public static double _Magnify;
        public static bool _IsTrade;
        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            Cbotset cbotset = new Cbotset();
            //cbotset.TopLevel = true;
            cbotset.ShowDialog();
        }

        private void Main_Activated(object sender, EventArgs e)
        {
            if (_getbuttontext != null)
            {
                String path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\cAlgo\cbotset\";
                DirectoryInfo folder = new DirectoryInfo(path);
                foreach (FileInfo file in folder.GetFiles("*.cbotset"))
                {
                    if (file.Name.Contains(_getbuttontext))
                    {
                        StreamReader sR = File.OpenText(path + file.Name);
                        string nextLine;
                        while ((nextLine = sR.ReadLine()) != null)
                        {
                            foreach (Control ctrl in Controls)
                            {
                                if (ctrl is TextBox||ctrl is ComboBox)
                                    if (nextLine.Contains(ctrl.Name))
                                    {
                                        int num = nextLine.IndexOf("=");
                                        ctrl.Text = nextLine.Substring(num + 2);
                                    }
                            }
                        }
                        sR.Close();
                    }
                }
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            _Init_Volume = Convert.ToDouble(this.Init_Volume.Text);
            _FirstSymbol = this.FirstSymbol.Text;
            _SecondSymbol = this.SecondSymbol.Text;
            _Period = Convert.ToInt32(this.Period.Text);
            _Distance = Convert.ToInt32(this.Distance.Text);
            _timer = Convert.ToInt32(this.timer.Text);
            _Ratio = Convert.ToDouble(this.Ratio.Text);
            _Magnify = Convert.ToDouble(this.Magnify.Text);
            _IsTrade = Convert.ToBoolean(this.IsTrade.Text);
            this.Close();
        }
    }
}
