using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Currency : Indicator
    {
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
        private MarketSeries _symbolFirstSeries, _symbolSecondSeries;
        private DateTime SymbolTime;
        private Symbol _firstsymbol, _secondsymbol;
        private int FirstIndex, SecondIndex;
        //private Colors PCorel;
        //private Colors  NCorel;
        private Colors NoCorel;
        protected override void Initialize()
        {
            _symbolFirstSeries = MarketData.GetSeries(FirstSymbol, TimeFrame);
            _symbolSecondSeries = MarketData.GetSeries(SecondSymbol, TimeFrame);
            _firstsymbol = MarketData.GetSymbol(FirstSymbol);
            _secondsymbol = MarketData.GetSymbol(SecondSymbol);
            //PCorel = Colors.Lime;
            //NCorel = Colors.OrangeRed;
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
            //Ratio
            GetRatio();
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
            if (FirstSymbol.Substring(0, 3) == "USD")
                FirstClose = 1 / _firstclose * (_firstsymbol.PipSize / 0.0001);
            if (FirstSymbol.Substring(3, 3) == "USD")
                FirstClose = _firstclose / (_firstsymbol.PipSize / 0.0001);
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
            if (SecondSymbol.Substring(0, 3) == "USD")
                SecondClose = 1 / _secondclose * (_secondsymbol.PipSize / 0.0001);
            if (SecondSymbol.Substring(3, 3) == "USD")
                SecondClose = _secondclose / (_secondsymbol.PipSize / 0.0001);
            #endregion
            //Result
            Result[index] = (FirstClose / Ratio - SecondClose) * Magnify / 0.0001 + 10000;
            //Average
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }

        private void GetRatio()
        {
            #region Parameter
            double _ratio0 = 1;
            double _ratio1 = 1;
            double _ratio2 = 1;
            double _ratio3 = 1;
            List<double> list_ratio = new List<double>();
            double firsttotal0 = 0;
            double secondtotal0 = 0;
            double firsttotal1 = 0;
            double secondtotal1 = 0;
            double firsttotal2 = 0;
            double secondtotal2 = 0;
            double firsttotal3 = 0;
            double secondtotal3 = 0;
            #endregion
            #region _ratio0
            for (int i = FirstIndex - Period; i < FirstIndex; i++)
            {
                double FC = 0;
                if (FirstSymbol.Substring(0, 3) == "USD")
                    FC = 1 / _symbolFirstSeries.Close[i] * (_firstsymbol.PipSize / 0.0001);
                if (FirstSymbol.Substring(3, 3) == "USD")
                    FC = _symbolFirstSeries.Close[i] / (_firstsymbol.PipSize / 0.0001);
                firsttotal0 += FC;
            }
            for (int i = SecondIndex - Period; i < SecondIndex; i++)
            {
                double SC = 0;
                if (SecondSymbol.Substring(0, 3) == "USD")
                    SC = 1 / _symbolSecondSeries.Close[i] * (_secondsymbol.PipSize / 0.0001);
                if (SecondSymbol.Substring(3, 3) == "USD")
                    SC = _symbolSecondSeries.Close[i] / (_secondsymbol.PipSize / 0.0001);
                secondtotal0 += SC;
            }
            _ratio0 = Math.Round(firsttotal0 / secondtotal0, 2);
            #endregion
            #region _ratio1
            for (int i = FirstIndex - Period * 2; i < FirstIndex - Period; i++)
            {
                double FC = 0;
                if (FirstSymbol.Substring(0, 3) == "USD")
                    FC = 1 / _symbolFirstSeries.Close[i] * (_firstsymbol.PipSize / 0.0001);
                if (FirstSymbol.Substring(3, 3) == "USD")
                    FC = _symbolFirstSeries.Close[i] / (_firstsymbol.PipSize / 0.0001);
                firsttotal1 += FC;
            }
            for (int i = SecondIndex - Period * 2; i < SecondIndex - Period; i++)
            {
                double SC = 0;
                if (SecondSymbol.Substring(0, 3) == "USD")
                    SC = 1 / _symbolSecondSeries.Close[i] * (_secondsymbol.PipSize / 0.0001);
                if (SecondSymbol.Substring(3, 3) == "USD")
                    SC = _symbolSecondSeries.Close[i] / (_secondsymbol.PipSize / 0.0001);
                secondtotal1 += SC;
            }
            _ratio1 = Math.Round(firsttotal1 / secondtotal1, 2);
            #endregion
            #region _ratio2
            for (int i = FirstIndex - Period * 3; i < FirstIndex - Period * 2; i++)
            {
                double FC = 0;
                if (FirstSymbol.Substring(0, 3) == "USD")
                    FC = 1 / _symbolFirstSeries.Close[i] * (_firstsymbol.PipSize / 0.0001);
                if (FirstSymbol.Substring(3, 3) == "USD")
                    FC = _symbolFirstSeries.Close[i] / (_firstsymbol.PipSize / 0.0001);
                firsttotal2 += FC;
            }
            for (int i = SecondIndex - Period * 3; i < SecondIndex - Period * 2; i++)
            {
                double SC = 0;
                if (SecondSymbol.Substring(0, 3) == "USD")
                    SC = 1 / _symbolSecondSeries.Close[i] * (_secondsymbol.PipSize / 0.0001);
                if (SecondSymbol.Substring(3, 3) == "USD")
                    SC = _symbolSecondSeries.Close[i] / (_secondsymbol.PipSize / 0.0001);
                secondtotal2 += SC;
            }
            _ratio2 = Math.Round(firsttotal2 / secondtotal2, 2);
            #endregion
            #region _ratio3
            for (int i = FirstIndex - Period * 4; i < FirstIndex - Period * 3; i++)
            {
                double FC = 0;
                if (FirstSymbol.Substring(0, 3) == "USD")
                    FC = 1 / _symbolFirstSeries.Close[i] * (_firstsymbol.PipSize / 0.0001);
                if (FirstSymbol.Substring(3, 3) == "USD")
                    FC = _symbolFirstSeries.Close[i] / (_firstsymbol.PipSize / 0.0001);
                firsttotal3 += FC;
            }
            for (int i = SecondIndex - Period * 4; i < SecondIndex - Period * 3; i++)
            {
                double SC = 0;
                if (SecondSymbol.Substring(0, 3) == "USD")
                    SC = 1 / _symbolSecondSeries.Close[i] * (_secondsymbol.PipSize / 0.0001);
                if (SecondSymbol.Substring(3, 3) == "USD")
                    SC = _symbolSecondSeries.Close[i] / (_secondsymbol.PipSize / 0.0001);
                secondtotal3 += SC;
            }
            _ratio3 = Math.Round(firsttotal3 / secondtotal3, 2);
            #endregion
            double[] _r = 
            {
                _ratio0,
                _ratio1,
                _ratio2,
                _ratio3
            };
            list_ratio.AddRange(_r);
            for (int i = 0; i < list_ratio.Count; i++)
            {
                if (list_ratio[i] < 10)
                    list_ratio[i] = Math.Round(list_ratio[i]);

                if (list_ratio[i] >= 10)
                    list_ratio[i] = Math.Round(list_ratio[i] / 5) * 5;
            }
            var result = from item in list_ratio
                group item by item into gro
                orderby gro.Count() descending
                select new 
                {
                    num = gro.Key,
                    nums = gro.Count()
                };
            double d_ratio = 1;
            foreach (var item in result.Take(1))
            {
                d_ratio = item.num;
            }
            _ratio = string.Format("{0:00}", d_ratio) + "_" + _r[0].ToString() + "_" + _r[1].ToString() + "_" + _r[2].ToString() + "_" + _r[3].ToString();
            ChartObjects.DrawText("Ratio", "\n" + _ratio, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Ratio2", "\n\nRatio-" + Ratio.ToString() + "_Magnify-" + Magnify.ToString(), StaticPosition.TopLeft, NoCorel);
        }
    }
}
