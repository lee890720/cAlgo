using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.GMTStandardTime)]
    [Levels(-6, -5, -2, 0, 2, 5, 6)]
    public class TriForce_IND : Indicator
    {
        [Parameter("TimeFrame")]
        public TimeFrame TF { get; set; }

        [Parameter("Min Période", DefaultValue = 15)]
        public int minPe { get; set; }

        [Parameter("Boite", DefaultValue = 5)]
        public int bt { get; set; }

        [Parameter("Multiple", DefaultValue = 3)]
        public int mt { get; set; }

        [Parameter("V%", DefaultValue = 25)]
        public int Vx { get; set; }

        [Parameter("Volume min pip", DefaultValue = 3)]
        public int vl { get; set; }

        [Parameter("Candle", DefaultValue = 3)]
        public int ct { get; set; }

        [Parameter("View TDN", DefaultValue = true)]
        public bool vtdn { get; set; }

        [Parameter("View PIV", DefaultValue = true)]
        public bool vpiv { get; set; }

        [Parameter("View HA", DefaultValue = true)]
        public bool vha { get; set; }

        [Parameter("View P&S", DefaultValue = true)]
        public bool vps { get; set; }

        [Parameter("View volume", DefaultValue = true)]
        public bool vvol { get; set; }

        private IndicatorDataSeries _xOpen, _xClose, Lopen;

        private Colors color, colorvx, colorvy, colorvz;

        private int dlopenx, dlopeny, dopenx, dopeny, Pe, Rx, lbar, bar, pez;

        private MarketSeries MS;

        private double PVval, HAval, LHAval;

        private double actlast, actnow;
        private double dir = 0;
        private double di1, di2, di3, di4;

        private bool playsound = false;

        protected override void Initialize()
        {
            _xOpen = CreateDataSeries();
            _xClose = CreateDataSeries();
            Lopen = CreateDataSeries();
            Rx = minPe;
            MS = MarketData.GetSeries(TF);
            actnow = MarketSeries.Close.LastValue;
        }

        public override void Calculate(int i)
        {
            ChartObjects.RemoveAllObjects();

            GetPeriode(i);

            if (vvol == true)
                FIT(i);
            if (vtdn == true)
                CTD(i);
            if (vpiv == true)
                PIV(i);
            if (vha == true)
                CHA(i);
            if (vps == true)
                PFX(i);

            Alert();

            ChartObjects.DrawVerticalLine("Start", MS.OpenTime.LastValue, Colors.Blue);
            ChartObjects.DrawVerticalLine("Starttdn", MarketSeries.OpenTime.GetIndexByTime(MarketSeries.OpenTime.Last(Pe)), Colors.YellowGreen);
            ChartObjects.DrawVerticalLine("middletdn", MarketSeries.OpenTime.Last(Pe / 2), Colors.Orange);
            ChartObjects.DrawVerticalLine("Now", MarketSeries.OpenTime.LastValue, Colors.YellowGreen, 1, LineStyle.DotsRare);

        }

        public void Alert()
        {
            if (playsound == true)
            {
                Notifications.PlaySound("C:\\Windows\\Media\\carillons.wav");
                playsound = false;
            }
        }

        public void GetPeriode(int i)
        {
            lbar = bar;
            bar = i;
            if (bar != lbar)
            {
                var lastPe = Pe;
                var lastRx = Rx;
                LHAval = HAval;
                var ix = GetIndexByDate(MS, MarketSeries.OpenTime[i]);
                if (ix != -1)
                    Rx = 1;
                else
                    Rx = lastRx + 1;
            }

            Pe = (Rx <= minPe) ? minPe : Rx - 1;
            pez = Pe / 2;

            ChartObjects.DrawText("Period", "Période: " + Convert.ToString(Pe) + " / " + Convert.ToString(pez), StaticPosition.BottomLeft, Colors.White);
        }

        public void PIV(int i)
        {
            var phigh = MarketSeries.Close.Maximum(Pe);
            var plow = MarketSeries.Close.Minimum(Pe);
            var pclose = MarketSeries.Close[i - Pe + 1];
            var nbor = 1.61803398875;

            var piv = (phigh + plow + pclose) / 3;
            var l1 = piv - ((piv - plow) / 3);
            var l2 = piv - ((piv - l1) * nbor);
            var l3 = l1 - ((l1 - l2) * nbor);

            var h1 = piv + ((phigh - piv) / 3);
            var h2 = piv + ((h1 - piv) * nbor);
            var h3 = h2 + ((h2 - h1) * nbor);

            ChartObjects.DrawLine("PIV", MarketSeries.OpenTime.Last(Pe), piv, MarketSeries.OpenTime.LastValue, piv, Colors.White);
            ChartObjects.DrawLine("PIVh1", MarketSeries.OpenTime.Last(Pe), h1, MarketSeries.OpenTime.LastValue, h1, Colors.Green);
            ChartObjects.DrawLine("PIVh2", MarketSeries.OpenTime.Last(Pe), h2, MarketSeries.OpenTime.LastValue, h2, Colors.Green);
            ChartObjects.DrawLine("PIVh3", MarketSeries.OpenTime.Last(Pe), h3, MarketSeries.OpenTime.LastValue, h3, Colors.YellowGreen);

            ChartObjects.DrawLine("PIVl1", MarketSeries.OpenTime.Last(Pe), l1, MarketSeries.OpenTime.LastValue, l1, Colors.DarkRed);
            ChartObjects.DrawLine("PIVl2", MarketSeries.OpenTime.Last(Pe), l2, MarketSeries.OpenTime.LastValue, l2, Colors.DarkRed);
            ChartObjects.DrawLine("PIVl3", MarketSeries.OpenTime.Last(Pe), l3, MarketSeries.OpenTime.LastValue, l3, Colors.Red);

            var indextime = MarketSeries.OpenTime.GetIndexByTime(MarketSeries.OpenTime.LastValue);

            ChartObjects.DrawText("PIVTxt1x", "R1", indextime, h1, VerticalAlignment.Top, HorizontalAlignment.Right, Colors.Blue);
            ChartObjects.DrawText("PIVTxt1y", "S1", indextime, l1, VerticalAlignment.Top, HorizontalAlignment.Right, Colors.Blue);
            ChartObjects.DrawText("PIVTxt2x", "R2", indextime, h2, VerticalAlignment.Top, HorizontalAlignment.Right, Colors.Blue);
            ChartObjects.DrawText("PIVTxt2y", "S2", indextime, l2, VerticalAlignment.Top, HorizontalAlignment.Right, Colors.Blue);
            ChartObjects.DrawText("PIVTxt3x", "R3", indextime, h3, VerticalAlignment.Top, HorizontalAlignment.Right, Colors.Blue);
            ChartObjects.DrawText("PIVTxt3y", "S3", indextime, l3, VerticalAlignment.Top, HorizontalAlignment.Right, Colors.Blue);

            ChartObjects.DrawLine("PIVlx", MarketSeries.OpenTime.Last(Pe), plow, MarketSeries.OpenTime.LastValue, plow, Colors.Gray);
            ChartObjects.DrawLine("PIVhx", MarketSeries.OpenTime.Last(Pe), phigh, MarketSeries.OpenTime.LastValue, phigh, Colors.Gray);
            ChartObjects.DrawText("PIVTxtxy", "SX", indextime, plow, VerticalAlignment.Top, HorizontalAlignment.Right, Colors.Blue);
            ChartObjects.DrawText("PIVTxtxx", "RX", indextime, phigh, VerticalAlignment.Top, HorizontalAlignment.Right, Colors.Blue);

            var PVvalx = (MarketSeries.Close.LastValue >= piv) ? MarketSeries.Close.LastValue - piv : piv - MarketSeries.Close.LastValue;
            var PVvaly = (MarketSeries.Close.LastValue >= piv) ? phigh - piv : piv - plow;
            PVval = (MarketSeries.Close.LastValue >= piv) ? (100 / 1.5) * (PVvalx / PVvaly) : -((100 / 1.5) * (PVvalx / PVvaly));

            ChartObjects.DrawText("PVvx", "PV: " + Convert.ToString(Math.Round(PVval * 1.5, 2)) + " %", StaticPosition.TopCenter, Colors.Yellow);
        }

        public void CHA(int i)
        {
            var open = MarketSeries.Open[i];
            var close = MarketSeries.Close[i];
            var high = MarketSeries.High[i];
            var low = MarketSeries.Low[i];

            var xClose = (((Math.Min(open, close) + low) / 2) + ((Math.Max(open, close) + high) / 2)) / 2;
            double xOpen;
            if (i > 0)
                xOpen = (_xOpen[i - 1] + _xClose[i - 1]) / 2;
            else
                xOpen = (open + close) / 2;

            var _xHigh = Math.Max(Math.Max(high, xOpen), xClose);
            var _xLow = Math.Min(Math.Min(low, xOpen), xClose);

            _xClose[i] = xClose;
            _xOpen[i] = xOpen;

            for (int Pex = i - Pe; Pex <= i; Pex++)
            {
                color = (_xOpen[Pex] > _xClose[Pex] && MarketSeries.Close[Pex] < _xClose[Pex]) ? Colors.Red : (_xOpen[Pex] < _xClose[Pex] && MarketSeries.Close[Pex] > _xClose[Pex]) ? Colors.YellowGreen : Colors.Orange;
                ChartObjects.DrawLine("candle" + Pex, Pex, MarketSeries.Open[Pex], Pex, MarketSeries.Close[Pex], color, ct, LineStyle.Solid);
                ChartObjects.DrawLine("Line" + Pex, Pex, MarketSeries.High[Pex], Pex, MarketSeries.Low[Pex], color, 1, LineStyle.Solid);
            }

            var HAvalx = Math.Max(_xClose[i], _xOpen[i]) - Math.Min(_xClose[i], _xOpen[i]);
            var HAvaly = _xHigh - _xLow;
            HAval = (_xClose[i] > _xOpen[i]) ? 100 * (HAvalx / HAvaly) : -(100 * (HAvalx / HAvaly));
            ChartObjects.DrawText("HAvx", "\nHA: " + Convert.ToString(Math.Round(LHAval, 2)) + " / " + Convert.ToString(Math.Round(HAval, 2)) + " %", StaticPosition.TopCenter, color);
        }

        public void CTD(int i)
        {
            for (int pxe = 0; pxe <= pez; pxe++)
            {
                Lopen[i - pxe] = MarketSeries.Open[i - (Pe - pxe)];
            }

            var lopenx = Lopen.Maximum(pez);
            var lopeny = Lopen.Minimum(pez);
            var openx = MarketSeries.Open.Maximum(pez);
            var openy = MarketSeries.Open.Minimum(pez);

            for (int pex = 0; pex <= Pe; pex++)
            {
                var zopen = MarketSeries.Open[i - pex];

                if (pex >= pez)
                {
                    var lastdlopenx = dlopenx;
                    dlopenx = (zopen == lopenx) ? MarketSeries.OpenTime.GetIndexByTime(MarketSeries.OpenTime[i - pex]) : lastdlopenx;

                    var lastdlopeny = dlopeny;
                    dlopeny = (zopen == lopeny) ? MarketSeries.OpenTime.GetIndexByTime(MarketSeries.OpenTime[i - pex]) : lastdlopeny;
                }
                else
                {
                    var lastdopenx = dopenx;
                    dopenx = (zopen == openx) ? MarketSeries.OpenTime.GetIndexByTime(MarketSeries.OpenTime[i - pex]) : lastdopenx;

                    var lastdopeny = dopeny;
                    dopeny = (zopen == openy) ? MarketSeries.OpenTime.GetIndexByTime(MarketSeries.OpenTime[i - pex]) : lastdopeny;
                }
            }

            var startdtdn = MarketSeries.OpenTime.GetIndexByTime(MarketSeries.OpenTime.Last(Pe));
            var stopdtdn = MarketSeries.OpenTime.GetIndexByTime(MarketSeries.OpenTime.LastValue);

            var middlopen = (dlopenx + dlopeny) / 2;
            var midlopen = (lopenx + lopeny) / 2;
            var middopen = (dopenx + dopeny) / 2;
            var midopen = (openx + openy) / 2;

            var dsttdn = (Math.Max(midlopen, midopen) - Math.Min(midlopen, midopen)) / (middopen - middlopen);

            var startvaluehigh1 = (midopen > midlopen) ? lopenx - (dsttdn * (dlopenx - startdtdn)) : lopenx + (dsttdn * (dlopenx - startdtdn));
            var stopvaluehigh1 = (midopen > midlopen) ? lopenx + (dsttdn * (stopdtdn - dlopenx)) : lopenx - (dsttdn * (stopdtdn - dlopenx));
            var startvaluehigh2 = (midopen > midlopen) ? openx - (dsttdn * (dopenx - startdtdn)) : openx + (dsttdn * (dopenx - startdtdn));
            var stopvaluehigh2 = (midopen > midlopen) ? openx + (dsttdn * (stopdtdn - dopenx)) : openx - (dsttdn * (stopdtdn - dopenx));

            var startvaluelow1 = (midopen > midlopen) ? lopeny - (dsttdn * (dlopeny - startdtdn)) : lopeny + (dsttdn * (dlopeny - startdtdn));
            var stopvaluelow1 = (midopen > midlopen) ? lopeny + (dsttdn * (stopdtdn - dlopeny)) : lopeny - (dsttdn * (stopdtdn - dlopeny));
            var startvaluelow2 = (midopen > midlopen) ? openy - (dsttdn * (dopeny - startdtdn)) : openy + (dsttdn * (dopeny - startdtdn));
            var stopvaluelow2 = (midopen > midlopen) ? openy + (dsttdn * (stopdtdn - dopeny)) : openy - (dsttdn * (stopdtdn - dopeny));

            ChartObjects.DrawLine("HTDN1", startdtdn, startvaluehigh1, stopdtdn, stopvaluehigh1, Colors.YellowGreen, 1, LineStyle.DotsRare);
            ChartObjects.DrawLine("HTDN2", startdtdn, startvaluehigh2, stopdtdn, stopvaluehigh2, Colors.YellowGreen, 1, LineStyle.DotsVeryRare);
            ChartObjects.DrawLine("LTDN1", startdtdn, startvaluelow1, stopdtdn, stopvaluelow1, Colors.Red, 1, LineStyle.DotsRare);
            ChartObjects.DrawLine("LTDN2", startdtdn, startvaluelow2, stopdtdn, stopvaluelow2, Colors.Red, 1, LineStyle.DotsVeryRare);

            ChartObjects.DrawLine("MTDN", startdtdn, (startvaluehigh1 + startvaluelow1) / 2, stopdtdn, (stopvaluehigh1 + stopvaluelow1) / 2, Colors.Red, 1, LineStyle.Lines);
        }

        public void PFX(int i)
        {
            actnow = MarketSeries.Close[i - (Pe + 1)];
            dir = di1 = di2 = di3 = di4 = 0;
            var rt = bt * mt;

            for (int pex = i - (Pe + 1); pex <= i; pex++)
            {
                var closex = MarketSeries.Close[pex];
                var lastactnow = actnow;
                var lastdir = dir;

                if (closex > (actnow + (rt * Symbol.PipSize)) && dir <= -1)
                {
                    di1 = di2;
                    di2 = di3;
                    di3 = di4;
                    di4 = dir;
                    actnow = lastactnow + (rt * Symbol.PipSize);
                    actlast = lastactnow;
                    dir = 3;
                    playsound = true;
                }

                else if (closex < (actnow - (rt * Symbol.PipSize)) && dir >= 1)
                {
                    di1 = di2;
                    di2 = di3;
                    di3 = di4;
                    di4 = dir;
                    actnow = lastactnow - (rt * Symbol.PipSize);
                    actlast = lastactnow;
                    dir = -3;
                    playsound = true;
                }

                else if (closex < (actnow - (bt * Symbol.PipSize)) && dir <= 0)
                {
                    var dirx = Math.Round((((actnow + (bt * Symbol.PipSize)) - closex) / Symbol.PipSize) / bt, 0);
                    actnow = lastactnow - ((bt * dirx) * Symbol.PipSize);
                    dir = lastdir - dirx;
                    playsound = false;
                }

                else if (closex > (actnow + (bt * Symbol.PipSize)) && dir >= 0)
                {
                    var dirx = Math.Round(((closex - (actnow + (bt * Symbol.PipSize))) / Symbol.PipSize) / bt, 0);
                    actnow = lastactnow + ((bt * dirx) * Symbol.PipSize);
                    dir = lastdir + dirx;
                    playsound = false;
                }

                var rtx = (dir > 0) ? actnow - (rt * Symbol.PipSize) : actnow + (rt * Symbol.PipSize);
                var rty = (dir > 0) ? actnow + (bt * Symbol.PipSize) : actnow - (bt * Symbol.PipSize);
                ChartObjects.DrawLine("Rx", i - 1, rtx, i + 1, rtx, Colors.Blue);
                ChartObjects.DrawLine("Ry", i - 1, rty, i + 1, rty, Colors.Cyan);
            }

            var Colorsb = (actlast > actnow) ? Colors.Red : Colors.YellowGreen;
            var Colors2 = (actlast < actnow) ? Colors.Red : Colors.YellowGreen;
            var Colors3 = Colorsb;
            var Colors4 = Colors2;
            var Colors5 = Colorsb;

            var bdcolor1 = (Math.Max(dir, -dir) >= Math.Max(di4, -di4)) ? Colorsb : Colors.Orange;
            var bdcolor2 = (Math.Max(di4, -di4) >= Math.Max(di3, -di3)) ? Colors2 : Colors.Orange;
            var bdcolor3 = (Math.Max(di3, -di3) >= Math.Max(di2, -di2)) ? Colors3 : Colors.Orange;
            var bdcolor4 = (Math.Max(di2, -di2) >= Math.Max(di1, -di1)) ? Colors4 : Colors.Orange;

            var mx = Math.Max(Math.Max(Math.Max(di1, di2), Math.Max(di3, di4)), dir);
            var my = Math.Min(Math.Min(Math.Min(di1, di2), Math.Min(di3, di4)), dir);
            var div = Math.Max(mx, -my);
            var btx = bt / div;

            var l5x = MarketSeries.Open[i] - (((di1 / 2) * btx) * Symbol.PipSize);
            var l5y = l5x + ((di1 * btx) * Symbol.PipSize);
            ChartObjects.DrawLine("last5", i + 3, l5x, i + 3, l5y, Colors5, ct);

            var l4x = (di1 > 0) ? l5y - (btx * Symbol.PipSize) : l5y + (btx * Symbol.PipSize);
            var l4y = l4x + ((di2 * btx) * Symbol.PipSize);
            ChartObjects.DrawLine("last4", i + 4, l4x, i + 4, l4y, bdcolor4, ct);

            var l3x = (di2 > 0) ? l4y - (btx * Symbol.PipSize) : l4y + (btx * Symbol.PipSize);
            var l3y = l3x + ((di3 * btx) * Symbol.PipSize);
            ChartObjects.DrawLine("last3", i + 5, l3x, i + 5, l3y, bdcolor3, ct);

            var l2x = (di3 > 0) ? l3y - (btx * Symbol.PipSize) : l3y + (btx * Symbol.PipSize);
            var l2y = l2x + ((di4 * btx) * Symbol.PipSize);
            ChartObjects.DrawLine("last2", i + 6, l2x, i + 6, l2y, bdcolor2, ct);

            var l1x = (di4 > 0) ? l2y - (btx * Symbol.PipSize) : l2y + (btx * Symbol.PipSize);
            var l1y = l1x + ((dir * btx) * Symbol.PipSize);
            ChartObjects.DrawLine("last1", i + 7, l1x, i + 7, l1y, bdcolor1, ct);

            ChartObjects.DrawText("Val5", "Last5: " + Convert.ToString(Math.Round(di1, 0)), StaticPosition.TopRight, Colors5);
            ChartObjects.DrawText("Val4", "\nLast4: " + Convert.ToString(Math.Round(di2, 0)), StaticPosition.TopRight, bdcolor4);
            ChartObjects.DrawText("Val3", "\n\nLast3: " + Convert.ToString(Math.Round(di3, 0)), StaticPosition.TopRight, bdcolor3);
            ChartObjects.DrawText("Val2", "\n\n\nLast2: " + Convert.ToString(Math.Round(di4, 0)), StaticPosition.TopRight, bdcolor2);
            ChartObjects.DrawText("Val1", "\n\n\n\nLast1: " + Convert.ToString(Math.Round(dir, 0)), StaticPosition.TopRight, bdcolor1);
        }

        public void FIT(int i)
        {
            double volmax = 0;
            double voldown = 0;
            double volup = 0;
            double dirx;
            bool voltststart = false;

            for (int Pex = i - Pe; Pex <= i; Pex++)
            {
                var open = MarketSeries.Open[Pex];
                var close = MarketSeries.Close[Pex];
                var lastvolmax = volmax;
                var volmaxtst = Math.Max(MarketSeries.Open[Pex], MarketSeries.Close[Pex]) - Math.Min(MarketSeries.Open[Pex], MarketSeries.Close[Pex]);
                volmax = (lastvolmax < volmaxtst) ? volmaxtst : lastvolmax;
                var volmin = (volmax / 100) * Vx;
                var volume = Math.Max(open, close) - Math.Min(open, close);

                dirx = (open < close) ? 1 : (open > close) ? -1 : 0;

                colorvx = (dirx >= 0 && volume >= volmin) ? Colors.YellowGreen : (dirx <= 0 && volume >= volmin) ? Colors.Red : Colors.Orange;

                if (voltststart == true)
                {
                    var lastvoldown = voldown;
                    var lastvolup = volup;
                    var tstvoldown = (close < open) ? (open - close) / Symbol.PipSize : lastvoldown;
                    var tstvolup = (close > open) ? (close - open) / Symbol.PipSize : lastvolup;
                    voldown = (tstvoldown > lastvoldown) ? tstvoldown : lastvoldown;
                    volup = (tstvolup > lastvolup) ? tstvolup : lastvolup;
                }

                if (voltststart == false)
                {
                    voldown = (close < open) ? (open - close) / Symbol.PipSize : 0;
                    volup = (close > open) ? (close - open) / Symbol.PipSize : 0;
                    voltststart = true;
                }
            }

            var openy = MarketSeries.Open[i];
            var closey = MarketSeries.Close[i];

            var volumey = Math.Max(openy, closey) - Math.Min(openy, closey);
            var puimin = vl * Symbol.PipSize;

            colorvy = (voldown < volup && volumey > puimin) ? Colors.YellowGreen : (voldown > volup && volumey > puimin) ? Colors.Red : Colors.Orange;

            ChartObjects.DrawText("Volv", "Volume:", StaticPosition.TopLeft, Colors.White);

            colorvz = (voldown > volup) ? Colors.Red : (voldown < volup) ? Colors.YellowGreen : Colors.Orange;
            ChartObjects.DrawText("Volvz", "\nDown: " + Convert.ToString(Math.Round(voldown, 2)) + " / Up: " + Convert.ToString(Math.Round(volup, 2)), StaticPosition.TopLeft, colorvz);

            var volact = Math.Max(openy, closey) - Math.Min(openy, closey);
            var vlx = Math.Round(Math.Max(voldown, volup), 2);
            ChartObjects.DrawText("Volvx", "\n\nNow: " + Convert.ToString(Math.Round((volact) / Symbol.PipSize, 2)) + "/ Min: " + Convert.ToString(vl) + " / Max: " + Convert.ToString(vlx), StaticPosition.TopLeft, colorvy);

            var volpc = 100 * (volact / Math.Max(volup, voldown));

            ChartObjects.DrawText("Volvy", "\n\n\nVolume Percent: " + Convert.ToString(Math.Round(volpc / Symbol.PipSize, 2)) + "% " + " / Min: " + Convert.ToString(Vx) + "%", StaticPosition.TopLeft, colorvx);
        }

        private int GetIndexByDate(MarketSeries series, DateTime time)
        {
            for (int iz = series.Close.Count - 1; iz > 0; iz--)
            {
                if (time == series.OpenTime[iz])
                    return iz;
            }
            return -1;
        }
    }
}
