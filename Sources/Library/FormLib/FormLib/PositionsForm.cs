using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using cAlgo.API;
using cAlgo.Lib;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.API.Indicators;
using System.Data.SqlClient;
using System.Reflection;
using System.Media;

namespace FormLib
{
    public partial class PositionsForm : Form
    {
        public delegate void dlSetAccountInfo(string b, string e, string m, string n);
        public delegate void dlSetLabelText(string cTxt);
        public delegate void dlSetPos(List<string> str);
        public delegate void Open_Click();
        public event Open_Click open_Click;
        public delegate void Close_Click();
        public event Close_Click close_Click;
        public PositionsForm()
        {
            InitializeComponent();
            this.textBox_symbol.AutoSize = false;
            this.textBox_symbol.Height = 30;
            this.textBox_label.AutoSize = false;
            this.textBox_label.Height = 30;
            this.textBox_volume.AutoSize = false;
            this.textBox_volume.Height = 30;
            this.textBox_comment.AutoSize = false;
            //this.textBox_comment.Height = 30;
            foreach (Control ctrl in this.panel_pos.Controls)
            {
                if (ctrl is Button)
                    ctrl.Click += new EventHandler(btnclick);
                if (ctrl is Label)
                    ctrl.Click += new EventHandler(lblclick);
                ctrl.Visible = false;
            }
            System.Timers.Timer timer = new System.Timers.Timer(5500);
            timer.Enabled = true;
            timer.Elapsed += T_Elapsed;
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            con.Open();
            DataSet dataset = new DataSet();
            string strsql = "select * from Person where PersonID=1";
            SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
            SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
            objdataadpater.Fill(dataset, "cBot");
            DateTime time = Convert.ToDateTime(dataset.Tables["cBot"].Rows[0][3]);
            con.Close();
            var utctime = DateTime.UtcNow;
            string namespaceName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            Assembly assembly = Assembly.GetExecutingAssembly();
            SoundPlayer sp = new SoundPlayer(assembly.GetManifestResourceStream(namespaceName + ".Resources" + ".Alarm.wav"));
            sp.Stop();
            if (DateTime.Compare(utctime.AddSeconds(-120), time) > 0)
            {
                this.label_vps.ForeColor = Color.Red;
                this.label_vps.Text = "ERROR!!!";
                sp.Play();
            }
            else
            {
                this.label_vps.ForeColor = Color.White;
                this.label_vps.Text = "VPS IS OK";
            }
        }

        public void SetAccountInfo(string b, string e, string m, string n)
        {
            foreach (Control ctrl in this.panel_comm.Controls)
            {
                if (ctrl is Label)
                    if (ctrl.Name.Contains("balance"))
                        if (ctrl.InvokeRequired)
                            ctrl.Invoke(new dlSetAccountInfo(SetAccountInfo), b, e, m, n);
                        else
                            ctrl.Text = "Balance: " + b;
                if (ctrl is Label)
                    if (ctrl.Name.Contains("equlity"))
                        if (ctrl.InvokeRequired)
                            ctrl.Invoke(new dlSetAccountInfo(SetAccountInfo), b, e, m, n);
                        else
                            ctrl.Text = "Equlity: " + e;
                if (ctrl is Label)
                    if (ctrl.Name.Contains("margin"))
                        if (ctrl.InvokeRequired)
                            ctrl.Invoke(new dlSetAccountInfo(SetAccountInfo), b, e, m, n);
                        else
                            ctrl.Text = "Margin: " + m;
                if (ctrl is Label)
                    if (ctrl.Name.Contains("net"))
                        if (ctrl.InvokeRequired)
                            ctrl.Invoke(new dlSetAccountInfo(SetAccountInfo), b, e, m, n);
                        else
                            ctrl.Text = "Unr.Net: " + n;
            }
        }
        private void SetLabelText(string cTxt)
        {
            foreach (Control ctrl in this.panel_pos.Controls)
            {
                if (ctrl is Label)
                {
                    if (ctrl.Name == "label_" + "l" + cTxt)
                    {
                        if (ctrl.InvokeRequired)
                            ctrl.Invoke(new dlSetLabelText(SetLabelText), cTxt);
                        else
                            this.textBox_label.Text = (ctrl as Label).Text;
                    }
                    if (ctrl.Name == "label_" + "v" + cTxt)
                    {
                        if (ctrl.InvokeRequired)
                            ctrl.Invoke(new dlSetLabelText(SetLabelText), cTxt);
                        else
                            this.textBox_volume.Text = (ctrl as Label).Text;
                    }
                    if (ctrl.Name == "label_" + "c" + cTxt)
                    {
                        if (ctrl.InvokeRequired)
                            ctrl.Invoke(new dlSetLabelText(SetLabelText), cTxt);
                        else
                        {
                            var mulstr = (ctrl as Label).Text;
                            string newstr = null;
                            var num=mulstr.Length;
                            if (num >= 15)
                            {
                                newstr= mulstr.Substring(0, 15);
                                if (num >= 29)
                                    newstr+= "\r\n"+mulstr.Substring(15, 14);
                                if (num >= 48)
                                    newstr += "\r\n" + mulstr.Substring(29, 19);
                                if (num >= 66)
                                    newstr += "\r\n" + mulstr.Substring(48, 18);
                                this.textBox_comment.Text =newstr;
                            }
                            else
                            this.textBox_comment.Text = (ctrl as Label).Text;
                        }
                    }
                }
            }
        }

        public void SetPos(List<string> str)
        {
            foreach (Control ctrl in this.panel_pos.Controls)
            {
                if (Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(ctrl.Name, @"[^0-9]+", "")) > str.Count())
                    ctrl.Visible = false;
            }
            int idx = 0;
            foreach (string s in str)
            {
                idx++;
                foreach (Control ctrl in this.panel_pos.Controls)
                {
                    if (ctrl is Button)
                    {
                        if (ctrl.Name == "button" + idx.ToString())
                            if (ctrl.InvokeRequired)
                                ctrl.Invoke(new dlSetPos(SetPos), str);
                            else
                            {
                                ctrl.Visible = true;
                                ctrl.Text = s.Substring(0, 6);
                            }
                    }
                    if (ctrl is Label)
                    {
                        if (ctrl.Name == "label_l" + idx.ToString())
                            if (ctrl.InvokeRequired)
                                ctrl.Invoke(new dlSetPos(SetPos), str);
                            else
                            {
                                ctrl.Visible = true;
                                ctrl.Text = s.Substring(s.IndexOf("(") + 1, s.IndexOf(")") - s.IndexOf("(") - 1);
                            }
                        if (ctrl.Name == "label_v" + idx.ToString())
                            if (ctrl.InvokeRequired)
                                ctrl.Invoke(new dlSetPos(SetPos), str);
                            else
                            {
                                ctrl.Visible = true;
                                ctrl.Text = s.Substring(s.IndexOf("[") + 1, s.IndexOf("]") - s.IndexOf("[") - 1);
                            }
                        if (ctrl.Name == "label_c" + idx.ToString())
                            if (ctrl.InvokeRequired)
                                ctrl.Invoke(new dlSetPos(SetPos), str);
                            else
                            {
                                ctrl.Visible = true;
                                ctrl.Text = s.Substring(s.IndexOf("<") + 1, s.IndexOf(">") - s.IndexOf("<") - 1);
                            }
                    }
                }
            }
            if (this.textBox_symbol.Text == "Symbol")
            {
                this.button1.Select();
                this.button1.PerformClick();
            }
        }

        public List<string> PositionParam()
        {
            List<string> str = new List<string>();
            str.Add(this.textBox_symbol.Text);
            str.Add(this.textBox_label.Text);
            str.Add(this.textBox_volume.Text);
            str.Add(this.textBox_comment.Text);
            return str;
        }

        private void button_open_Click(object sender, EventArgs e)
        {
            if (open_Click != null)
                open_Click();
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            if (close_Click != null)
                close_Click();
        }

        private void lblclick(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            string sub = lbl.Name.Substring(7);
            foreach (Control ctrl in this.panel_pos.Controls)
            {
                if (ctrl is Button)
                {
                    if (ctrl.Name == "button" + sub)
                    {
                        (ctrl as Button).Select();
                        (ctrl as Button).PerformClick();
                    }
                }
            }

        }

        private void btnclick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string sub = btn.Name.Substring(6);
            this.textBox_symbol.Text = btn.Text;
            SetLabelText(sub);
        }
    }
}

