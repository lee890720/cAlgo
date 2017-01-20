using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AccessRights = AccessRights.None)]
    public class Bollinger_Double : Indicator
    {
        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Period", DefaultValue = 40)]
        public int Period { get; set; }

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

        BollingerBands boll;
        BollingerBands bollhalf;

        protected override void Initialize()
        {
            boll = Indicators.BollingerBands(Source, Period, K, MaType);
            bollhalf = Indicators.BollingerBands(Source, Period / 2, K, MaType);
        }
        public override void Calculate(int index)
        {
            MainMin[index] = boll.Main[index] < bollhalf.Main[index] ? boll.Main[index] : bollhalf.Main[index];
            MainMax[index] = boll.Main[index] > bollhalf.Main[index] ? boll.Main[index] : bollhalf.Main[index];
            UpperMin[index] = boll.Top[index] < bollhalf.Top[index] ? boll.Top[index] : bollhalf.Top[index];
            UpperMax[index] = boll.Top[index] > bollhalf.Top[index] ? boll.Top[index] : bollhalf.Top[index];
            LowerMin[index] = boll.Bottom[index] < bollhalf.Bottom[index] ? boll.Bottom[index] : bollhalf.Bottom[index];
            LowerMax[index] = boll.Bottom[index] > bollhalf.Bottom[index] ? boll.Bottom[index] : bollhalf.Bottom[index];
        }
    }
}
