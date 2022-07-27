using Binance.API.Csharp.Client;
using Binance.API.Csharp.Client.Models.Enums;
using Binance.API.Csharp.Client.Models.Market;
using Binance.API.Csharp.Client.Utils;
using Operasyon.DAL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trady.Core;
using Trady.Core.Infrastructure;

namespace Operasyon
{
    class BinanceIslemleri
    {
        private static readonly AyarlarTablosu Ayarlar = DbIslemleri.AyarlariGetir();

        private static BinanceClient BinanceClientOlustur(bool ticaretKurallariAlinsinMi = false) => new BinanceClient(new ApiClient(Ayarlar.BinanceApiKey, Ayarlar.BinanceApiSecret), ticaretKurallariAlinsinMi);

        public static List<SymbolPrice> SembolleriVeFiyatlariDon(string taranacakSembol, string haricSembolListesi)
        {
            BinanceClient binanceClient = BinanceClientOlustur();
            var allSymbol = binanceClient.GetAllPrices().Result;

                var tickerPrices=allSymbol.Where(x => x.Symbol.EndsWith(taranacakSembol) && !haricSembolListesi.Contains(x.Symbol))
                .Select(i => new SymbolPrice { Symbol = i.Symbol, Price = i.Price })
                .OrderBy(x => x.Symbol).ToList();

            if (tickerPrices.Any())
            {
                return tickerPrices;
            }

            return null;
        }

        public static List<IOhlcv> UzunDonemBarGetir(string sembol, ZamanPeriyotu zamanPeriyotu)
        {
            var binanceClient = BinanceClientOlustur();
            var interval = Genel.StringdenTimeIntervalDon(zamanPeriyotu.GetDescription());

            var lst = new List<Candlestick>();
            var sonTarih = DateTime.Now;
            var pd = Genel.PeriyottanDakikaDon(zamanPeriyotu.GetDescription());

            var barSayisi = Ayarlar.BacktestBarSayisi + 500;

            var geriGit = barSayisi * pd;

            var baslangicTarihi = sonTarih.AddMinutes(-1 * geriGit);

            var zamanFarki = sonTarih - baslangicTarihi;
            var zamanFarkiDakika = zamanFarki.TotalMinutes;

            var getlimit = 1000 * pd;

            var tamBol = Convert.ToInt32(Math.Floor(zamanFarkiDakika / getlimit));
            var kalan = Convert.ToInt32(zamanFarkiDakika - (getlimit * tamBol));


            if (kalan > 0)
            {
                var candlestick = binanceClient.GetCandleSticks(sembol, interval, startTime: sonTarih.AddMinutes(kalan * (-1)), limit: getlimit).Result;
                var candlesticks = candlestick as Candlestick[] ?? candlestick.Where(x => Genel.StartUnixTime.AddMilliseconds(x.CloseTime).ToUniversalTime() < DateTime.Now.ToUniversalTime()).ToArray();

                candlesticks.Reverse();
                foreach (var item in candlestick)
                {
                    if (!lst.Any(x => x.OpenTime == item.OpenTime))
                    {
                        lst.Add(item);
                    }
                }
            }

            for (int i = 1; i < tamBol + 1; i++)
            {
                var candlestick = binanceClient.GetCandleSticks(sembol, interval, startTime: sonTarih.AddMinutes((getlimit * (-1) * i) - kalan), limit: getlimit).Result;
                var candlesticks = candlestick as Candlestick[] ?? candlestick.Where(x => Genel.StartUnixTime.AddMilliseconds(x.CloseTime).ToUniversalTime() < DateTime.Now.ToUniversalTime()).ToArray();

                candlesticks.Reverse();
                foreach (var item in candlestick)
                {
                    if (lst.All(x => x.OpenTime != item.OpenTime))
                    {
                        lst.Add(item);
                    }
                }
            }

            lst.Reverse();

            var trimList = lst.Where(x => Genel.StartUnixTime.AddMilliseconds(x.OpenTime).ToUniversalTime() >= baslangicTarihi.ToUniversalTime() && Genel.StartUnixTime.AddMilliseconds(x.OpenTime).ToUniversalTime() <= sonTarih.ToUniversalTime()).OrderBy(x => x.OpenTime).ToList();

            var tradyCandles = trimList.Select(candle => new Candle(Genel.StartUnixTime.AddMilliseconds(candle.OpenTime).ToUniversalTime(), candle.Open, candle.High, candle.Low, candle.Close, candle.Volume)).Cast<IOhlcv>().ToList();

            return tradyCandles;
        }


        public static EmirSonuc AlisYap(string sembol)
        {
            var sonuc = new EmirSonuc { BasariliMi = false };
            try
            {
                BinanceClient binanceClient = BinanceClientOlustur(true);

                var accountInfo = binanceClient.GetAccountInfo().Result;
                var kapitalBakiye = accountInfo.Balances.First(x => x.Asset == "USDT").Free;
                var uyeKomisyonOran = 0;// decimal.Parse(((double)accountInfo.TakerCommission / (double)1000).ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

                if (kapitalBakiye > Ayarlar.EmirBoyutuUSDT)
                {
                    var sembolFiyati = binanceClient.GetAllPrices().Result
                    .Where(x => x.Symbol.EndsWith(sembol))
                    .Select(i => new SymbolPrice { Symbol = i.Symbol, Price = i.Price })
                    .OrderBy(x => x.Symbol).ToList();

                    var emirBoyutundaSembolAdeti = Ayarlar.EmirBoyutuUSDT / sembolFiyati.FirstOrDefault().Price;
                    var stepSize = binanceClient._tradingRules.Symbols.First(x => x.SymbolName == sembol).Filters.First(y => y.FilterType == "LOT_SIZE").StepSize;
                    var stepIndex = stepSize.ToString(CultureInfo.InvariantCulture).Split('.')[1].IndexOf('1') + 1;

                    var komisyonMiktar = emirBoyutundaSembolAdeti * uyeKomisyonOran;
                    var alinabilirMiktar = emirBoyutundaSembolAdeti - komisyonMiktar;

                    var alinabilirMiktarString = alinabilirMiktar.ToString(CultureInfo.InvariantCulture).Split('.');
                    var islemeGirilecekMiktar = Convert.ToDecimal(alinabilirMiktarString[0] + "," + alinabilirMiktarString[1].Substring(0, stepIndex));

                    var buyMinNotional = binanceClient._tradingRules.Symbols.First(x => x.SymbolName == sembol).Filters.First(y => y.FilterType == "MIN_NOTIONAL").MinNotional;

                    if (Ayarlar.EmirBoyutuUSDT > buyMinNotional)
                    {
                        var buyOrder = binanceClient.PostNewOrder(sembol, islemeGirilecekMiktar, 0, OrderSide.BUY, OrderType.MARKET, recvWindow: 60000).Result;

                        sonuc = new EmirSonuc
                        {
                            BasariliMi = true,
                            TransactTime = buyOrder.TransactTime
                        };
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("<b>Verilen emir boyutuna göre min alım miktarını geçemediği için işleme girilemedi (" +
                                  sembol + ")</b>\n");

                        TelegramTanimlari.MesajGonder(sb.ToString(), TaramaTip.Tarayici);
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<b>Bakiye yetersiz olduğu için Gerçek işleme girilemedi, Sanal olarak işlem takip edilecek (" +
                              sembol + ")</b>\n");

                    TelegramTanimlari.MesajGonder(sb.ToString(), TaramaTip.Tarayici);
                }
            }
            catch (Exception ex)
            {
                TelegramTanimlari.MesajGonder(sembol + " Alım yaparken hata oluştu: " + ex.InnerException.Message, TaramaTip.Tarayici);
            }

            return sonuc;
        }

        public static EmirSonuc SatisYap(string sembol, decimal alinmisAdet)
        {
            var sonuc = new EmirSonuc { BasariliMi = false, ZorlaKapat = false };

            try
            {
                BinanceClient binanceClient = BinanceClientOlustur(true);
                var accountInfo = binanceClient.GetAccountInfo().Result;
                var sembolBakiye = accountInfo.Balances.Where(x => x.Asset == sembol.Replace("USDT", "")).First().Free;

                var stepSize = binanceClient._tradingRules.Symbols.Where(x => x.SymbolName == sembol).First().Filters.Where(y => y.FilterType == "LOT_SIZE").First().StepSize;
                var stepIndex = stepSize.ToString(CultureInfo.InvariantCulture).Split('.')[1].IndexOf('1') + 1;

                if (sembolBakiye > alinmisAdet)
                {
                    sembolBakiye = alinmisAdet;
                }

                var satilabilirMiktarString = sembolBakiye.ToString(CultureInfo.InvariantCulture).Split('.');
                var islemeGirilecekMiktar = Convert.ToDecimal(satilabilirMiktarString[0] + "," + satilabilirMiktarString[1].Substring(0, stepIndex));

                try
                {
                    var sellOrder = binanceClient.PostNewOrder(sembol, islemeGirilecekMiktar, 0, OrderSide.SELL, OrderType.MARKET, recvWindow: 60000).Result;
                    sonuc = new EmirSonuc
                    {
                        BasariliMi = true,
                        TransactTime = sellOrder.TransactTime,
                        ZorlaKapat = false
                    };
                }
                catch (Exception e)
                {
                    if (e.InnerException != null && e.InnerException.Message.Contains("1013"))
                    {
                        TelegramTanimlari.MesajGonder(sembol + " satışında borsa robot satışını engelledi, lüten pozisyonu borsada elle kapatın, robot bu işlemin takibini kapattı.", TaramaTip.Tarayici);
                        sonuc = new EmirSonuc { BasariliMi = true, ZorlaKapat = true };
                    }
                    else if (e.InnerException != null && e.InnerException.Message.Contains("zero"))
                    {
                        TelegramTanimlari.MesajGonder(sembol + " için borsada bakiye görünmüyor, robot bu işlemin takibini kapattı. İşlemi kendiniz takip etmelisiniz ya da elle kapatmalısınız.", TaramaTip.Tarayici);
                        sonuc = new EmirSonuc { BasariliMi = true, ZorlaKapat = true };
                    }
                    else
                    {
                        if (e.InnerException != null)
                            TelegramTanimlari.MesajGonder(sembol + " Satış yaparken hata oluştu: " + e.InnerException.Message, TaramaTip.Tarayici);
                        else
                            TelegramTanimlari.MesajGonder(sembol + " Satış yaparken hata oluştu: " + e.Message, TaramaTip.Tarayici);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    TelegramTanimlari.MesajGonder(sembol + " Satış yaparken hata oluştu: " + ex.InnerException.Message, TaramaTip.Tarayici);
                else
                    TelegramTanimlari.MesajGonder(sembol + " Satış yaparken hata oluştu: " + ex.Message, TaramaTip.Tarayici);
            }

            return sonuc;
        }

        public static TicaretBilgisi TicaretFiyatiDon(string sembol, long zamanDamgasi)
        {
            var binanceClient = BinanceClientOlustur();
            var getTrades = binanceClient.GetTradeList(sembol, 20000);
            var getTradesResult = getTrades.Result;

            var ticaretBilgisi = new TicaretBilgisi { Fiyat = -1, Adet = 0 };

            var ticaretList = getTradesResult.Where(x => x.Time == zamanDamgasi);

            if (ticaretList.Any())
            {
                decimal toplamMaliyet = 0;
                decimal toplamAdet = 0;
                foreach (var ticaret in ticaretList)
                {
                    toplamMaliyet += ticaret.Price * ticaret.Quantity;
                    toplamAdet += ticaret.Quantity;
                }

                if (toplamAdet > 0)
                {
                    var agirlikliOrtalama = toplamMaliyet / toplamAdet;
                    ticaretBilgisi = new TicaretBilgisi { Adet = toplamAdet, Fiyat = Math.Round(agirlikliOrtalama, 8) };
                }
            }

            return ticaretBilgisi;
        }
    }
}
