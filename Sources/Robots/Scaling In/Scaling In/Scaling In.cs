using System;
using System.Linq;
using Microsoft.Win32;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


/*

Version 1.0
Developed by afhacker (Ahmad Noman Musleh)
Email : afhackermubasher@gmail.com

*/

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.Registry)]
    public class ScalingIn : Robot
    {
        [Parameter("Scale Prefix", DefaultValue = "sc")]
        public string scalePrefix { get; set; }

        [Parameter("Scale In after:", DefaultValue = 1)]
        public double scalePips { get; set; }

        [Parameter("Contorl Scaled In Trades Volume?", DefaultValue = false)]
        public bool scalePositionControl { get; set; }

        [Parameter("Limit Scaling In?", DefaultValue = false)]
        public bool limitScalingIn { get; set; }

        [Parameter("Scale In #", DefaultValue = 2)]
        public int scaleNumber { get; set; }

        [Parameter("Mother Close?", DefaultValue = false)]
        public bool motherClose { get; set; }


        [Parameter("ATR Periods", DefaultValue = 20)]
        public int atrPeriod { get; set; }

        [Parameter("ATR Multiplier", DefaultValue = 3)]
        public int atrMultiplier { get; set; }


        [Parameter("% Risk Per Trade", DefaultValue = 0.5, MinValue = 0.1, MaxValue = 50.0)]
        public double riskPercentage { get; set; }

        [Parameter("Slippage(Pips)", DefaultValue = 1, MinValue = 0.1)]
        public double slippage { get; set; }






        private AverageTrueRange atr;
        private MarketSeries series;


        protected override void OnStart()
        {
            CreateSubKey();
            if (motherClose)
                Positions.Closed += OnPositionClose;

        }


        private double atrResult(string symbolCode)
        {
            series = MarketData.GetSeries(symbolCode, MarketSeries.TimeFrame);
            atr = Indicators.AverageTrueRange(series, atrPeriod, MovingAverageType.Simple);
            return atr.Result.LastValue;
        }


        protected override void OnTick()
        {
            foreach (var position in Positions)
            {
                if (position.Label == "" && position.Pips > scalePips)
                {
                    scalingIn(position);
                }
            }
        }







        private void scalingIn(Position position)
        {
            bool tradeId = false;
            if (GetFromRegistry(position.Id.ToString(), "") != "")
                tradeId = true;

            string tradeLabel;

            int tradesNumber = int.Parse(GetFromRegistry(string.Format("{0} {1}", position.Id, "#"), "0"));
            double pipsCount = double.Parse(GetFromRegistry(string.Format("{0} {1}", position.Id, "pips"), "0"));



            if (limitScalingIn && tradesNumber >= scaleNumber)
                return;

            long volume;

            Symbol sym = MarketData.GetSymbol(position.SymbolCode);

            double sl = Math.Round((atrResult(sym.Code) * Math.Pow(10, sym.Digits - 1)) * atrMultiplier, 1);

            if (position.Pips >= scalePips && !tradeId)
            {
                tradesNumber = 1;
                SetValue(string.Format("{0} {1}", position.Id, "#"), tradesNumber.ToString());
                volume = PositionVolume(sl, position.SymbolCode, position.Id);
                tradeLabel = string.Format("{0} {1} {2}", scalePrefix, position.Id, tradesNumber);
                ExecuteMarketOrder(position.TradeType, sym, volume, tradeLabel, sl, null, slippage);
                pipsCount += scalePips * 2;
                SetValue(string.Format("{0} {1}", position.Id, "pips"), pipsCount.ToString());

                SetValue(position.Id.ToString(), "true");

            }
            else if (tradeId && position.Pips >= pipsCount)
            {
                tradesNumber += 1;
                SetValue(string.Format("{0} {1}", position.Id, "#"), tradesNumber.ToString());
                volume = PositionVolume(sl, position.SymbolCode, position.Id);
                tradeLabel = string.Format("{0} {1} {2}", scalePrefix, position.Id, tradesNumber);
                ExecuteMarketOrder(position.TradeType, sym, volume, tradeLabel, sl, null, slippage);
                pipsCount += scalePips;
                SetValue(string.Format("{0} {1}", position.Id, "pips"), pipsCount.ToString());
            }

        }






        // Position volume calculator
        private long PositionVolume(double stopLossInPips, string symbolCode, int tradeId = 0)
        {
            Symbol sym = MarketData.GetSymbol(symbolCode);
            double riskPercent = riskPercentage;
            int tradeNumber = int.Parse(GetFromRegistry(string.Format("{0} {1}", tradeId, "#"), "0"));
            if (scalePositionControl && tradeNumber == 1)
                riskPercent = riskPercentage / 2;
            else if (scalePositionControl && tradeNumber > 1)
                riskPercent = riskPercentage / (tradeNumber + 1);

            double costPerPip = (double)((int)(sym.PipValue * 10000000)) / 100;
            double positionSizeForRisk = Math.Round((Account.Balance * riskPercent / 100) / (stopLossInPips * costPerPip), 2);

            if (positionSizeForRisk < 0.01)
                positionSizeForRisk = 0.01;
            return sym.QuantityToVolume(positionSizeForRisk);

        }





        // Setting, getting and deleting of Registry data
        private void CreateSubKey()
        {
            RegistryKey softwarekey = Registry.CurrentUser.OpenSubKey("Software", true);
            RegistryKey botKey = softwarekey.CreateSubKey("ScalingIn");

            botKey.Close();
            softwarekey.Close();
        }

        private void SetValue(string name, string v)
        {
            RegistryKey botKey = Registry.CurrentUser.OpenSubKey("Software\\ScalingIn\\", true);
            botKey.SetValue(name, (object)v, RegistryValueKind.String);
            botKey.Close();
        }


        private string GetFromRegistry(string valueName, string defaultValue)
        {
            RegistryKey botKey = Registry.CurrentUser.OpenSubKey("Software\\ScalingIn\\", false);
            string valueData = (string)botKey.GetValue(valueName, (object)defaultValue);
            botKey.Close();
            return valueData;
        }

        private void DeleteRegistryValue(string name)
        {
            if (GetFromRegistry(name, "0") != "0")
            {
                RegistryKey botKey = Registry.CurrentUser.OpenSubKey("Software\\ScalingIn\\", true);
                botKey.DeleteValue(name);
                botKey.Close();
            }
        }

        private void DeleteRegistryKey()
        {
            bool noOpenPosition = true;

            if (!IsBacktesting)
            {
                foreach (var position in Positions)
                {
                    if (position.Label.StartsWith(scalePrefix))
                    {
                        noOpenPosition = false;
                        break;
                    }

                }
            }

            if (noOpenPosition)
            {
                RegistryKey softwareKey = Registry.CurrentUser.OpenSubKey("Software\\", true);
                softwareKey.DeleteSubKey("ScalingIn", false);
                softwareKey.Close();
            }
        }



        private void OnPositionClose(PositionClosedEventArgs args)
        {
            var position = args.Position;

            if (position.Pips < 0)
            {
                if (position.Label == "" && motherClose)
                {
                    foreach (var pos in Positions)
                    {
                        if (pos.Label.Contains(position.Id.ToString()))
                            ClosePosition(pos);
                    }
                }
                if (position.Label.StartsWith(scalePrefix))
                {
                    string id = position.Label.Substring(position.Label.IndexOf(" ") + 1, position.Id.ToString().Length);
                    double pipsCount = double.Parse(GetFromRegistry(string.Format("{0} {1}", id, "pips"), "0"));
                    if (pipsCount != 0)
                    {
                        pipsCount -= scalePips;
                        SetValue(string.Format("{0} {1}", id, "pips"), pipsCount.ToString());
                    }
                }
            }
        }



        protected override void OnStop()
        {
            DeleteRegistryKey();
        }
    }
}
