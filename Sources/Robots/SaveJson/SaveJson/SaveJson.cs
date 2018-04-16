using cAlgo.API;
using System;
using System.Data;
using System.Data.SqlClient;
using JsonLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class SaveJson : Robot
    {
        [Parameter("Data Source", DefaultValue = "tcp:leeinfo.database.windows.net,1433")]
        public string DataSource { get; set; }

        [Parameter("Initial Catalog", DefaultValue = "LeeInfoDb")]
        public string InitialCatalog { get; set; }

        [Parameter("User ID", DefaultValue = "lee890720")]
        public string UserID { get; set; }

        [Parameter("Password", DefaultValue = "Lee37355175")]
        public string Password { get; set; }

        private string _filePath;
        private string _fileName;
        private System.Timers.Timer _timer1;

        protected override void OnStart()
        {
            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            Print("fiName=" + _fileName);
            InitTimer1();
            _timer1.Start();
        }

        private void InitTimer1()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 60000;
            _timer1 = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            _timer1.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            _timer1.Enabled = true;
            //绑定Elapsed事件
            _timer1.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer1);
        }

        private void OnTimer1(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string strcon = "Data Source=";
                strcon += DataSource + ";Initial Catalog=";
                strcon += InitialCatalog + ";User ID=";
                strcon += UserID + ";Password=";
                strcon += Password + ";Integrated Security=False;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True";
                SqlConnection sqlCon = new SqlConnection();
                sqlCon.ConnectionString = strcon;
                sqlCon.Open();

                DataSet dataset_time = new DataSet();
                string strsql_time = "select * from Frx_Server";
                SqlDataAdapter sqlData_time = new SqlDataAdapter(strsql_time, sqlCon);
                SqlCommandBuilder sqlCom_time = new SqlCommandBuilder(sqlData_time);
                sqlData_time.Fill(dataset_time, "time");
                var utctime = DateTime.UtcNow;
                DataTable dt_time = dataset_time.Tables["time"];
                string computerName = System.Environment.MachineName;
                bool isExist = false;
                foreach (DataRow dr_time in dt_time.Rows)
                {
                    if (dr_time["ServerName"].ToString() == computerName)
                    {
                        isExist = true;
                        dr_time["ServerTime"] = utctime;
                        dr_time["AccountNumber"] = this.Account.Number;
                    }
                }
                if (!isExist)
                {
                    DataRow dr_time = dt_time.NewRow();
                    dr_time["ServerName"] = computerName;
                    dr_time["ServerTime"] = utctime;
                    dr_time["AccountNumber"] = this.Account.Number;
                    dt_time.Rows.Add(dr_time);
                }
                var result_time = sqlData_time.Update(dataset_time, "time");
                if (result_time > 0)
                    Print("It's Successful to update " + utctime.ToString());
                dataset_time.Dispose();
                sqlCom_time.Dispose();
                sqlData_time.Dispose();

                DataSet dataset = new DataSet();
                string strsql = "select * from Frx_Cbotset";
                SqlDataAdapter sqlData = new SqlDataAdapter(strsql, sqlCon);
                SqlCommandBuilder sqlCom = new SqlCommandBuilder(sqlData);
                //objdataadpater.SelectCommand.CommandTimeout = 1000;
                sqlData.Fill(dataset, "cbotset");
                DataTable dt = dataset.Tables["cbotset"];
                if (!Directory.Exists(_filePath))
                    Directory.CreateDirectory(_filePath);
                if (!File.Exists(_fileName))
                    Json.SaveJson(dt, _fileName);
                string cbotset_server = Json.ToJson(dt);
                var list_cbotset_server = JsonConvert.DeserializeObject<List<FrxCbotset>>(cbotset_server);
                string cbotset_local = ReadFileData();
                var list_cbotset_local = JsonConvert.DeserializeObject<List<FrxCbotset>>(cbotset_local);
                if (list_cbotset_local.Count != list_cbotset_server.Count)
                {
                    Json.SaveJson(dt, _fileName);
                    cbotset_local = ReadFileData();
                    list_cbotset_local = JsonConvert.DeserializeObject<List<FrxCbotset>>(cbotset_local);
                }

                var serverChanged = false;
                var localChanged = false;
                foreach (var s in list_cbotset_server)
                {
                    foreach (var l in list_cbotset_local)
                    {
                        if (l.Symbol == s.Symbol)
                        {
                            if (l.InitVolume != s.InitVolume)
                            {
                                l.InitVolume = s.InitVolume;
                                Print(l.Symbol + "-InitVolume is changed.");
                                localChanged = true;
                            }
                            if (l.Tmr != s.Tmr)
                            {
                                l.Tmr = s.Tmr;
                                Print(l.Symbol + "-Tmr is changed.");
                                localChanged = true;
                            }
                            if (l.Brk != s.Brk)
                            {
                                l.Brk = s.Brk;
                                Print(l.Symbol + "-Brk is changed.");
                                localChanged = true;
                            }
                            if (l.Distance != s.Distance)
                            {
                                l.Distance = s.Distance;
                                Print(l.Symbol + "-Distance is changed.");
                                localChanged = true;
                            }
                            if (l.IsTrade != s.IsTrade)
                            {
                                l.IsTrade = s.IsTrade;
                                Print(l.Symbol + "-IsTrade is changed.");
                                localChanged = true;
                            }
                            if (l.IsBreak != s.IsBreak)
                            {
                                l.IsBreak = s.IsBreak;
                                Print(l.Symbol + "-IsBreak is changed.");
                                localChanged = true;
                            }
                            if (l.IsBrkFirst != s.IsBrkFirst)
                            {
                                l.IsBrkFirst = s.IsBrkFirst;
                                Print(l.Symbol + "-IsBrkFirst is changed.");
                                localChanged = true;
                            }
                            if (l.Result != s.Result)
                            {
                                l.Result = s.Result;
                                Print(l.Symbol + "-Result is changed.");
                                localChanged = true;
                            }
                            if (l.Average != s.Average)
                            {
                                l.Average = s.Average;
                                Print(l.Symbol + "-Average is changed.");
                                localChanged = true;
                            }
                            if (l.Magnify != s.Magnify)
                            {
                                l.Magnify = s.Magnify;
                                Print(l.Symbol + "-Magnify is changed.");
                                localChanged = true;
                            }
                            if (l.Sub != s.Sub)
                            {
                                l.Sub = s.Sub;
                                Print(l.Symbol + "-Sub is changed.");
                                localChanged = true;
                            }
                            if (l.Alike != s.Alike)
                            {
                                l.Alike = s.Alike;
                                Print(l.Symbol + "-Alike is changed.");
                                localChanged = true;
                            }
                            if (l.Cr != s.Cr || l.Ca != s.Ca || l.Sr != s.Sr || l.Sa != s.Sa || l.Signal != s.Signal)
                            {
                                dt.PrimaryKey = new DataColumn[] 
                                {
                                    dt.Columns["Id"]
                                };
                                DataRow dr = dt.Rows.Find(s.Id);
                                if (dr != null)
                                {
                                    dr["Cr"] = l.Cr;
                                    dr["Ca"] = l.Ca;
                                    dr["Sr"] = l.Sr;
                                    dr["Sa"] = l.Sa;
                                    dr["Signal"] = l.Signal;
                                    serverChanged = true;
                                }
                            }
                        }
                    }
                }
                if (localChanged)
                {
                    Json.SaveJson<FrxCbotset>(list_cbotset_local, _fileName);
                    Print("It's Successful to save Local-Json.");
                }
                else
                    Print("Local No Change.");
                if (serverChanged)
                {
                    var result = sqlData.Update(dataset, "cbotset");
                    Print(result.ToString() + " has been changed.");
                }
                else
                {
                    Print("Server No Change.");
                }
                dataset.Dispose();
                sqlCom.Dispose();
                sqlData.Dispose();
                sqlCon.Close();
                sqlCon.Dispose();
            } catch (System.Data.SqlClient.SqlException ex)
            {
                Print(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        private string ReadFileData()
        {
            if (!File.Exists(_fileName))
                return null;
            FileStream stream = null;
            StreamReader streamReader = null;
            //StreamWriter streamWriter = null;
            stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            streamReader = new StreamReader(stream);
            //streamWriter = new StreamWriter(stream,Encoding.Default);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            stream.Close();
            return data;
        }

        private void WriteFileData(string data)
        {
            if (!File.Exists(_fileName))
                return;
            FileStream stream = null;
            StreamWriter streamWriter = null;
            stream = new FileStream(_fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            streamWriter = new StreamWriter(stream, Encoding.Default);
            streamWriter.Write(data);
            streamWriter.Close();
            stream.Close();
        }

        protected override void OnStop()
        {
            _timer1.Stop();
            Print("OnStop()");
            // Put your deinitialization logic here
        }
    }
    public class FrxCbotset
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public int InitVolume { get; set; }
        public int Tmr { get; set; }
        public double Brk { get; set; }
        public double Distance { get; set; }
        public bool IsTrade { get; set; }
        public bool IsBreak { get; set; }
        public bool IsBrkFirst { get; set; }
        public int Result { get; set; }
        public int Average { get; set; }
        public double Magnify { get; set; }
        public double Sub { get; set; }
        public double? Cr { get; set; }
        public double? Ca { get; set; }
        public double? Sr { get; set; }
        public double? Sa { get; set; }
        public string Signal { get; set; }
        public string Alike { get; set; }
    }
}
