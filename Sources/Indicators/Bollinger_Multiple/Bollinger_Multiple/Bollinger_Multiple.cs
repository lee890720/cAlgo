using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class Bollinger_Multiple : Indicator
    {
        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Period", DefaultValue = 20)]
        public int Period { get; set; }

        [Parameter("Period +", DefaultValue = 5)]
        public int PeriodIncrease { get; set; }

        [Parameter("SD Weight Coef", DefaultValue = 2)]
        public double K { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MaType { get; set; }

        [Output("MainMin", Color = Colors.White)]
        public IndicatorDataSeries MainMin { get; set; }

        [Output("MainMax", Color = Colors.White)]
        public IndicatorDataSeries MainMax { get; set; }

        [Output("UpperMin", Color = Colors.Blue)]
        public IndicatorDataSeries UpperMin { get; set; }

        [Output("UpperMax", Color = Colors.Red)]
        public IndicatorDataSeries UpperMax { get; set; }

        [Output("LowerMin", Color = Colors.Red)]
        public IndicatorDataSeries LowerMin { get; set; }

        [Output("LowerMax", Color = Colors.Blue)]
        public IndicatorDataSeries LowerMax { get; set; }

        BollingerBands boll1, boll2, boll3, boll4, boll5, boll6, boll7;

        protected override void Initialize()
        {
            boll1 = Indicators.BollingerBands(Source, Period, K, MaType);
            boll2 = Indicators.BollingerBands(Source, Period + PeriodIncrease, K, MaType);
            boll3 = Indicators.BollingerBands(Source, Period + PeriodIncrease * 2, K, MaType);
            boll4 = Indicators.BollingerBands(Source, Period + PeriodIncrease * 3, K, MaType);
            boll5 = Indicators.BollingerBands(Source, Period + PeriodIncrease * 4, K, MaType);
            boll6 = Indicators.BollingerBands(Source, Period + PeriodIncrease * 5, K, MaType);
            boll7 = Indicators.BollingerBands(Source, Period + PeriodIncrease * 6, K, MaType);

        }
        public override void Calculate(int index)
        {
            double[] main = 
            {
                boll1.Main[index],
                boll2.Main[index],
                boll3.Main[index],
                boll4.Main[index],
                boll5.Main[index],
                boll6.Main[index],
                boll7.Main[index]
            };
            double[] upper = 
            {
                boll1.Top[index],
                boll2.Top[index],
                boll3.Top[index],
                boll4.Top[index],
                boll5.Top[index],
                boll6.Top[index],
                boll7.Top[index]
            };
            double[] lower = 
            {
                boll1.Bottom[index],
                boll2.Bottom[index],
                boll3.Bottom[index],
                boll4.Bottom[index],
                boll5.Bottom[index],
                boll6.Bottom[index],
                boll7.Bottom[index]
            };
            MainMin[index] = main.Min();
            MainMax[index] = main.Max();
            UpperMin[index] = upper.Min();
            UpperMax[index] = upper.Max();
            LowerMin[index] = lower.Min();
            LowerMax[index] = lower.Max();
        }
    }
}

