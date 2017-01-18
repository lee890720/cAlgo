// -------------------------------------------------------------------------------------------------
//
//    TANGO TRADE OPTIMIZER
//
//    When the bid/ask price of the currency you are trading is about to hit the target (take profit), this cBot will move the take profit setting a few pips further, 
//    and will put the STOP LOSS a few pips away from the current price. This way your target will not be hit on a winning trend without risking your winning position. 
//  
//      Additionally, it will send an email every time the position is modified by the bot
//
//
//    Example: the current price is 3(A) pips away from the target. TP will be moved 10(B) pips further and SL trailed 10(C) pips away from the current price. This process will be repeated until the SL is hit.
//    
//     Contact: sebodena@gmail.com
//      Settings:
//      (A) Trigger pips: how many pips away from the target the process is executed
//      (B) Trail SL: how many pips to move the SL away from the current bid/ask price
//      (C) Trail TP:how many pips to move TP from the current TP
// -------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Text;


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TangoTradeOptimizer : Robot
    {



        Symbol currentSymbol;
        Position currentPosition;
        double? newStopLoss = 0;
        double? newTakeProfit = 0;
       
      
        [Parameter("Trigger (pips)", DefaultValue = 2.0, MinValue = 0)]
        public double TriggerPips { get; set; }

        [Parameter("Trail SL (pips)", DefaultValue = 10.0, MinValue = 0)]
        public double TrailSLPips { get; set; }

        [Parameter("Trail TP (pips)", DefaultValue = 5.0, MinValue = 0)]
        public double TrailTPPips { get; set; }

        [Parameter("Notify", DefaultValue = true)]
        public bool Notifiy { get; set; }


        [Parameter("Email Address")]
        public string EmailAddress { get; set; }


        protected override void OnStart()
        {
          

        }

        protected override void OnTick()
        {
            foreach (var pos in Positions)
            {
                this.currentSymbol = MarketData.GetSymbol(pos.SymbolCode);
                this.currentPosition = pos;
                if (isNearTarget())
                {
                   
                    CalcNewValues();

                    AdjustPosition();
                   
                    SendMail();
                }
            }
        }
        
        string BuildMessage()
        {
            StringBuilder sb=new StringBuilder();
            sb.AppendFormat("Tango Trade Optimizer modified your position:{0}",System.Environment.NewLine);
            sb.AppendFormat(@"Unr. Net:{0} {3} Stop Loss:{1}{3}  Take Profit:{2}",currentPosition.NetProfit,currentPosition.StopLoss,currentPosition.TakeProfit,System.Environment.NewLine);
            return sb.ToString();
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        void CalcNewValues()
        {
            if (currentPosition.TradeType == TradeType.Buy)
            {
                newStopLoss = currentSymbol.Bid - (this.currentSymbol.PipSize * this.TrailSLPips);
                newTakeProfit = currentSymbol.Bid + (this.currentSymbol.PipSize * this.TrailTPPips);
            }
            else
            {
                newStopLoss = currentSymbol.Ask + (this.currentSymbol.PipSize * this.TrailSLPips);
                newTakeProfit = currentSymbol.Ask - (this.currentSymbol.PipSize * this.TrailTPPips);
            }
        }



        void AdjustPosition()
        {
            if (newStopLoss == 0 || newTakeProfit == 0)
                return;
            ModifyPosition(currentPosition, newStopLoss, newTakeProfit);

        }

        void SendMail()
        {
            if (this.Notifiy)
                Notifications.SendEmail(this.EmailAddress, this.EmailAddress, "Tango Trade Optimizer", BuildMessage());

            Print("new values new SL:{0}- New TP{1}", newStopLoss, newTakeProfit);
        }



        bool isNearTarget()
        {

            if (this.currentPosition.TradeType == TradeType.Buy)
            {
                if (currentSymbol.Bid > this.currentPosition.TakeProfit - (this.currentSymbol.PipSize * this.TriggerPips))
                    return true;

            }
            else
            {
                if (currentSymbol.Ask < this.currentPosition.TakeProfit + (this.currentSymbol.PipSize * this.TriggerPips))
                    return true;
            }

            return false;
        }


    }
}
