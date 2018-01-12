using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None, AutoRescale = true)]
    public class Correlation : Indicator
    {

        [Parameter(DefaultValue = "EURGBP")]
        public string Symbol1 { get; set; }
        [Parameter(DefaultValue = "EURJPY")]
        public string Symbol2 { get; set; }
        [Parameter(DefaultValue = "USDJPY")]
        public string Symbol3 { get; set; }
        [Parameter(DefaultValue = "USDCHF")]
        public string Symbol4 { get; set; }
        [Parameter(DefaultValue = "GBPUSD")]
        public string Symbol5 { get; set; }

        [Parameter(DefaultValue = 50)]
        public int Lookback { get; set; }

        [Output("Correlation1", Color = Colors.Goldenrod)]
        public IndicatorDataSeries Result1 { get; set; }
        [Output("Correlation2", Color = Colors.Blue)]
        public IndicatorDataSeries Result2 { get; set; }
        [Output("Correlation3", Color = Colors.GhostWhite)]
        public IndicatorDataSeries Result3 { get; set; }
        [Output("Correlation4", Color = Colors.Firebrick)]
        public IndicatorDataSeries Result4 { get; set; }
        [Output("Correlation5", Color = Colors.ForestGreen)]
        public IndicatorDataSeries Result5 { get; set; }

        private MarketSeries _symbol1Series;
        private MarketSeries _symbol2Series;
        private MarketSeries _symbol3Series;
        private MarketSeries _symbol4Series;
        private MarketSeries _symbol5Series;

        private Colors color1, color2, color3, color4, color5, PCorel, NCorel, NoCorel;

        protected override void Initialize()
        {
            _symbol1Series = MarketData.GetSeries(Symbol1, TimeFrame);
            _symbol2Series = MarketData.GetSeries(Symbol2, TimeFrame);
            _symbol3Series = MarketData.GetSeries(Symbol3, TimeFrame);
            _symbol4Series = MarketData.GetSeries(Symbol4, TimeFrame);
            _symbol5Series = MarketData.GetSeries(Symbol5, TimeFrame);
            PCorel = Colors.Lime;
            NCorel = Colors.OrangeRed;
            NoCorel = Colors.Gray;

        }

        public override void Calculate(int index)
        {
            DateTime date = MarketSeries.OpenTime[index];

            //get index for Symbol 2 series
            var idx1 = _symbol1Series.OpenTime.GetIndexByExactTime(date);
            var idx2 = _symbol2Series.OpenTime.GetIndexByExactTime(date);
            var idx3 = _symbol3Series.OpenTime.GetIndexByExactTime(date);
            var idx4 = _symbol4Series.OpenTime.GetIndexByExactTime(date);
            var idx5 = _symbol5Series.OpenTime.GetIndexByExactTime(date);

            if (index < Lookback || idx1 < Lookback || idx2 < Lookback || idx3 < Lookback || idx4 < Lookback || idx5 < Lookback)
                return;

            double[] tab = new double[Lookback];
            double[] tab1 = new double[Lookback];
            double[] tab2 = new double[Lookback];
            double[] tab3 = new double[Lookback];
            double[] tab4 = new double[Lookback];
            double[] tab5 = new double[Lookback];

            //populate tab1 and tab2 arrays with close for Symbol 1 (MarketSeries) and Symbol 2
            for (int i = 0; i < Lookback; i++)
            {
                tab[i] = MarketSeries.Close[index - Lookback + i];
                tab1[i] = _symbol1Series.Close[idx1 - Lookback + i];
                tab2[i] = _symbol2Series.Close[idx2 - Lookback + i];
                tab3[i] = _symbol3Series.Close[idx3 - Lookback + i];
                tab4[i] = _symbol4Series.Close[idx4 - Lookback + i];
                tab5[i] = _symbol5Series.Close[idx5 - Lookback + i];

            }

            Result1[index] = Stat.Correlation(tab, tab1);
            Result2[index] = Stat.Correlation(tab, tab2);
            Result3[index] = Stat.Correlation(tab, tab3);
            Result4[index] = Stat.Correlation(tab, tab4);
            Result5[index] = Stat.Correlation(tab, tab5);

            color1 = (Result1.LastValue > 0.8) ? PCorel : (Result1.LastValue < -0.8) ? NCorel : NoCorel;
            color2 = (Result2.LastValue > 0.8) ? PCorel : (Result2.LastValue < -0.8) ? NCorel : NoCorel;
            color3 = (Result3.LastValue > 0.8) ? PCorel : (Result3.LastValue < -0.8) ? NCorel : NoCorel;
            color4 = (Result4.LastValue > 0.8) ? PCorel : (Result4.LastValue < -0.8) ? NCorel : NoCorel;
            color5 = (Result5.LastValue > 0.8) ? PCorel : (Result5.LastValue < -0.8) ? NCorel : NoCorel;

            ChartObjects.DrawText("corel", Convert.ToString(Symbol.Code) + "      corel", StaticPosition.TopRight, Colors.AliceBlue);
            ChartObjects.DrawText("corel1", "\n" + Convert.ToString(Symbol1) + "  " + "\t", StaticPosition.TopRight, Colors.Goldenrod);
            ChartObjects.DrawText("corel12", "\n" + Convert.ToString(Math.Round(Result1.LastValue, 2)), StaticPosition.TopRight, color1);
            ChartObjects.DrawText("corel2", "\n\n" + Convert.ToString(Symbol2) + "  " + "\t", StaticPosition.TopRight, Colors.Blue);
            ChartObjects.DrawText("corel21", "\n\n" + Convert.ToString(Math.Round(Result2.LastValue, 2)), StaticPosition.TopRight, color2);
            ChartObjects.DrawText("corel3", "\n\n\n" + Convert.ToString(Symbol3) + "  " + "\t", StaticPosition.TopRight, Colors.GhostWhite);
            ChartObjects.DrawText("corel31", "\n\n\n" + Convert.ToString(Math.Round(Result3.LastValue, 2)), StaticPosition.TopRight, color3);
            ChartObjects.DrawText("corel4", "\n\n\n\n" + Convert.ToString(Symbol4) + "  " + "\t", StaticPosition.TopRight, Colors.Firebrick);
            ChartObjects.DrawText("corel41", "\n\n\n\n" + Convert.ToString(Math.Round(Result4.LastValue, 2)), StaticPosition.TopRight, color4);
            ChartObjects.DrawText("corel5", "\n\n\n\n\n" + Convert.ToString(Symbol5) + "  " + "\t", StaticPosition.TopRight, Colors.ForestGreen);
            ChartObjects.DrawText("corel51", "\n\n\n\n\n" + Convert.ToString(Math.Round(Result5.LastValue, 2)), StaticPosition.TopRight, color5);

        }
    }

    //Correlation from (c) http://mantascode.com/c-how-to-get-correlation-coefficient-of-two-arrays/
    public class Stat
    {
        public static double Correlation(double[] array1, double[] array2)
        {
            double[] array_xy = new double[array1.Length];
            double[] array_xp2 = new double[array1.Length];
            double[] array_yp2 = new double[array1.Length];
            for (int i = 0; i < array1.Length; i++)
                array_xy[i] = array1[i] * array2[i];
            for (int i = 0; i < array1.Length; i++)
                array_xp2[i] = Math.Pow(array1[i], 2.0);
            for (int i = 0; i < array1.Length; i++)
                array_yp2[i] = Math.Pow(array2[i], 2.0);
            double sum_x = 0;
            double sum_y = 0;
            foreach (double n in array1)
                sum_x += n;
            foreach (double n in array2)
                sum_y += n;
            double sum_xy = 0;
            foreach (double n in array_xy)
                sum_xy += n;
            double sum_xpow2 = 0;
            foreach (double n in array_xp2)
                sum_xpow2 += n;
            double sum_ypow2 = 0;
            foreach (double n in array_yp2)
                sum_ypow2 += n;
            double Ex2 = Math.Pow(sum_x, 2.0);
            double Ey2 = Math.Pow(sum_y, 2.0);

            return (array1.Length * sum_xy - sum_x * sum_y) / Math.Sqrt((array1.Length * sum_xpow2 - Ex2) * (array1.Length * sum_ypow2 - Ey2));
        }
    }
}
