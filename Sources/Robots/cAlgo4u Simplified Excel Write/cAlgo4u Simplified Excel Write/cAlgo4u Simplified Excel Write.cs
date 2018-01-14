using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.IO;

// UNCOMMENT CODE BELOW

//using NPOI.HSSF.UserModel;
//using NPOI.HPSF;
//using NPOI.POIFS.FileSystem;
//using NPOI.SS.UserModel;
//using NPOI.HSSF.Model;
//using NPOI.SS.Formula.Functions;

// This has been written by Paul Hayes | http://www.cAlgo4u.com | 26/12/2015

// The robot is an example using a 3rd party utility to simplify writing data to an excel file using NPOI
// https://npoi.codeplex.com/

// The robot gets the market series data for an instrument and stores the information in an excel file specified from your 'user defined parameters'.
// To operate you back-test between start and end dates for the data that you wish to save and at the and of the test an excel file is created or updated 
// with the following data shown below. You can also use it to store any data you wish.

// OPEN TIME
// OPEN PRICE
// HIGH PRICE
// LOW PRICE
// CLOSE PRICE
// TICK VOLUME

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FileSystem)]
    public class cAlgo4uSimplifiedExcelWrite : Robot
    {
        // User enters the actual path to the media file.
        [Parameter("Excel File Path", DefaultValue = "C:\\Users\\Paul\\Documents\\test.xls")]
        public string ExcelFilePath { get; set; }

        // UNCOMMENT CODE BELOW

        //static HSSFWorkbook hssfworkbook;
        //HSSFSheet workSheet;

        #region cTrader events

        protected override void OnStart()
        {

        }

        protected override void OnTick()
        {
        }

        /// <summary>
        /// When back-testing is complete, when the robot stops, all th data gathered is written to the excel file.
        /// </summary>
        protected override void OnStop()
        {
            // UNCOMMENT CODE BELOW

            //InitializeWorkbook();

            //// create xls if not exists
            //if (!File.Exists(ExcelFilePath))
            //{
            //    hssfworkbook = HSSFWorkbook.Create(InternalWorkbook.CreateWorkbook());

            //    // create work sheet
            //    workSheet = (HSSFSheet)hssfworkbook.CreateSheet("MARKET SERIES");

            //    CreateColumnHeaders();

            //    // iterate through the entire date range for the back-test and export the data to an excel file.
            //    for (int i = 1; i < MarketSeries.Close.Count; i++)
            //    {
            //        var r = workSheet.CreateRow(i);

            //        // create columns
            //        r.CreateCell(0).SetCellValue(MarketSeries.OpenTime[i]);
            //        r.CreateCell(1).SetCellValue(MarketSeries.Open[i]);
            //        r.CreateCell(2).SetCellValue(MarketSeries.High[i]);
            //        r.CreateCell(3).SetCellValue(MarketSeries.Low[i]);
            //        r.CreateCell(4).SetCellValue(MarketSeries.Close[i]);
            //        r.CreateCell(5).SetCellValue(MarketSeries.TickVolume[i]);
            //    }

            //    using (var fs = new FileStream(ExcelFilePath, FileMode.Create, FileAccess.Write))
            //    {
            //        hssfworkbook.Write(fs);
            //    }
            //}
        }

        #endregion

        #region excel logic

        /// <summary>
        /// Create the column headers on the first row
        /// </summary>
        private void CreateColumnHeaders()
        {
            // UNCOMMENT CODE BELOW

            //var r = workSheet.CreateRow(0);
            //r.CreateCell(0).SetCellValue("OPEN TIME");
            //r.CreateCell(1).SetCellValue("OPEN");
            //r.CreateCell(2).SetCellValue("HIGH");
            //r.CreateCell(3).SetCellValue("LOW");
            //r.CreateCell(4).SetCellValue("CLOSE");
            //r.CreateCell(5).SetCellValue("TICK VOLUME");
        }

        /// <summary>
        /// initialize the document's summary information, its more of a nice to have.
        /// </summary>
        static void InitializeWorkbook()
        {
            // UNCOMMENT CODE BELOW

            //hssfworkbook = new HSSFWorkbook();

            //////create a entry of DocumentSummaryInformation
            //DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            //dsi.Company = "cAlgo4u";
            //hssfworkbook.DocumentSummaryInformation = dsi;

            //////create a entry of SummaryInformation
            //SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            //si.Subject = "cAlgo4u writing to excel example";
            //hssfworkbook.SummaryInformation = si;
        }

        #endregion
    }
}
