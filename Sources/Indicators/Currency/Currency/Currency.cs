using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Currency : Indicator
    {
        #region Parameter
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter(DefaultValue = "EURUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "USDCHF")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Magnify { get; set; }

        public string _ratio;
        public string _magnify;
        private MarketSeries _symbolFirstSeries, _symbolSecondSeries;
        private DateTime SymbolTime;
        private Symbol _firstsymbol, _secondsymbol;
        private int FirstIndex, SecondIndex;
        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
        private Colors NoCorel;
        #endregion

        protected override void Initialize()
        {
            _symbolFirstSeries = MarketData.GetSeries(FirstSymbol, TimeFrame);
            _symbolSecondSeries = MarketData.GetSeries(SecondSymbol, TimeFrame);
            _firstsymbol = MarketData.GetSymbol(FirstSymbol);
            _secondsymbol = MarketData.GetSymbol(SecondSymbol);
            NoCorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            #region Parameter
            SymbolTime = MarketSeries.OpenTime[index];
            FirstIndex = _symbolFirstSeries.GetIndexByDate(SymbolTime);
            SecondIndex = _symbolSecondSeries.GetIndexByDate(SymbolTime);
            var _firsttime = _symbolFirstSeries.OpenTime[FirstIndex];
            var _secondtime = _symbolSecondSeries.OpenTime[SecondIndex];
            List<DateTime> _time = new List<DateTime>();
            _time.Add(SymbolTime);
            _time.Add(_firsttime);
            _time.Add(_secondtime);
            FirstIndex = _symbolFirstSeries.GetIndexByDate(_time.Min());
            SecondIndex = _symbolSecondSeries.GetIndexByDate(_time.Min());
            #endregion

            #region FirstClose
            double FirstClose = 0;
            double _firstclose = _symbolFirstSeries.Close[FirstIndex];
            if (double.IsNaN(_firstclose))
            {
                for (int i = FirstIndex - 1; i >= 0; i--)
                {
                    if (!double.IsNaN(_symbolFirstSeries.Close[i]))
                    {
                        _firstclose = _symbolFirstSeries.Close[i];
                        break;
                    }
                }
            }
            FirstClose = _getvalue(_firstsymbol, _firstclose);
            #endregion

            #region SecondClose
            double SecondClose = 0;
            double _secondclose = _symbolSecondSeries.Close[SecondIndex];
            if (double.IsNaN(_secondclose))
            {
                for (int i = SecondIndex - 1; i >= 0; i--)
                {
                    if (!double.IsNaN(_symbolSecondSeries.Close[i]))
                    {
                        _secondclose = _symbolSecondSeries.Close[i];
                        break;
                    }
                }
            }
            SecondClose = _getvalue(_secondsymbol, _secondclose);
            #endregion

            //Result
            if (Ratio >= 1)
                Result[index] = (SecondClose * Ratio - FirstClose) * Magnify / 0.0001 + 10000;
            else
                Result[index] = (SecondClose - FirstClose / Ratio) * Magnify / 0.0001 + 10000;
            //Average
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
            //Chart
            Chart(index);
        }

        private void Chart(int index)
        {
            string str_ratio = null;
            double average_ratio = 0;
            List<double> list_ratio = new List<double>();
            for (int i = 0; i < 10; i++)
            {
                if (double.IsNaN(_getratio(i)))
                    list_ratio.Add(1);
                else
                    list_ratio.Add(_getratio(i));
                str_ratio += "_" + list_ratio[i].ToString();
            }
            if (FirstSymbol.Contains("XAU") || FirstSymbol.Contains("XAG"))
                average_ratio = Math.Round(list_ratio.Average(), 4);
            else
                average_ratio = Math.Round(list_ratio.Average(), 3);
            _ratio = str_ratio = "(" + average_ratio.ToString() + ")" + str_ratio;
            _magnify = _getmagnify(index);
            ChartObjects.DrawText("Ratio", "\n" + _ratio, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Magnify", "\n\n" + _magnify, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Param", "\n\n\nRatio_" + Ratio.ToString() + "_Magnify_" + Magnify.ToString(), StaticPosition.TopLeft, NoCorel);
        }

        private string _getmagnify(int index)
        {
            double min = 0;
            double max = 0;
            double current = Result[index - 1];
            double mmin = 0;
            double mmax = 0;
            double mcurrent = MarketSeries.Close[index - 1] / Symbol.PipSize;
            List<double> list_mag = new List<double>();
            string mag = null;
            for (int n = 0; n < 10; n++)
            {
                for (int i = index - Period * (n + 1); i < index - Period * n; i++)
                {
                    if (min == 0)
                        min = Result[i];
                    if (max == 0)
                        max = Result[i];
                    if (min > Result[i])
                        min = Result[i];
                    if (max < Result[i])
                        max = Result[i];
                    if (mmin == 0)
                        mmin = MarketSeries.Close[i] / Symbol.PipSize;
                    if (mmax == 0)
                        mmax = MarketSeries.Close[i] / Symbol.PipSize;
                    if (mmin > MarketSeries.Close[i] / Symbol.PipSize)
                        mmin = MarketSeries.Close[i] / Symbol.PipSize;
                    if (mmax < MarketSeries.Close[i] / Symbol.PipSize)
                        mmax = MarketSeries.Close[i] / Symbol.PipSize;
                }
                var mm = Math.Round((mmax - mmin) / (max - min), 2);
                list_mag.Add(mm);
                mag += "_" + list_mag[n].ToString();
            }
            mag = "(" + Math.Round(list_mag.Average(), 2).ToString() + ")" + mag;
            return mag;
        }

        private double _getratio(int _r)
        {
            double firsttotal = 0;
            double secondtotal = 0;
            double r_ratio = 0;
            for (int i = FirstIndex - Period * (_r + 1); i < FirstIndex - Period * _r; i++)
            {
                double FH = 0;
                double FL = 0;
                FH = _getvalue(_firstsymbol, _symbolFirstSeries.High[i]);
                FL = _getvalue(_firstsymbol, _symbolFirstSeries.Low[i]);
                firsttotal += Math.Abs(FH - FL);
            }
            for (int i = SecondIndex - Period * (_r + 1); i < SecondIndex - Period * _r; i++)
            {
                double SH = 0;
                double SL = 0;
                SH = _getvalue(_secondsymbol, _symbolSecondSeries.High[i]);
                SL = _getvalue(_secondsymbol, _symbolSecondSeries.Low[i]);
                secondtotal += Math.Abs(SH - SL);
            }
            if (FirstSymbol.Contains("XAU") || FirstSymbol.Contains("XAG"))
                r_ratio = Math.Round(firsttotal / secondtotal, 4);
            else
                r_ratio = Math.Round(firsttotal / secondtotal, 3);
            return r_ratio;
        }

        private double _getvalue(Symbol symbol, double v)
        {
            double _value = 0;
            if (symbol.Code.Substring(0, 3) == "USD")
                _value = v / (symbol.PipSize / 0.0001);
            if (symbol.Code.Substring(3, 3) == "USD")
                _value = 1 / v * (symbol.PipSize / 0.0001);
            return _value;
        }
    }
}
