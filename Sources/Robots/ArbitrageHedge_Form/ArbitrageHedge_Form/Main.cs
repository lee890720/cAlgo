using System;
using System.IO;
using System.Windows.Forms;

namespace cAlgo
{
    public partial class Main : Form
    {
        public static string _getbuttontext;
        public static double _Init_Volume;
        public static string _FirstSymbol;
        public static string _SecondSymbol;
        public static int _timer;
        public static double _break;
        public static int _Period;
        public static int _Distance;
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
                                if (ctrl is TextBox)
                                    if (nextLine.Contains(ctrl.Name.Substring(8) ))
                                    {
                                        int num = nextLine.IndexOf("=");
                                        ctrl.Text = nextLine.Substring(num + 2);
                                    }
                                if(ctrl is ComboBox)
                                                                        if (nextLine.Contains(ctrl.Name.Substring(9)))
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
            _Init_Volume = Convert.ToDouble(this.textBox_Init_Volume.Text);
            _FirstSymbol = this.textBox_FirstSymbol.Text;
            _SecondSymbol = this.textBox_SecondSymbol.Text;
            _timer = Convert.ToInt32(this.textBox_timer.Text);
            _break = Convert.ToDouble(this.textBox_break.Text);
            _Period = Convert.ToInt32(this.textBox_Period.Text);
            _Distance = Convert.ToInt32(this.textBox_Distance.Text);
            _Ratio = Convert.ToDouble(this.textBox_Ratio.Text);
            _Magnify = Convert.ToDouble(this.textBox_Magnify.Text);
            _IsTrade = Convert.ToBoolean(this.comboBox_IsTrade.Text);
            this.Close();
        }
    }
}
