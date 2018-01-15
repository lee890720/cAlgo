using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Wave_Currency : Indicator
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

        public double _ratio;
        private MarketSeries _symbolFirstSeries, _symbolSecondSeries;
        private DateTime SymbolTime;
        private Symbol _firstsymbol, _secondsymbol;
        private int FirstIndex, SecondIndex;
        private Colors PCorel, NCorel, NoCorel;
        protected override void Initialize()
        {
            _symbolFirstSeries = MarketData.GetSeries(FirstSymbol, TimeFrame);
            _symbolSecondSeries = MarketData.GetSeries(SecondSymbol, TimeFrame);
            _firstsymbol = MarketData.GetSymbol(FirstSymbol);
            _secondsymbol = MarketData.GetSymbol(SecondSymbol);
            _ratio = 1;
            PCorel = Colors.Lime;
            NCorel = Colors.OrangeRed;
            NoCorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            SymbolTime = MarketSeries.OpenTime[index];
            FirstIndex = _symbolFirstSeries.GetIndexByDate(SymbolTime);
            SecondIndex = _symbolSecondSeries.GetIndexByDate(SymbolTime);
            GetRatio();
            //FirstClose
            double FirstClose = 0;
            if (FirstSymbol.Substring(0, 3) == "USD")
                FirstClose = 1 / _symbolFirstSeries.Close[FirstIndex] * (_firstsymbol.PipSize / 0.0001);
            if (FirstSymbol.Substring(3, 3) == "USD")
                FirstClose = _symbolFirstSeries.Close[FirstIndex] / (_firstsymbol.PipSize / 0.0001);

            //SecondClose
            double SecondClose = 0;
            if (SecondSymbol.Substring(0, 3) == "USD")
                SecondClose = 1 / _symbolSecondSeries.Close[SecondIndex] * (_secondsymbol.PipSize / 0.0001);
            if (SecondSymbol.Substring(3, 3) == "USD")
                SecondClose = _symbolSecondSeries.Close[SecondIndex] / (_secondsymbol.PipSize / 0.0001);

            Result[index] = (FirstClose / Ratio - SecondClose) * Magnify / 0.0001 + 10000;

            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }

        private void GetRatio()
        {
            #region Ratio
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
                    list_ratio[i] = Math.Round(list_ratio[i] / 10) * 10;
            }
            var result = from item in list_ratio
                group item by item into gro
                orderby gro.Count() descending
                select new 
                {
                    num = gro.Key,
                    nums = gro.Count()
                };
            foreach (var item in result.Take(1))
            {
                _ratio = item.num;
            }
            ChartObjects.DrawText("Ratio", "\nratio-" + _ratio.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Ratio2", "\n\nRatio-" + Ratio.ToString() + "_Magnify-" + Magnify.ToString(), StaticPosition.TopLeft, NoCorel);
            #endregion
        }
    }
}
