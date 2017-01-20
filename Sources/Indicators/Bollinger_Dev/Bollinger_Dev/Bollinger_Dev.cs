using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class Bollinger_Dev : Indicator
    {
        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Period", DefaultValue = 40)]
        public int Period { get; set; }

        [Parameter("SD Weight Coef", DefaultValue = 2)]
        public double K { get; set; }

        [Parameter("SD Weight Coef Step", DefaultValue = 0.5)]
        public double K_S { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MaType { get; set; }

        [Output("Main")]
        public IndicatorDataSeries Main { get; set; }

        [Output("Upper")]
        public IndicatorDataSeries Upper { get; set; }

        [Output("Lower")]
        public IndicatorDataSeries Lower { get; set; }

        [Output("Upper Increase", Color = Colors.Blue)]
        public IndicatorDataSeries Upper_Increase { get; set; }

        [Output("Lower Increase", Color = Colors.Blue)]
        public IndicatorDataSeries Lower_Increase { get; set; }

        [Output("Upper Decrease", Color = Colors.Red)]
        public IndicatorDataSeries Upper_Decrease { get; set; }

        [Output("Lower Decrease", Color = Colors.Red)]
        public IndicatorDataSeries Lower_Decrease { get; set; }


        BollingerBands boll;
        BollingerBands boll_increase;
        BollingerBands boll_decrease;

        protected override void Initialize()
        {
            boll = Indicators.BollingerBands(Source, Period, K, MaType);
            boll_increase = Indicators.BollingerBands(Source, Period, K + K_S, MaType);
            boll_decrease = Indicators.BollingerBands(Source, Period, K - K_S, MaType);
        }
        public override void Calculate(int index)
        {

            Main[index] = boll.Main[index];
            Upper[index] = boll.Top[index];
            Lower[index] = boll.Bottom[index];
            Upper_Increase[index] = boll_increase.Top[index];
            Lower_Increase[index] = boll_increase.Bottom[index];
            Upper_Decrease[index] = boll_decrease.Top[index];
            Lower_Decrease[index] = boll_decrease.Bottom[index];
        }
    }
}
