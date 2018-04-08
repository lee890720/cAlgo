using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CSVLib
{
    public class CsvParsingHelper
    {
        /// <summary>
        /// 将csv文件的数据转成datatable
        /// </summary>
        /// <param name="csvfilePath">csv文件路径</param>
        /// <param name="firstIsRowHead">是否将第一行作为字段名</param>
        /// <returns></returns>
        public static DataTable CsvToDataTable(string csvfilePath, bool firstIsRowHead)
        {
            FileStream fstream= File.Open(csvfilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader sreader = new StreamReader(fstream, Encoding.Default);
            DataTable dtResult = null;
            if (File.Exists(csvfilePath))
            {
                string csvstr = sreader.ReadToEnd();
                if (!string.IsNullOrEmpty(csvstr))
                {
                    dtResult = ToDataTable(csvstr, firstIsRowHead);
                }
            }
            sreader.Close();
            fstream.Close();
            return dtResult;
        }

        /// <summary>
        /// 将CSV数据转换为DataTable
        /// </summary>
        /// <param name="csv">包含以","分隔的CSV数据的字符串</param>
        /// <param name="isRowHead">是否将第一行作为字段名</param>
        /// <returns></returns>
        private static DataTable ToDataTable(string csv, bool isRowHead)
        {
            DataTable dt = null;
            if (!string.IsNullOrEmpty(csv))
            {
                dt = new DataTable();
                string[] csvRows = csv.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                string[] csvColumns = null;
                if (csvRows != null)
                {
                    if (csvRows.Length > 0)
                    {
                        //第一行作为字段名,添加第一行记录并删除csvRows中的第一行数据
                        if (isRowHead)
                        {
                            csvColumns = FromCsvLine(csvRows[0]);
                            csvRows[0] = null;
                            for (int i = 0; i < csvColumns.Length; i++)
                            {
                                dt.Columns.Add(csvColumns[i]);
                            }
                        }

                        for (int i = 0; i < csvRows.Length; i++)
                        {
                            if (csvRows[i] != null)
                            {
                                csvColumns = FromCsvLine(csvRows[i]);
                                //检查列数是否足够,不足则补充
                                if (dt.Columns.Count < csvColumns.Length)
                                {
                                    int columnCount = csvColumns.Length - dt.Columns.Count;
                                    for (int c = 0; c < columnCount; c++)
                                    {
                                        dt.Columns.Add();
                                    }
                                }
                                dt.Rows.Add(csvColumns);
                            }
                        }
                    }
                }
            }

            return dt;
        }
        /// <summary>
        /// 解析一行CSV数据
        /// </summary>
        /// <param name="csv">csv数据行</param>
        /// <returns></returns>
        public static string[] FromCsvLine(string csv)
        {
            List<string> csvLiAsc = new List<string>();
            List<string> csvLiDesc = new List<string>();

            if (!string.IsNullOrEmpty(csv))
            {
                //顺序查找
                int lastIndex = 0;
                int quotCount = 0;
                //剩余的字符串
                string lstr = string.Empty;
                for (int i = 0; i < csv.Length; i++)
                {
                    if (csv[i] == '"')
                    {
                        quotCount++;
                    }
                    else if (csv[i] == ',' && quotCount % 2 == 0)
                    {
                        csvLiAsc.Add(ReplaceQuote(csv.Substring(lastIndex, i - lastIndex)));
                        lastIndex = i + 1;
                    }
                    if (i == csv.Length - 1 && lastIndex < csv.Length)
                    {
                        lstr = csv.Substring(lastIndex, i - lastIndex + 1);
                    }
                }
                if (!string.IsNullOrEmpty(lstr))
                {
                    //倒序查找
                    lastIndex = 0;
                    quotCount = 0;
                    string revStr = Reverse(lstr);
                    for (int i = 0; i < revStr.Length; i++)
                    {
                        if (revStr[i] == '"')
                        {
                            quotCount++;
                        }
                        else if (revStr[i] == ',' && quotCount % 2 == 0)
                        {
                            csvLiDesc.Add(ReplaceQuote(Reverse(revStr.Substring(lastIndex, i - lastIndex))));
                            lastIndex = i + 1;
                        }
                        if (i == revStr.Length - 1 && lastIndex < revStr.Length)
                        {
                            csvLiDesc.Add(ReplaceQuote(Reverse(revStr.Substring(lastIndex, i - lastIndex + 1))));
                            lastIndex = i + 1;
                        }

                    }
                    string[] tmpStrs = csvLiDesc.ToArray();
                    Array.Reverse(tmpStrs);
                    csvLiAsc.AddRange(tmpStrs);
                }
            }

            return csvLiAsc.ToArray();
        }
        /// <summary>
        /// 反转字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Reverse(string str)
        {
            string revStr = string.Empty;
            foreach (char chr in str)
            {
                revStr = chr.ToString() + revStr;
            }
            return revStr;
        }
        /// <summary>
        /// 替换CSV中的双引号转义符为正常双引号,并去掉左右双引号
        /// </summary>
        /// <param name="csvValue">csv格式的数据</param>
        /// <returns></returns>
        private static string ReplaceQuote(string csvValue)
        {
            string rtnStr = csvValue;
            if (!string.IsNullOrEmpty(csvValue))
            {
                //首尾都是"
                Match m = Regex.Match(csvValue, "^\"(.*?)\"$");
                if (m.Success)
                {
                    rtnStr = m.Result("${1}").Replace("\"\"", "\"");
                }
                else
                {
                    rtnStr = rtnStr.Replace("\"\"", "\"");
                }
            }
            return rtnStr;

        }

        /// <summary>
        /// 将DataTable转换成CSV文件
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="filePath">文件路径</param>
        public static void SaveCsv(DataTable dt, string filePath)
        {
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                if(!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                fs = new FileStream(filePath + dt.TableName + ".csv", FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs, Encoding.Default);
                var data = string.Empty;
                //写出列名称
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    data += dt.Columns[i].ColumnName;
                    if (i < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
                //写出各行数据
                for (var i = 0; i < dt.Rows.Count; i++)
                {
                    data = string.Empty;
                    for (var j = 0; j < dt.Columns.Count; j++)
                    {
                        data += dt.Rows[i][j].ToString();
                        if (j < dt.Columns.Count - 1)
                        {
                            data += ",";
                        }
                    }
                    sw.WriteLine(data);
                }
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message, ex);
            }
            finally
            {
                if (sw != null) sw.Close();
                if (fs != null) fs.Close();
            }
        }
    }
}
