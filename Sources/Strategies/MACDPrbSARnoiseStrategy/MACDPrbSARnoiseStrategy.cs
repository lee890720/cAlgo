#region Strategy Infos
// -------------------------------------------------------------------------------
//
//		Author : https://www.facebook.com/ab.hacid
//
//	Argunes : Forex Factory jan 2009
//
//	Hi Andi,
//
//	Thanks for offering help. I'll share a part of my trading system with you and if possible i would like to have an indicator showing my buy/sell signals.
//
//	Find part of my trading system includes moving averages.
//	Here are the values,
//	5 EMA (close),
//	8 SMA (open),
//	21 EMA (close),
//	55 EMA (close)
//	200 EMA (close).
//
//	My signals are these;
//		1. when 21 EMA is over 55 EMA; buy when 5 EMA crosses up 8 SMA,
//		2. when 21 EMA is under 55 EMA; sell when 5 EMA crosses down 8 SMA.
//
//	I also look at MACD and Stothastics for confirmation but moving average crossovers are enought for now.
//
//	I'd be glad if you send indicators to my email; argunes@hotmail.com .
//	If you have any questions about my strategy please don't hesitate to ask.
// -------------------------------------------------------------------------------
#endregion

using System;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Lib;

namespace cAlgo.Strategies
{
	public class MACDPrbSARnoiseStrategy : Strategy
    {
		#region Strategy Parameters
		public int Period_MACD_SMA { get; set; }
		public int Noise_MACD_sm { get; set; }
		public int Noise_MACD_m0 { get; set; }
		public int Noise_MACD_s0 { get; set; }
		public int Noise_Prb_SAR_ema { get; set; }
		public double Step_PrbSAR { get; set; }
		public int Period_SlowEMA { get; set; }
		public int Period_FastEMA { get; set; }
		#endregion

		#region Strategy Variables
        private MacdHistogram i_MACD_main;
        private MacdHistogram i_MCAD_signal;
     //   private SimpleMovingAverage i_MA_Close;
        private ParabolicSAR i_Parabolic_SAR;
        private SimpleMovingAverage i_MA_Open;
        private ExponentialMovingAverage i_EMAf;

		double _MACD_main;
		double _MCAD_signal;
	//	double _MA_Close;
		double _Parabolic_SAR;

		bool _isMacdMainPositive;
		bool _isMacdMainAboveMacdSignal;
		bool _isMacdSignalPositive;
		bool _isParabolicSARBelowMaClose;

		private double _dixPowerDigits;
		#endregion


		public MACDPrbSARnoiseStrategy(Robot robot)
			: base(robot)
		{
			Initialize();
		}

		protected override void Initialize()
		{
			_dixPowerDigits = Math.Pow(10, Robot.Symbol.Digits);

			i_MACD_main = Robot.Indicators.MacdHistogram(Robot.MarketSeries.Close, Period_SlowEMA, Period_FastEMA, Period_MACD_SMA);
			i_MCAD_signal = Robot.Indicators.MacdHistogram(Robot.MarketSeries.Close, Period_SlowEMA, Period_FastEMA, Period_MACD_SMA);
			//i_MA_Close = Robot.Indicators.SimpleMovingAverage(Robot.MarketSeries.Close, 1);
			i_Parabolic_SAR = Robot.Indicators.ParabolicSAR(Step_PrbSAR, 0.1);
			i_MA_Open = Robot.Indicators.SimpleMovingAverage(Robot.MarketSeries.Open, 1);
			i_EMAf = Robot.Indicators.ExponentialMovingAverage(Robot.MarketSeries.Close, Period_FastEMA);
		}

		/// <summary>
		/// Signals with noise levels on indicators MACD and Parabolic SAR. 
		/// </summary>
		/// <returns></returns>
		public override TradeType? signal1()
		{
			_MACD_main = i_MACD_main.Histogram.Last(0);
			_MCAD_signal = i_MCAD_signal.Signal.Last(0);
		//	_MA_Close = i_MA_Close.Result.Last(0);
			_Parabolic_SAR = i_Parabolic_SAR.Result.Last(0);

			_isMacdMainPositive = (_MACD_main > 0);
			_isMacdMainAboveMacdSignal = (_MACD_main > _MCAD_signal);
			_isMacdSignalPositive = (_MCAD_signal > 0);
			_isParabolicSARBelowMaClose = (_Parabolic_SAR < Robot.MarketSeries.Close.LastValue);

			TradeType? tradeType = null;

			if(_isMacdMainAboveMacdSignal &&
				_isMacdMainPositive &&
				_isParabolicSARBelowMaClose &&
				(Math.Abs(_MACD_main - _MCAD_signal) > (Noise_MACD_sm / _dixPowerDigits)) &&
				(Math.Abs(_MACD_main) > (Noise_MACD_m0 / _dixPowerDigits)) &&
				(Math.Abs(_Parabolic_SAR - i_EMAf.Result.Last(0)) > (Noise_Prb_SAR_ema / _dixPowerDigits)) &&
				(Math.Abs(_MCAD_signal) > (Noise_MACD_s0 / _dixPowerDigits)) &&
				_isMacdSignalPositive)
				tradeType = TradeType.Buy;
			else if(!_isMacdMainAboveMacdSignal &&
					!_isMacdMainPositive &&
					!_isParabolicSARBelowMaClose &&
					(Math.Abs(_MACD_main - _MCAD_signal) > (Noise_MACD_sm / _dixPowerDigits)) &&
					(Math.Abs(_MACD_main) > (Noise_MACD_m0 / _dixPowerDigits)) &&
					(Math.Abs((_Parabolic_SAR - i_EMAf.Result.Last(0))) > (Noise_Prb_SAR_ema / _dixPowerDigits)) &&
					(Math.Abs(_MCAD_signal) > (Noise_MACD_s0 / _dixPowerDigits)) &&
					!_isMacdSignalPositive)
				tradeType = TradeType.Sell;

			return tradeType;
		}
        public override TradeType? signal2()
        {
            throw new NotImplementedException();
        }
        public override TradeType? signal3()
        {
            throw new NotImplementedException();
        }
    }
}
