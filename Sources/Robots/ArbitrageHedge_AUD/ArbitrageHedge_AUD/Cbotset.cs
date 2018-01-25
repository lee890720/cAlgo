using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace cAlgo
{
    public partial class Cbotset : Form
    {
        public Cbotset()
        {
            InitializeComponent();
            foreach (Control ctrl in Controls)
            {
                if (ctrl is Button)
                    ctrl.Click += new EventHandler(btnclick);
            }
        }

        private void Cbotset_Load(object sender, EventArgs e)
        {
            String path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+@"\cAlgo\cbotset\";
            DirectoryInfo folder = new DirectoryInfo(path);     
            int i = folder.GetFiles("*.cbotset").Count();
            int b = 1;
            foreach (FileInfo file in folder.GetFiles("*.cbotset"))
            {
                int f_num = file.Name.IndexOf(",");
                if (file.Name.Substring(f_num + 2, 3) == "AUD")
                {
                    foreach (Control ctrl in this.Controls)
                    {
                        if (ctrl is Button)
                        {
                            if (ctrl.Name == "button" + b.ToString())
                            {
                                b++;
                                int num = file.Name.IndexOf(",");
                                ctrl.Text = file.Name.Substring(num + 2, 9);
                                continue;
                            }
                        }
                    }
                }
                else
                    continue;
            }
            int b_count = 0;
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button)
                {
                    b_count++;
                }
            }
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button)
                {
                    for (int c = b; c <= b_count; c++)
                    {
                        if (ctrl.Name == "button" + c.ToString())
                        {
                            ctrl.Visible = false;
                        }
                    }
                }
            }
        }

        private void btnclick(Object sender, EventArgs e)
        {
            Main._getbuttontext = (sender as Button).Text;
            this.Close();
        } 
    }
}

