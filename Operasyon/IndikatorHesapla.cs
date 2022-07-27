using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Extension;
using Trady.Core.Infrastructure;
using static Operasyon.IndikatorHesapla;

namespace Operasyon
{
    class IndikatorHesapla
    {
        public static List<OttSonuc> OttHesapla(List<IOhlcv> tradyCandles, int maPeriyot, decimal ottYuzde)
        {
            //MAvg=getMA(src, length)
            //fark=MAvg*percent*0.01
            //longStop = MAvg - fark
            //longStopPrev = nz(longStop[1], longStop)
            //longStop := MAvg > longStopPrev ? max(longStop, longStopPrev) : longStop
            //shortStop =  MAvg + fark
            //shortStopPrev = nz(shortStop[1], shortStop)
            //shortStop := MAvg < shortStopPrev ? min(shortStop, shortStopPrev) : shortStop
            //dir = 1
            //dir := nz(dir[1], dir)
            //dir := dir == -1 and MAvg > shortStopPrev ? 1 : dir == 1 and MAvg < longStopPrev ? -1 : dir
            //MT = dir==1 ? longStop: shortStop
            //OTT=MAvg>MT ? MT*(200+percent)/200 : MT*(200-percent)/200 
            //plot(showsupport ? MAvg : na, color=#0585E1, linewidth=2, title="Support Line")
            //pALL=plot(nz(OTT[2]), color=OTTC, linewidth=2, title="OTT", transp=0)
            //buySignalk = crossover(MAvg, OTT[2])
            //sellSignallk = crossunder(MAvg, OTT[2])

            var ottList = new List<OttSonuc>();

            var maList = SmaHesapla(tradyCandles, maPeriyot);
            var longStop = new List<decimal>();
            var shortStop = new List<decimal>();
            var dir = new List<decimal>();
            var ott = new List<decimal> { 0, 0, 0, 0 };

            for (int i = 0; i < maList.Count; i++)
            {
                //MAvg=getMA(src, length)
                var MAvg = maList[i].Tick;

                //fark=MAvg*percent*0.01
                var fark = MAvg * ottYuzde * (decimal)0.01;

                //longStop = MAvg - fark
                var lStop = MAvg - fark;

                //longStopPrev = nz(longStop[1], longStop)
                var longStopPrev = longStop.Count > 0 ? longStop[i - 1] : lStop;

                //longStop := MAvg > longStopPrev ? max(longStop, longStopPrev) : longStop
                var calcLongStop = MAvg > longStopPrev ? Math.Max(lStop, longStopPrev) : lStop;
                longStop.Add(calcLongStop);

                var sStop = MAvg + fark;
                var shortStopPrev = shortStop.Count > 0 ? shortStop[i - 1] : sStop;
                var calcShortStop = MAvg < shortStopPrev ? Math.Min(sStop, shortStopPrev) : sStop;
                shortStop.Add(calcShortStop);

                var dr = dir.Count > 0 ? dir[i - 1] : 1;

                //dir := dir == -1 and MAvg > shortStopPrev ? 1 : dir == 1 and MAvg < longStopPrev ? -1 : dir
                var calcDir = dr == -1 && MAvg > shortStopPrev ? 1 : dr == 1 && MAvg < longStopPrev ? -1 : dr;
                dir.Add(calcDir);

                var MT = calcDir == 1 ? calcLongStop : calcShortStop;

                //OTT=MAvg>MT ? MT*(200+percent)/200 : MT*(200-percent)/200 
                var OTT = MAvg > MT ? MT * (200 + ottYuzde) / 200 : MT * (200 - ottYuzde) / 200;
                ott.Add(OTT);

                var durum = MAvg > ott[i + 3] ? "AL" : MAvg < ott[i + 3] ? "SAT" : ottList[i - 1].Durum;
                ottList.Add(new OttSonuc { Bar = tradyCandles.First(x => x.DateTime == maList[i].DateTime), MaValue = MAvg, Ott = ott[i + 2], Durum = durum });

                //var utc = tradyCandles.First(x => x.DateTime == maList[i].DateTime).DateTime.UtcDateTime;
                //Console.WriteLine(i + " - " + utc.ToLocalTime()
                //                  + " - OTT[2]: " + $"{ott[i + 2]:N8}"
                //                  + " - MAvg: " + $"{MAvg:N8}"
                //                  + " - Durum: " + $"{durum}");
            }

            return ottList;
        }

        public static List<AnalyzableTick<decimal>> SmaHesapla(List<IOhlcv> tradyCandles, int smaUzunluk)
        {
            var smaList = tradyCandles.Select(x => x.Close).Sma(smaUzunluk).Where(x => x != null).Select(x => Math.Round(Convert.ToDecimal(x.Value), 8)).ToList();
            var retList = new List<AnalyzableTick<decimal>>();

            for (int i = smaUzunluk; i < tradyCandles.Count; i++)
            {
                retList.Add(new AnalyzableTick<decimal>(tradyCandles[i - 1].DateTime, smaList[i - smaUzunluk]));
            }

            return retList;
        }

        public static List<AnalyzableTick<decimal>> EmaHesapla(List<IOhlcv> tradyCandles, int smaUzunluk)
        {
            var smaList = tradyCandles.Select(x => x.Close).Ema(smaUzunluk).Where(x => x != null).Select(x => Math.Round(Convert.ToDecimal(x.Value), 8)).ToList();
            var retList = new List<AnalyzableTick<decimal>>();

            for (int i = smaUzunluk; i < tradyCandles.Count; i++)
            {
                retList.Add(new AnalyzableTick<decimal>(tradyCandles[i - 1].DateTime, smaList[i - smaUzunluk]));
            }

            return retList;
        }

        public class OttSonuc
        {
            public string Durum { get; set; }
            public decimal Ott { get; set; }
            public decimal MaValue { get; set; }
            public IOhlcv Bar { get; set; }
        }
    }
}
