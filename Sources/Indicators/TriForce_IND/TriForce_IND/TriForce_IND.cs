using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AutoRescale = true)]
    public class TriForce_IND : Indicator
    {
        [Parameter("Période", DefaultValue = 15)]
        public int Pe { get; set; }

        [Parameter("pip size", DefaultValue = 3)]
        public int pip { get; set; }

        [Parameter("Période Robot", DefaultValue = 20)]
        public int PeR { get; set; }

        [Parameter("Candle", DefaultValue = 3)]
        public int ct { get; set; }

        [Parameter("Long Timeframe")]
        public TimeFrame LTF { get; set; }

        [Parameter("Capital risk max %", DefaultValue = 5)]
        public int CRK { get; set; }

        [Parameter("View Timeframe", DefaultValue = true)]
        public bool vvol { get; set; }

        [Parameter("View Positions", DefaultValue = true)]
        public bool vpos { get; set; }

        private IndicatorDataSeries _xOpen, _xClose, V;

        private Colors color, bdcolor1, lbdcolor1;

        private int ix, lix, mt;

        private double HAval, LHAval, _xHigh, _xLow;

        private double actlast, actnow;
        private double dir = 0;
        private double di1, di2, di3, di4;

        private Colors Cr, Co, Cg, Cdr, Cdg, EmaC;

        private double O, C, H, L;

        private double stoplossBuy, stoplossSell;

        protected override void Initialize()
        {
            _xOpen = CreateDataSeries();
            _xClose = CreateDataSeries();
            V = CreateDataSeries();
            actnow = MarketSeries.Close.LastValue;
            mt = 1;

            Cr = Colors.Red;
            Co = Colors.Orange;
            Cg = Colors.YellowGreen;
            Cdr = Colors.DarkRed;
            Cdg = Colors.Green;
        }

        public override void Calculate(int i)
        {
            ChartObjects.RemoveAllObjects();

            //--------------------------------------------------------Variables globales

            lix = ix;
            ix = i;
            lbdcolor1 = bdcolor1;
            O = MarketSeries.Open[i];
            C = MarketSeries.Close[i];
            H = MarketSeries.High[i];
            L = MarketSeries.Low[i];

            //--------------------------------------------------------HA/MMA/PFX Début        

            ////Variables de base

            double HAvaly = Symbol.PipSize;
            string StD = "";
            double dstD = 0;
            bool Engl = false;
            bool Doji = false;
            bool FGaps = false;
            bool Hama = false;

            actnow = MarketSeries.Close[i - Pe + 1];
            dir = di1 = di2 = di3 = di4 = 0;

            double rtx = 0;

            double exp;
            string tdn4, tdn5, tdn6 = "wait";
            int Pema = Pe;
            Colors EmaCx;

            ////Calculs de Base

            if (lix != ix)
            {
                LHAval = HAval;
            }

            var xClose = (((Math.Min(O, C) + L) / 2) + ((Math.Max(O, C) + H) / 2)) / 2;
            double xOpen;
            if (i > 0)
                xOpen = (_xOpen[i - 1] + _xClose[i - 1]) / 2;
            else
                xOpen = (O + C) / 2;

            _xHigh = Math.Max(Math.Max(H, xOpen), xClose);
            _xLow = Math.Min(Math.Min(L, xOpen), xClose);

            _xClose[i] = xClose;
            _xOpen[i] = xOpen;


            var emaopen = (((Math.Min(O, C) + L) / 2) + ((Math.Max(O, C) + H) / 2)) / 2;
            exp = 8.0 / (Pema + 1);
            var exmax = V[i - 1];
            var exma1 = (double.IsNaN(exmax)) ? emaopen : emaopen * exp + exmax * (1 - exp);
            V[i] = exma1;

            //--------------------------------------------------------HA/MMA/PFX Boucle

            for (int Pex = i - Pe; Pex <= i; Pex++)
            {
                //Variables de boucle
                var ox = MarketSeries.Open[Pex];
                var cx = MarketSeries.Close[Pex];
                var hx = MarketSeries.High[Pex];
                var lx = MarketSeries.Low[Pex];
                var me = MarketSeries.Median[Pex];
                var lcx = MarketSeries.Close[Pex - 1];
                var lox = MarketSeries.Open[Pex - 1];
                var lhx = MarketSeries.High[Pex - 1];
                var llx = MarketSeries.Low[Pex - 1];

                var lHAvaly = HAvaly;
                var HAvalyx = Math.Max(_xClose[Pex], _xOpen[Pex]) - Math.Min(_xClose[Pex], _xOpen[Pex]);
                HAvaly = (HAvalyx > lHAvaly) ? HAvalyx : lHAvaly;
                var lval = _xClose[Pex - 1] - _xOpen[Pex - 1];
                var val = _xClose[Pex] - _xOpen[Pex];
                color = (_xOpen[Pex] > _xClose[Pex] && MarketSeries.Close[Pex] < _xClose[Pex] && lval < val) ? Cdr : (_xOpen[Pex] > _xClose[Pex] && MarketSeries.Close[Pex] < _xClose[Pex] && lval >= val) ? Cr : (_xOpen[Pex] < _xClose[Pex] && MarketSeries.Close[Pex] > _xClose[Pex] && lval > val) ? Cdg : (_xOpen[Pex] < _xClose[Pex] && MarketSeries.Close[Pex] > _xClose[Pex] && lval <= val) ? Cg : Co;

                var hlx = ((hx - lx) * 3) / 2;
                var mxh = ((hx + lx) / 2) + (((hx - lx) / 100) * 33);
                var mxl = ((hx + lx) / 2) - (((hx - lx) / 100) * 33);
                var ex = 3 * (Math.Max(ox, cx) - Math.Min(ox, cx));
                var ey = 3 * (Math.Max(lox, lcx) - Math.Min(lox, lcx));
                Doji = ((ox == cx && ox >= mxh) || (ox == cx && ox <= mxl)) ? true : false;
                Engl = ((Math.Max(ox, cx) > Math.Max(lox, lcx) && Math.Min(ox, cx) < Math.Min(lox, lcx)) || (Math.Max(ox, cx) > Math.Max(MarketSeries.Open[Pex - 2], MarketSeries.Close[Pex - 2]) && Math.Min(ox, cx) < Math.Min(MarketSeries.Open[Pex - 2], MarketSeries.Close[Pex - 2]))) ? true : false;
                FGaps = ((ox < cx && lox < lcx && lcx < ox && Engl == false) || (ox > cx && lox > lcx && lcx > ox && Engl == false)) ? true : false;
                Hama = (ex < (ey / 2) && Math.Max(ox, cx) < Math.Max(lox, lcx) && Math.Min(ox, cx) > Math.Min(lox, lcx)) ? true : false;
                var dstup = hx + (((hx - lx) / 100) * 33);
                var dstdn = lx - (((hx - lx) / 100) * 33);

                var lastactnow = actnow;
                var lastdir = dir;

                var lEmac = EmaC;
                EmaC = (V[Pex] < cx && V[Pex] < me) ? Cg : (V[Pex] > cx && V[Pex] > me) ? Cr : lEmac;
                tdn6 = (EmaC == Cg) ? "Buy" : "Sell";

                //Point et Figure

                if (dir >= 0)
                {
                    if (cx < (actnow - ((pip * mt) * Symbol.PipSize)))
                    {
                        di1 = di2;
                        di2 = di3;
                        di3 = di4;
                        di4 = dir;
                        var newactnow = actnow - ((pip * mt) * Symbol.PipSize);
                        actlast = actnow;
                        actnow = newactnow;
                        dir = -(pip * mt);

                        var dirx = Math.Floor(((actnow - (mt * Symbol.PipSize)) - cx) / (mt * Symbol.PipSize));
                        var newactnowx = actnow - ((mt * dirx) * Symbol.PipSize);
                        actnow = newactnowx;
                        dir = dir - dirx - 1;
                    }

                    if (cx > (actnow + (mt * Symbol.PipSize)))
                    {
                        var dirx = Math.Floor((cx - (actnow + (mt * Symbol.PipSize))) / (mt * Symbol.PipSize));
                        var newactnowx = actnow + ((mt * dirx) * Symbol.PipSize);
                        actnow = newactnowx;
                        dir = lastdir + dirx;
                    }
                }

                else if (dir <= 0)
                {
                    if (cx > (actnow + ((pip * mt) * Symbol.PipSize)))
                    {
                        di1 = di2;
                        di2 = di3;
                        di3 = di4;
                        di4 = dir;
                        var newactnow = actnow + ((pip * mt) * Symbol.PipSize);
                        actlast = actnow;
                        actnow = newactnow;
                        dir = (pip * mt);

                        var dirx = Math.Floor((cx - (actnow + (mt * Symbol.PipSize))) / (mt * Symbol.PipSize));
                        var newactnowx = actnow + ((mt * dirx) * Symbol.PipSize);
                        actnow = newactnowx;
                        dir = dir + dirx + 1;
                    }

                    if (cx < (actnow - (mt * Symbol.PipSize)))
                    {
                        var dirx = Math.Floor(((actnow - (mt * Symbol.PipSize)) - cx) / (mt * Symbol.PipSize));
                        var newactnowx = actnow - ((mt * dirx) * Symbol.PipSize);
                        actnow = newactnowx;
                        dir = lastdir - dirx;
                    }
                }

                rtx = (dir > 0) ? actnow - ((pip * mt) * Symbol.PipSize) : actnow + ((pip * mt) * Symbol.PipSize);
                var rty = (dir > 0) ? actnow + (mt * Symbol.PipSize) : actnow - (mt * Symbol.PipSize);

                //Création graphique

                ChartObjects.DrawLine("Candle" + Pex, Pex, MarketSeries.Open[Pex], Pex, MarketSeries.Close[Pex], color, ct, LineStyle.Solid);
                ChartObjects.DrawLine("Line" + Pex, Pex, MarketSeries.High[Pex], Pex, MarketSeries.Low[Pex], color, 1, LineStyle.Solid);

                StD = (Doji == true && ox < me && ox > V[Pex]) ? "D\n▼" : (Doji == true && ox > me && ox < V[Pex]) ? "▲\nD" : "";
                dstD = (_xOpen[Pex] < _xClose[Pex]) ? dstdn : dstup;
                ChartObjects.DrawText("Doji" + Pex, StD, Pex, dstD, VerticalAlignment.Center, HorizontalAlignment.Center, color);
                StD = (Engl == true && ox > cx && ox > V[Pex]) ? "E\n▼" : (Engl == true && ox < cx && ox < V[Pex]) ? "▲\nE" : "";
                dstD = (ox < cx) ? dstdn : dstup;
                ChartObjects.DrawText("Englobant" + Pex, StD, Pex, dstD, VerticalAlignment.Center, HorizontalAlignment.Center, color);
                StD = (FGaps == true && ox > cx && ox > V[Pex]) ? "G\n▼" : (FGaps == true && ox < cx && ox < V[Pex]) ? "▲\nG" : "";
                dstD = (ox < cx) ? dstdn : dstup;
                ChartObjects.DrawText("Gaps" + Pex, StD, Pex, dstD, VerticalAlignment.Center, HorizontalAlignment.Center, color);
                StD = (Hama == true && ox > cx && lox < lcx && ox > V[Pex]) ? "H\n▼" : (Hama == true && ox < cx && lox > lcx && ox < V[Pex]) ? "▲\nH" : "";
                dstD = (ox < cx) ? dstdn : dstup;
                ChartObjects.DrawText("Hamari" + Pex, StD, Pex, dstD, VerticalAlignment.Center, HorizontalAlignment.Center, color);

                ChartObjects.DrawLine("Rx", i - 1, rtx, i + 1, rtx, Colors.Blue);
                ChartObjects.DrawLine("Rz", i - 1, actnow, i + 1, actnow, Colors.BlueViolet);
                ChartObjects.DrawLine("Ry", i - 1, rty, i + 1, rty, Colors.Cyan);

                ChartObjects.DrawLine("EMA" + Pex, Pex - 1, V[Pex - 1], Pex, V[Pex], EmaC, 2);

            }

            //--------------------------------------------------------HA/MMA/PFX Fin

            ///HeikinAshi & Chandeliers FIN

            var HAvalx = Math.Max(_xClose[i], _xOpen[i]) - Math.Min(_xClose[i], _xOpen[i]);
            HAval = (_xClose[i] > _xOpen[i]) ? 100 * (HAvalx / HAvaly) : -(100 * (HAvalx / HAvaly));

            tdn5 = (color == Cg) ? "Buy" : (color == Cdg) ? "Wait on Buy" : (color == Cr) ? "Sell" : (color == Cdr) ? "Wait on Sell" : "Wait";

            ///Point et Figure FIN

            var Colorsb = (actlast > actnow) ? Cr : Cg;
            var Colors2 = (actlast < actnow) ? Cr : Cg;
            var Colors3 = Colorsb;
            var Colors4 = Colors2;
            var Colors5 = Colorsb;

            bdcolor1 = (Math.Max(dir, -dir) >= Math.Max(di4, -di4)) ? Colorsb : Co;
            var bdcolor2 = (Math.Max(di4, -di4) >= Math.Max(di3, -di3)) ? Colors2 : Co;
            var bdcolor3 = (Math.Max(di3, -di3) >= Math.Max(di2, -di2)) ? Colors3 : Co;
            var bdcolor4 = (Math.Max(di2, -di2) >= Math.Max(di1, -di1)) ? Colors4 : Co;

            var mx = Math.Max(Math.Max(Math.Max(di1, di2), Math.Max(di3, di4)), dir);
            var my = Math.Min(Math.Min(Math.Min(di1, di2), Math.Min(di3, di4)), dir);
            var div = Math.Max(mx, -my);
            var btx = (((MarketSeries.High.Maximum(Pe) - MarketSeries.Low.Minimum(Pe)) / 4) / Symbol.PipSize) / div;

            var fpfx = Math.Max(Math.Max(Math.Max(Math.Max(dir, -dir), Math.Max(di1, -di1)), Math.Max(Math.Max(di2, -di2), Math.Max(di3, -di3))), Math.Max(di4, -di4));
            var fpfy = Math.Min(Math.Min(Math.Min(Math.Max(dir, -dir), Math.Max(di1, -di1)), Math.Min(Math.Max(di2, -di2), Math.Max(di3, -di3))), Math.Max(di4, -di4));
            var fpfmax = fpfx - fpfy;
            var fpfpc = (dir > 0) ? 100 * ((dir - fpfy) / fpfmax) : -(100 * (((-dir) - fpfy) / fpfmax));

            tdn4 = (bdcolor1 == Cg) ? "Buy" : (bdcolor1 == Cr) ? "Sell" : "Wait";

            ///EXMA

            EmaCx = (tdn6 == "Buy") ? Cg : (tdn6 == "Sell") ? Cr : Co;

            //Création des textes bis

            ChartObjects.DrawText("Tdnha", "\n\n\n\nHeikinAshi: " + tdn5, StaticPosition.TopLeft, color);
            ChartObjects.DrawText("HAvx", "\n\n\n\n\nHA: " + Convert.ToString(Math.Round(LHAval, 2)) + " / " + Convert.ToString(Math.Round(HAval, 2)) + " %", StaticPosition.TopLeft, color);

            ChartObjects.DrawText("Tdnpf", "Point & Figure: " + tdn4, StaticPosition.TopLeft, bdcolor1);
            ChartObjects.DrawText("pxf5", "\n" + Convert.ToString(Math.Round(di1, 0)), StaticPosition.TopLeft, Colors5);
            ChartObjects.DrawText("pxf4", "\n        " + Convert.ToString(Math.Round(di2, 0)), StaticPosition.TopLeft, bdcolor4);
            ChartObjects.DrawText("pxf3", "\n                " + Convert.ToString(Math.Round(di3, 0)), StaticPosition.TopLeft, bdcolor3);
            ChartObjects.DrawText("pxf2", "\n                        " + Convert.ToString(Math.Round(di4, 0)), StaticPosition.TopLeft, bdcolor2);
            ChartObjects.DrawText("pxf1", "\n                                " + Convert.ToString(Math.Round(dir, 0)), StaticPosition.TopLeft, bdcolor1);
            ChartObjects.DrawText("fpfperc", "\n\nP&F: " + Convert.ToString(Math.Round(fpfpc, 2)) + " %", StaticPosition.TopLeft, bdcolor1);

            ChartObjects.DrawText("Tdnema", "\n\n\n\n\n\n\nEXMA: " + tdn6, StaticPosition.TopLeft, EmaCx);

            //--------------------------------------------------------Calcul Tvol

            if (vvol == true)
            {
                var rxx = TFstr(MarketSeries.TimeFrame.ToString());

                double prcx = 0;
                double pipx = 0;
                double totx = 0;
                string direct = "Wait";

                for (int rx = rxx; rx <= 10; rx++)
                {
                    var TFx = TTF(rx);
                    var _MS = MarketData.GetSeries(TFx);
                    var open = _MS.Open.LastValue;
                    var close = MarketSeries.Close.LastValue;
                    var high = _MS.High.LastValue;
                    var low = _MS.Low.LastValue;
                    var median = _MS.Median.LastValue;
                    var lopen = _MS.Open.Last(1);
                    var lclose = _MS.Close.Last(1);
                    var llopen = _MS.Open.Last(2);
                    var llclose = _MS.Close.Last(2);
                    var stade1 = (lclose > lopen) ? lclose - ((Math.Max(lclose, lopen) - Math.Min(lclose, lopen)) / 3) : lclose + ((Math.Max(lclose, lopen) - Math.Min(lclose, lopen)) / 3);
                    var stade2 = (lclose > lopen) ? lclose - (((Math.Max(lclose, lopen) - Math.Min(lclose, lopen)) / 3) * 2) : lclose + (((Math.Max(lclose, lopen) - Math.Min(lclose, lopen)) / 3) * 2);
                    direct = ((lclose >= lopen && close > lclose) || (lclose <= lopen && close >= lopen)) ? "Buy" : ((lclose >= lopen && close <= lclose && close > stade1) || (lclose <= lopen && close >= stade2 && close < lopen)) ? "Wait Buy" : ((lclose >= lopen && close <= stade2 && close > lopen) || (lclose <= lopen && close >= lclose && close < stade1)) ? "Wait Sell" : ((lclose >= lopen && close <= lopen) || (lclose <= lopen && close < lclose)) ? "Sell" : "Wait";
                    prcx = Math.Round(((Math.Max(open, close) - Math.Min(open, close)) / Math.Max(open, close)) * 100, 2);
                    pipx = Math.Round((Math.Max(open, close) - Math.Min(open, close)) / Symbol.PipSize, 0);
                    var pip = (close > open) ? pipx : -pipx;
                    totx = (close > open) ? Math.Round((high - open) / Symbol.PipSize, 0) : Math.Round((open - low) / Symbol.PipSize, 0);
                    var tot = Math.Round((pipx / totx) * 100, 0);
                    var Color = (direct == "Buy") ? Colors.YellowGreen : (direct == "Wait Buy") ? Colors.DarkGreen : (direct == "Sell") ? Colors.Red : (direct == "Wait Sell") ? Colors.DarkRed : Colors.Orange;
                    char rpl = '\n';
                    var rpu = rx;
                    var rll = new string(rpl, rpu);
                    var sbl = TTS(rx);

                    var xhlx = ((high - low) * 3) / 2;
                    var xmxh = ((high + low) / 2) + (((high - low) / 100) * 33);
                    var xmxl = ((high + low) / 2) - (((high - low) / 100) * 33);
                    var xex = 3 * (Math.Max(open, close) - Math.Min(open, close));
                    var xey = 3 * (Math.Max(lopen, lclose) - Math.Min(lopen, lclose));
                    var Dojix = ((open == close && open >= xmxh) || (open == close && open <= xmxl)) ? true : false;
                    var Englx = ((Math.Max(open, close) > Math.Max(lopen, lclose) && Math.Min(open, close) < Math.Min(lopen, lclose)) || (Math.Max(open, close) > Math.Max(llopen, llclose) && Math.Min(open, close) < Math.Min(llopen, llclose))) ? true : false;
                    var FGapsx = ((open < close && lopen < lclose && lclose < open && Englx == false) || (open > close && lopen > lclose && lclose > open && Englx == false)) ? true : false;
                    var Hamax = (xex < (xey / 2) && Math.Max(open, close) < Math.Max(lopen, lclose) && Math.Min(open, close) > Math.Min(lopen, lclose)) ? true : false;

                    var LongChand = (Dojix == true && open < median) ? "DOJI ▼" : (Englx == true && open > close) ? "ENGLOBANTE ▼" : (FGapsx == true && open > close) ? "GAPS ▼" : (Hamax == true && open > close && lopen < lclose) ? "HAMARI ▼" : (Dojix == true && open > median) ? "DOJI ▲" : (Englx == true && open < close) ? "ENGLOBANTE ▲" : (FGapsx == true && open < close) ? "GAPS ▲" : (Hamax == true && open < close && lopen > lclose) ? "HAMARI ▲" : "";

                    ChartObjects.DrawText("Time" + rx, rll + LongChand + " (" + Convert.ToString(tot) + "%) " + sbl, StaticPosition.TopRight, Color);
                }
            }

            //--------------------------------------------------------Calcul position

            if (vpos == true)
            {
                var _posbuy = Positions.Find("TriForce" + Symbol.Code, Symbol, TradeType.Buy);
                var _possell = Positions.Find("TriForce" + Symbol.Code, Symbol, TradeType.Sell);

                Colors PosColor;
                Colors pColor;

                double netprofitpos = Symbol.PipSize;
                double netpippos = Symbol.PipSize;
                double pospipmax = 0;
                int posc = 0;

                PosColor = (Account.UnrealizedNetProfit > 0) ? Cg : (Account.UnrealizedNetProfit < 0) ? Cr : Co;

                var postype = (_posbuy == null && _possell == null) ? "En attente: " : (_posbuy == null && _possell != null) ? "Vente en cours: " : (_posbuy != null && _possell == null) ? "Achat en cours: " : "Multipositions: ";

                var lslb = stoplossBuy;
                var lsls = stoplossSell;

                stoplossBuy = (_posbuy == null) ? C - ((MarketSeries.High.Maximum(PeR) - MarketSeries.Low.Minimum(PeR)) / 2) : lslb;
                stoplossSell = (_possell == null) ? C + ((MarketSeries.High.Maximum(PeR) - MarketSeries.Low.Minimum(PeR)) / 2) : lsls;

                foreach (var _pos in Positions)
                {
                    var lposc = posc;
                    var lnetpippos = netpippos;
                    var lpospipmax = pospipmax;
                    var lpospipmaxsell = pospipmax;
                    double profit = 0;
                    double onpip = 0;
                    double startline = 0;

                    if (_pos.Label == "TriForce" + Symbol.Code)
                    {
                        posc = lposc + 1;
                        profit = _pos.NetProfit;
                        onpip = _pos.Pips;
                        startline = _pos.EntryPrice;
                    }

                    pColor = (_pos.TradeType == TradeType.Buy && profit > 0) ? Cg : (_pos.TradeType == TradeType.Sell && profit > 0) ? Cr : Co;
                    pospipmax = (_pos.Pips > lpospipmax) ? _pos.Pips : lpospipmax;

                    ChartObjects.DrawLine(Convert.ToString(_pos.EntryPrice), MarketSeries.OpenTime.GetIndexByTime(_pos.EntryTime), startline, i, C, pColor, 2);
                }

                var Cap = (posc == 0 || posc == 1) ? 2 : CalcCap(posc);
                var pipBuy = Convert.ToInt32((Math.Max(C, stoplossBuy) - Math.Min(C, stoplossBuy)) / Symbol.PipSize);
                var pipSell = Convert.ToInt32((Math.Max(C, stoplossSell) - Math.Min(C, stoplossSell)) / Symbol.PipSize);
                var mise = (Account.Balance / 100) * CRK;
                var xpipvalb = mise / (pipBuy + Symbol.Spread);
                var xpipvals = mise / (pipSell + Symbol.Spread);
                var volbasebuy = Convert.ToInt64(xpipvalb / Symbol.PipValue);
                var volbasesell = Convert.ToInt64(xpipvals / Symbol.PipValue);
                var volL = Symbol.VolumeMin;
                var volM = Symbol.VolumeMax;
                var volumexb = (volbasebuy < volL) ? volL : (volbasebuy > volM) ? volM : volbasebuy;
                var volumexs = (volbasesell < volL) ? volL : (volbasesell > volM) ? volM : volbasesell;
                var volumebuy = Convert.ToInt64(Math.Floor(Convert.ToDouble(volumexb / Symbol.VolumeMin)) * Symbol.VolumeMin);
                var volumesell = Convert.ToInt64(Math.Floor(Convert.ToDouble(volumexs / Symbol.VolumeMin)) * Symbol.VolumeMin);

                ChartObjects.DrawText("PosProfit", postype + Convert.ToString(Math.Round(Account.UnrealizedNetProfit, 2)) + " € (" + Convert.ToString(Math.Round(pospipmax, 2)) + " pip) Spread: " + Convert.ToString(Math.Round((Symbol.Ask - Symbol.Bid) / Symbol.PipSize, 2)), StaticPosition.TopCenter, PosColor);
                ChartObjects.DrawText("InfoBuy", "\nB : " + "CRK " + Convert.ToString(CRK) + "% / CAP " + Convert.ToString(Cap) + "pip / PMax " + Convert.ToString(Math.Round(mise, 2)) + "€ / PipVal " + Convert.ToString(Math.Round(volumebuy * Symbol.PipValue, 2)) + "€ / Volume " + Convert.ToString(volumebuy) + " (" + Convert.ToString(pipBuy) + ")", StaticPosition.TopCenter, PosColor);
                ChartObjects.DrawText("InfoSell", "\n\nS : " + "CRK " + Convert.ToString(CRK) + "% / CAP " + Convert.ToString(Cap) + "pip / PMax " + Convert.ToString(Math.Round(mise, 2)) + "€ / PipVal " + Convert.ToString(Math.Round(volumesell * Symbol.PipValue, 2)) + "€ / Volume " + Convert.ToString(volumesell) + " (" + Convert.ToString(pipSell) + ")", StaticPosition.TopCenter, PosColor);
            }

            //--------------------------------------------------------Calcul Fibonacci

            var _MSx = MarketData.GetSeries(LTF);
            var max = Math.Max(_MSx.High.Last(1), _MSx.High.LastValue) - ((Math.Max(_MSx.High.Last(1), _MSx.High.LastValue) - Math.Min(_MSx.Low.Last(1), _MSx.Low.LastValue)) / 3);
            var min = Math.Min(_MSx.Low.Last(1), _MSx.Low.LastValue) + ((Math.Max(_MSx.High.Last(1), _MSx.High.LastValue) - Math.Min(_MSx.Low.Last(1), _MSx.Low.LastValue)) / 3);
            var fO = _MSx.Open.Last(1);
            var nbor = 0.61803398875;

            var Pv = fO;
            var S1 = fO - ((max - min) * nbor);
            var S2 = fO - ((max - min) * (nbor * 2));
            var S3 = fO - ((max - min) * (nbor * 3));
            var S4 = fO - ((max - min) * (nbor * 4));
            var R1 = fO + ((max - min) * nbor);
            var R2 = fO + ((max - min) * (nbor * 2));
            var R3 = fO + ((max - min) * (nbor * 3));
            var R4 = fO + ((max - min) * (nbor * 4));

            var dstmax = Math.Max(fO, max) - Math.Min(fO, max);
            var dstmin = Math.Max(fO, min) - Math.Min(fO, min);

            var lastix = MarketSeries.OpenTime.GetIndexByTime(_MSx.OpenTime.Last(1));

            ChartObjects.DrawLine("R1x", lastix, R1, i, R1, Cdg, 1, LineStyle.DotsVeryRare);
            ChartObjects.DrawLine("R2x", lastix, R2, i, R2, Cdg, 1, LineStyle.DotsVeryRare);
            ChartObjects.DrawLine("R3x", lastix, R3, i, R3, Cg, 1, LineStyle.DotsVeryRare);
            ChartObjects.DrawLine("R4x", lastix, R4, i, R4, Cg, 1, LineStyle.DotsVeryRare);

            ChartObjects.DrawLine("Pvx", lastix, Pv, i, Pv, Co, 1, LineStyle.DotsVeryRare);

            ChartObjects.DrawLine("S1x", lastix, S1, i, S1, Cdr, 1, LineStyle.DotsVeryRare);
            ChartObjects.DrawLine("S2x", lastix, S2, i, S2, Cdr, 1, LineStyle.DotsVeryRare);
            ChartObjects.DrawLine("S3x", lastix, S3, i, S3, Cr, 1, LineStyle.DotsVeryRare);
            ChartObjects.DrawLine("S4x", lastix, S4, i, S4, Cr, 1, LineStyle.DotsVeryRare);

            //--------------------------------------------------------Calcul Alerte (MODIFIER)

            //if (nbring == 0)
            //{
            //for (int rt = 0; rt <= 3; rt++)
            //{
            //Notifications.PlaySound("C:\\302.wav");
            //}
            //nbring = 1;
            //}
        }

        public int TFstr(string TFs)
        {
            switch (TFs)
            {
                case "Minute1":
                case "Minute2":
                case "Minute3":
                case "Minute4":
                    return 1;
                    break;
                case "Minute5":
                case "Minute6":
                case "Minute7":
                case "Minute8":
                case "Minute9":
                case "Minute10":
                    return 2;
                    break;
                case "Minute15":
                case "Minute20":
                    return 3;
                    break;
                case "Minute30":
                case "Minute45":
                    return 4;
                    break;
                case "Hour":
                    return 5;
                    break;
                case "Hour2":
                case "Hour3":
                    return 6;
                    break;
                case "Hour4":
                case "Hour6":
                case "Hour8":
                    return 7;
                    break;
                case "Hour12":
                    return 8;
                    break;
                case "Daily":
                case "Day2":
                case "Day3":
                    return 9;
                    break;
                case "Weekly":
                    return 10;
                    break;
                default:
                    return 1;
                    break;
            }
        }

        public TimeFrame TTF(int rz)
        {
            switch (rz)
            {
                case 1:
                    return TimeFrame.Minute5;
                    break;
                case 2:
                    return TimeFrame.Minute15;
                    break;
                case 3:
                    return TimeFrame.Minute30;
                    break;
                case 4:
                    return TimeFrame.Hour;
                    break;
                case 5:
                    return TimeFrame.Hour2;
                    break;
                case 6:
                    return TimeFrame.Hour4;
                    break;
                case 7:
                    return TimeFrame.Hour12;
                    break;
                case 8:
                    return TimeFrame.Daily;
                    break;
                case 9:
                    return TimeFrame.Weekly;
                    break;
                case 10:
                    return TimeFrame.Monthly;
                    break;
                default:
                    return TimeFrame.Minute;
                    break;
            }
        }

        public string TTS(int rz)
        {
            switch (rz)
            {
                case 1:
                    return "m5";
                    break;
                case 2:
                    return "m15";
                    break;
                case 3:
                    return "m30";
                    break;
                case 4:
                    return "h1";
                    break;
                case 5:
                    return "h2";
                    break;
                case 6:
                    return "h4";
                    break;
                case 7:
                    return "h12";
                    break;
                case 8:
                    return "D1";
                    break;
                case 9:
                    return "WE";
                    break;
                case 10:
                    return "MO";
                    break;
                default:
                    return "m1";
                    break;
            }
        }

        private int CalcCap(int posCount)
        {
            var Cappos2 = 2;
            var Cappos3 = Cappos2 + (Cappos2 * 2);
            var Cappos4 = Cappos3 + (Cappos2 * 3);
            var Cappos5 = Cappos4 + (Cappos2 * 4);
            var Cappos6 = Cappos5 + (Cappos2 * 5);
            var Cappos7 = Cappos6 + (Cappos2 * 6);
            var Cappos8 = Cappos7 + (Cappos2 * 7);
            var Cappos9 = Cappos8 + (Cappos2 * 8);
            var Cappos10 = Cappos9 + (Cappos2 * 9);
            var Cappos11 = Cappos10 + (Cappos2 * 10);

            switch (posCount)
            {
                case 1:
                    return Cappos2;
                    break;
                case 2:
                    return Cappos3;
                    break;
                case 3:
                    return Cappos4;
                    break;
                case 4:
                    return Cappos5;
                    break;
                case 5:
                    return Cappos6;
                    break;
                case 6:
                    return Cappos7;
                    break;
                case 7:
                    return Cappos8;
                    break;
                case 8:
                    return Cappos9;
                    break;
                case 9:
                    return Cappos10;
                    break;
                case 10:
                    return Cappos11;
                    break;
                default:
                    return Cappos11;
                    break;
            }
        }
    }
}
