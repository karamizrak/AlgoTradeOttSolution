using Binance.API.Csharp.Client.Utils;
using Operasyon.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Operasyon
{
    public class StratejiIslemleri
    {
        public static readonly AyarlarTablosu _ayarlar = DbIslemleri.AyarlariGetir();

        private List<string> taramaBilgiList;
        private List<string> taramaHataList;
        private List<string> stratejiyeTakilanlarList;
        private List<StratejiyeTakilanlar> stratejiyeTakilanlarTabloList;

        public StratejiIslemleri()
        {
            taramaBilgiList = new List<string>();
            taramaHataList = new List<string>();
            stratejiyeTakilanlarList = new List<string>();
            stratejiyeTakilanlarTabloList = new List<StratejiyeTakilanlar>();
        }

        public void TarayiciTaramaYap()
        {
            TelegramTanimlari.MesajGonder("Tarama başladı...", TaramaTip.Tarayici);

            var semboller = BinanceIslemleri.SembolleriVeFiyatlariDon("USDT", _ayarlar.HaricSemboller).Where(x => !x.Symbol.EndsWith("DOWNUSDT") && !x.Symbol.EndsWith("UPUSDT")).ToList();
            var stratejiParametreleri = DbIslemleri.StratejiParametreleriGetir();
            var parametreDic = stratejiParametreleri.ToDictionary(x => x.ParametreAdi, Genel.ParametreListesiDonList);
            var parallelCount = 30;

            foreach (var sembol in semboller)
            {
                DbIslemleri.Truncate_temp_RehberTablo();

                Console.Clear();
                try
                {
                    Parallel.ForEach(_ayarlar.IslemPeriyotAralik.Split(',').ToList(),
                        new ParallelOptions() { MaxDegreeOfParallelism = parallelCount },
                        _islemPeriyot =>
                        {
                            var barlar = BinanceIslemleri.UzunDonemBarGetir(sembol.Symbol, Genel.StringdenZamanPeriyotuDon(_islemPeriyot));

                            Parallel.ForEach(parametreDic["Period"], new ParallelOptions { MaxDegreeOfParallelism = parallelCount }, _period =>
                            {
                                Parallel.ForEach(parametreDic["Yuzde"], new ParallelOptions { MaxDegreeOfParallelism = parallelCount }, _yuzde =>
                                {
                                    object[] paramObjects = { _period, _yuzde };
                                    var stratejiSonucListe = TarayiciHesapla(sembol.Symbol, Genel.StringdenZamanPeriyotuDon(_islemPeriyot), barlar, paramObjects);

                                    if (stratejiSonucListe != null && stratejiSonucListe.Count > 0)
                                    {
                                        var ozet = OzetHesapla(stratejiSonucListe, stratejiSonucListe.Count);
                                        //Console.WriteLine(ozet.Sembol + " - " + ozet.Periyot + " - " + ozet.Parametreler + " KAR ORAN:" + $"{ozet.NetKar:N2}" + " BAR SAYISI:" + ozet.BarSayisi + " İŞLEM SAYISI:" + ozet.IslemSayisi + " BAŞARILI İŞLEM SAYISI:" + ozet.BasariliIslemSayisi);
                                        //if (ozet.NetKar > 60)
                                        //    TelegramTanimlari.MesajGonder($"{ozet.Sembol} - {ozet.Periyot} - {ozet.Parametreler} KAR ORAN: {ozet.NetKar:N2} BAR SAYISI: {ozet.BarSayisi}  İŞLEM SAYISI: {ozet.IslemSayisi} BAŞARILI İŞLEM SAYISI: {ozet.BasariliIslemSayisi}", TaramaTip.Avci);

                                        DbIslemleri.Insert_temp_RehberTablo(ozet);
                                    }
                                });
                            });
                        });

                }
                catch (Exception e)
                {
                    taramaHataList.Add("Tarayici Hatasi: " + sembol.Symbol + " " + _ayarlar.IslemPeriyot + " " + "(" + e.Message + ")");
                }

                DbIslemleri.TaramaSonucKaydet(sembol.Symbol, _ayarlar.BacktestEsikKarOrani);

            }

            Rapor.TarayiciRaporla(taramaBilgiList, taramaHataList);

            TelegramTanimlari.MesajGonder("Tarama Bitti...", TaramaTip.Tarayici);
        }

        public void AvciTaramaYap()
        {
            DbIslemleri.Truncate_temp_AvciSonuclari();
            DbIslemleri.Truncate_StratejiyeTakilanlar();
            TelegramTanimlari.MesajGonder("Avcı tarama yapıyor", TaramaTip.Avci);

            var rehberTabloListesi = DbIslemleri.RehberTabloGetir();

            Parallel.ForEach(rehberTabloListesi, new ParallelOptions { MaxDegreeOfParallelism = 30 }, rehberSatir =>
            {
                var barlar = BinanceIslemleri.UzunDonemBarGetir(rehberSatir.Sembol.Symbol, rehberSatir.Periyot);
                object[] paramObjects = {
                            Convert.ToDecimal(rehberSatir.Parametreler.Split('|')[0]),
                            Convert.ToDecimal(rehberSatir.Parametreler.Split('|')[1]) };

                var stratejiSonucListe = AvciHesapla(rehberSatir.Sembol.Symbol, rehberSatir.Periyot, barlar, paramObjects);

                if (stratejiSonucListe != null && stratejiSonucListe.Count > 0)
                {
                    if (stratejiSonucListe.Last().Durum == "AL")
                    {
                        var sembolunSonKapanisindanGecenSure = Genel.SembolunSonKapanisindanGecenDakika(rehberSatir.Sembol.Symbol);

                        var sonAlSinyalKaydi = SonSinyalDegisimiDon(stratejiSonucListe);
                        var ozet = OzetHesapla(stratejiSonucListe, stratejiSonucListe.Count);

                        ozet.SonDurum = "AL";
                        ozet.AlBarAcilisTarihi = sonAlSinyalKaydi.AcilisZamani;
                        ozet.GecenBarSayisi = stratejiSonucListe.Last().IslemIndex - sonAlSinyalKaydi.IslemIndex;
                        ozet.AlSinyalFiyat = sonAlSinyalKaydi.Fiyat;
                        ozet.MevcutFiyat = rehberSatir.Sembol.Price;
                        ozet.AlVeMevcutFarkOran = (rehberSatir.Sembol.Price - sonAlSinyalKaydi.Fiyat) / sonAlSinyalKaydi.Fiyat * 100;

                        if (StratejiAlOnay(ozet, sembolunSonKapanisindanGecenSure))
                        {
                            //Console.WriteLine(ozet.Sembol + " - " + ozet.Periyot + " - " + ozet.Parametreler + " işlemi açılacak!");
                            TelegramTanimlari.MesajGonder($"Strateji Onay Aldi:{ozet.Sembol} - {ozet.Periyot} - {ozet.Parametreler} işlemi açılacak!", TaramaTip.Avci);
                            DbIslemleri.Insert_temp_AvciSonuclari(ozet);
                        }
                        else
                        {
                            //Console.WriteLine(ozet.Sembol + " - " + ozet.Periyot + " - " + ozet.Parametreler + " strateji onayı alamadı");
                            //TelegramTanimlari.MesajGonder($"Strateji :{ozet.Sembol} - {ozet.Periyot} - {ozet.Parametreler} strateji onayı alamadı!", TaramaTip.Avci);
                        }
                    }
                    else
                    {
                        //Console.WriteLine(rehberSatir.Sembol.Symbol + " - " + rehberSatir.Periyot + " henüz SAT durumunda)");
                        //TelegramTanimlari.MesajGonder($"Strateji :{rehberSatir.Sembol.Symbol} - {rehberSatir.Periyot}  henüz SAT durumunda", TaramaTip.Avci);
                    }
                }
            });

            DbIslemleri.AvciKaydet();
            var acilisKayitlar = DbIslemleri.IslemAcilisKaydet();
            Rapor.AvciRaporla(acilisKayitlar, stratejiyeTakilanlarList);
        }

        public void TuccarTaramaYap()
        {
            var ds = DbIslemleri.AcikIslemleriGetir();
            if (ds.Count > 0)
            {
                TelegramTanimlari.MesajGonder($" {ds.Count.ToString()} adet açık işlem var", TaramaTip.Tuccar);
                var kapananlarIdListe = new List<IslemTablosu>();
                var tickerPrices = BinanceIslemleri.SembolleriVeFiyatlariDon("USDT", "");

                Parallel.ForEach(ds, new ParallelOptions { MaxDegreeOfParallelism = 30 }, islemRow =>
                {
                    var kapanmaSebebi = "-";
                    var coin = tickerPrices.Where(x => x.Symbol == islemRow.Sembol.ToString()).ToList().First();
                    var coinAdeti = islemRow.CoinAdeti;
                    var kontrolPariteKarsiligi = coin.Price * coinAdeti;
                    var karPariteKarsiligi = kontrolPariteKarsiligi - islemRow.GirisPariteKarsiligi;
                    var karOran = karPariteKarsiligi / islemRow.GirisPariteKarsiligi * 100;

                    var pikOrani = islemRow.PikOrani;
                    var pikFiyat = islemRow.PikFiyat;
                    var pikTarihi = islemRow.PikTarihi;

                    if (karOran > pikOrani)
                    {
                        pikOrani = karOran;
                        pikFiyat = coin.Price;
                        pikTarihi = DateTime.Now;
                    }

                    DbIslemleri.Insert_FiyatKontrolTablosu(islemRow.Id, Convert.ToDecimal(coin.Price), karOran);

                    var islemDurum = 0;

                    if (karOran < 0 - _ayarlar.TakipEdenDurdurOran)
                    {
                        islemDurum = 1;
                        //Console.WriteLine(coin.Symbol + "manuel zarar durdur oranına düştü ve satışa geçiyor...");
                        TelegramTanimlari.MesajGonder(coin.Symbol + " manuel zarar durdur oranına düştü ve satışa geçiyor...", TaramaTip.Tuccar);
                        kapanmaSebebi = "ZARAR DURDUR";
                    }
                    else if (karOran > _ayarlar.KarAlOran)
                    {
                        islemDurum = 1;
                        TelegramTanimlari.MesajGonder(coin.Symbol + " max kar al seviyesini kırdı ve satışa geçiyor, tebrikler...", TaramaTip.Tuccar);
                        //Console.WriteLine(coin.Symbol + " max kar al seviyesini kırdı ve satışa geçiyor...");
                        TelegramTanimlari.MesajGonder(coin.Symbol + " max kar al seviyesini kırdı ve satışa geçiyor...", TaramaTip.Tuccar);
                        kapanmaSebebi = "KAR AL";
                    }
                    else
                    {
                        if ((pikOrani - karOran) > _ayarlar.TakipEdenDurdurOran)
                        {
                            islemDurum = 1;
                            TelegramTanimlari.MesajGonder(coin.Symbol + " takip eden kar al seviyesini kırdı ve satışa geçiyor, tebrikler...", TaramaTip.Tuccar);
                            //Console.WriteLine(coin.Symbol + " takip eden kar al seviyesini kırdı ve satışa geçiyor...");
                            TelegramTanimlari.MesajGonder(coin.Symbol + " takip eden kar al seviyesini kırdı ve satışa geçiyor...", TaramaTip.Tuccar);
                            kapanmaSebebi = "TAKİP EDEN KAR AL";
                        }

                        if (islemDurum != 1)
                        {
                            var barlar = BinanceIslemleri.UzunDonemBarGetir(islemRow.Sembol, Genel.StringdenZamanPeriyotuDon(islemRow.Periyot));
                            var ssh = new List<BardakiStratejiSonucu>();
                            object[] paramObjects = {
                                    Convert.ToDecimal(islemRow.Parametreler.Split('|')[0]),
                                    Convert.ToDecimal(islemRow.Parametreler.Split('|')[1])};

                            ssh = StratejiSonucHesapla(islemRow.Sembol, barlar, paramObjects);


                            if (ssh == null)
                            {
                                islemDurum = 1;
                                kapanmaSebebi = "SSH NULL";
                            }
                            else
                            {
                                islemDurum = ssh.Last().IslemDurum == "AL" ? 0 : 1;
                                if (islemDurum == 1)
                                {
                                    //Console.WriteLine(coin.Symbol + " strateji değerine göre satışa geçiyor...");
                                    TelegramTanimlari.MesajGonder(coin.Symbol + " strateji değerine göre satışa geçiyor...", TaramaTip.Tuccar);

                                    kapanmaSebebi = "STRATEJİ SAT SİNYALİ";
                                }
                            }
                        }
                    }

                    if (islemDurum == 1)
                    {
                        islemRow.KapanmaSebebi = kapanmaSebebi;
                        kapananlarIdListe.Add(islemRow);

                        var fiyat = Convert.ToDecimal(coin.Price);
                        var adet = coinAdeti;
                        var fiyatBilgi = "Bar Fiyatı";

                        if (islemRow.IslemTipi == (int)TicaretTipi.Gercek)
                        {
                            var ticaret = BinanceIslemleri.SatisYap(islemRow.Sembol.ToString(), coinAdeti);
                            if (ticaret.BasariliMi)
                            {
                                if (!ticaret.ZorlaKapat)
                                {
                                    var ticaretFiyat = BinanceIslemleri.TicaretFiyatiDon(coin.Symbol, ticaret.TransactTime);

                                    fiyat = ticaretFiyat.Fiyat > -1 ? ticaretFiyat.Fiyat : Convert.ToDecimal(coin.Price);
                                    adet = ticaretFiyat.Fiyat > -1 ? ticaretFiyat.Adet : coinAdeti;
                                    fiyatBilgi = ticaretFiyat.Fiyat > -1 ? "Ticaret Fiyatı" : "Bar Fiyatı";

                                    TelegramTanimlari.MesajGonder(coin.Symbol + " SATIŞ fiyat: " + fiyat + " | fiyat tip: " + fiyatBilgi + " | satış adet: " + adet + " | zaman damgası: " + ticaret.TransactTime, TaramaTip.Tarayici);
                                }
                                else
                                {
                                    DbIslemleri.IslemBilgileriGuncelle(islemRow.Id, Convert.ToDecimal(coin.Price), coinAdeti, kontrolPariteKarsiligi, Convert.ToDecimal(0), Convert.ToDecimal(0), Convert.ToDecimal(pikFiyat), Convert.ToDecimal(pikOrani), Convert.ToDateTime(pikTarihi), islemDurum, "ZORLA KAPANDI");
                                }
                            }
                        }

                        kontrolPariteKarsiligi = fiyat * coinAdeti;
                        karPariteKarsiligi = kontrolPariteKarsiligi - islemRow.GirisPariteKarsiligi;
                        karOran = karPariteKarsiligi / islemRow.GirisPariteKarsiligi * 100;

                        DbIslemleri.IslemBilgileriGuncelle(islemRow.Id, fiyat, adet, kontrolPariteKarsiligi, karPariteKarsiligi, karOran, Convert.ToDecimal(pikFiyat), Convert.ToDecimal(pikOrani), Convert.ToDateTime(pikTarihi), islemDurum, kapanmaSebebi);
                    }
                    else
                    {
                        //Console.WriteLine(coin.Symbol + " devam ediyor...");
                        TelegramTanimlari.MesajGonder($"{coin.Symbol} devam ediyor...", TaramaTip.Tuccar);
                        DbIslemleri.IslemBilgileriGuncelle(islemRow.Id, Convert.ToDecimal(coin.Price), coinAdeti, kontrolPariteKarsiligi, karPariteKarsiligi, karOran, Convert.ToDecimal(pikFiyat), Convert.ToDecimal(pikOrani), Convert.ToDateTime(pikTarihi), islemDurum, "-");
                    }
                });

                if (kapananlarIdListe.Count > 0)
                {
                    Rapor.TuccarRaporla(kapananlarIdListe);
                }
            }
            //else
            //{
            //    //TelegramTanimlari.MesajGonder($"Açık işlem yok", TaramaTip.Tuccar);
            //    //Console.WriteLine("Açık işlem yok");
            //}
        }

        public void PanikCalistir()
        {
            var ds = DbIslemleri.AcikIslemleriGetir();
            if (ds.Count > 0)
            {
                var kapananlarIdListe = new List<IslemTablosu>();
                var tickerPrices = BinanceIslemleri.SembolleriVeFiyatlariDon("USDT", "");

                Parallel.ForEach(ds, new ParallelOptions { MaxDegreeOfParallelism = 30 }, islemRow =>
                {
                    var kapanmaSebebi = "PANİK SATIŞI";
                    var coin = tickerPrices.Where(x => x.Symbol == islemRow.Sembol.ToString()).ToList().First();
                    var coinAdeti = islemRow.CoinAdeti;
                    var kontrolPariteKarsiligi = coin.Price * coinAdeti;
                    var karPariteKarsiligi = kontrolPariteKarsiligi - islemRow.GirisPariteKarsiligi;
                    var karOran = karPariteKarsiligi / islemRow.GirisPariteKarsiligi * 100;

                    var pikOrani = islemRow.PikOrani;
                    var pikFiyat = islemRow.PikFiyat;
                    var pikTarihi = islemRow.PikTarihi;

                    if (karOran > pikOrani)
                    {
                        pikOrani = karOran;
                        pikFiyat = coin.Price;
                        pikTarihi = DateTime.Now;
                    }

                    DbIslemleri.Insert_FiyatKontrolTablosu(islemRow.Id, Convert.ToDecimal(coin.Price), karOran);

                    kapananlarIdListe.Add(islemRow);

                    var fiyat = Convert.ToDecimal(coin.Price);
                    var adet = coinAdeti;
                    var fiyatBilgi = "Bar Fiyatı";

                    if (islemRow.IslemTipi == (int)TicaretTipi.Gercek)
                    {
                        var ticaret = BinanceIslemleri.SatisYap(islemRow.Sembol.ToString(), coinAdeti);
                        if (ticaret.BasariliMi)
                        {
                            if (!ticaret.ZorlaKapat)
                            {
                                var ticaretFiyat = BinanceIslemleri.TicaretFiyatiDon(coin.Symbol, ticaret.TransactTime);

                                fiyat = ticaretFiyat.Fiyat > -1 ? ticaretFiyat.Fiyat : Convert.ToDecimal(coin.Price);
                                adet = ticaretFiyat.Fiyat > -1 ? ticaretFiyat.Adet : coinAdeti;
                                fiyatBilgi = ticaretFiyat.Fiyat > -1 ? "Ticaret Fiyatı" : "Bar Fiyatı";

                                TelegramTanimlari.MesajGonder(coin.Symbol + " SATIŞ fiyat: " + fiyat + " | fiyat tip: " + fiyatBilgi + " | satış adet: " + adet + " | zaman damgası: " + ticaret.TransactTime, TaramaTip.Tarayici);
                            }
                            else
                            {
                                DbIslemleri.IslemBilgileriGuncelle(islemRow.Id, Convert.ToDecimal(coin.Price), coinAdeti, kontrolPariteKarsiligi, Convert.ToDecimal(0), Convert.ToDecimal(0), Convert.ToDecimal(pikFiyat), Convert.ToDecimal(pikOrani), Convert.ToDateTime(pikTarihi), 1, kapanmaSebebi);
                            }
                        }
                    }

                    kontrolPariteKarsiligi = fiyat * coinAdeti;
                    karPariteKarsiligi = kontrolPariteKarsiligi - islemRow.GirisPariteKarsiligi;
                    karOran = karPariteKarsiligi / islemRow.GirisPariteKarsiligi * 100;

                    DbIslemleri.IslemBilgileriGuncelle(islemRow.Id, fiyat, adet, kontrolPariteKarsiligi, karPariteKarsiligi, karOran, Convert.ToDecimal(pikFiyat), Convert.ToDecimal(pikOrani), Convert.ToDateTime(pikTarihi), 1, kapanmaSebebi);
                });
            }
            else
            {
                TelegramTanimlari.MesajGonder("Açık pozisyon yok", TaramaTip.Tarayici);
            }
        }

        public void ZRaporuAl()
        {
            Rapor.ZRaporu();
        }

        public List<IslemRaporModel> TarayiciHesapla(string coin, ZamanPeriyotu periyot, List<IOhlcv> kandilListe, params object[] parametreler)
        {
            var islemListe = new List<IslemRaporModel>();
            var ssh = StratejiSonucHesapla(coin, kandilListe, parametreler);
            if (ssh == null || !ssh.Any()) return islemListe;

            var stratejiSonucListe = StratejiIlkAlSinyalineKadarSatYap(ssh);

            var stratejiyeGoreIslemSay = 0;
            var stratejiyeGorePozisyon = "SAT";
            var durumAciklama = "SAT";
            decimal posizyonEnYuksekOran = 0;
            decimal sonAlFiyat = stratejiSonucListe.First().Bar.Open;

            for (int i = 1; i < stratejiSonucListe.Count; i++)
            {
                var kapanmisBar = stratejiSonucListe[i - 1];
                var mevcutBar = stratejiSonucListe[i];

                if (kapanmisBar.IslemDurum == "AL")
                {
                    if (stratejiyeGorePozisyon == "SAT" && islemListe.Last().DurumAciklama == "SAT" && kapanmisBar.KesisimBuradaMi)
                    {
                        stratejiyeGorePozisyon = "AL";
                        durumAciklama = "AL";
                        sonAlFiyat = mevcutBar.Bar.Open;
                        stratejiyeGoreIslemSay++;
                    }
                    else
                    {
                        var karOraniEnDusuk = !kapanmisBar.KesisimBuradaMi ? (kapanmisBar.Bar.Low - sonAlFiyat) / sonAlFiyat * 100 : 0;
                        var karOraniEnYuksek = !kapanmisBar.KesisimBuradaMi ? (kapanmisBar.Bar.High - sonAlFiyat) / sonAlFiyat * 100 : 0;
                        posizyonEnYuksekOran = posizyonEnYuksekOran > karOraniEnYuksek ? posizyonEnYuksekOran : karOraniEnYuksek;

                        //manuel stop kontrol
                        if (karOraniEnDusuk < 0 - _ayarlar.TakipEdenDurdurOran)
                        {
                            posizyonEnYuksekOran = 0;
                            stratejiyeGorePozisyon = "SAT";
                            durumAciklama = "STOP";
                        }

                        //manuel tp kontrol
                        if (karOraniEnYuksek > _ayarlar.KarAlOran)
                        {
                            posizyonEnYuksekOran = 0;
                            stratejiyeGorePozisyon = "SAT";
                            durumAciklama = "TAKE-PROFIT";
                        }

                        //trailing stop kontrol
                        if (posizyonEnYuksekOran - karOraniEnYuksek > _ayarlar.TakipEdenDurdurOran)
                        {
                            posizyonEnYuksekOran = 0;
                            stratejiyeGorePozisyon = "SAT";
                            durumAciklama = "TRAILING-STOP";
                        }
                    }
                }
                else
                {
                    stratejiyeGorePozisyon = "SAT";
                    durumAciklama = "SAT";
                }

                islemListe.Add(new IslemRaporModel
                {
                    IslemIndex = i - 1,
                    AcilisZamani = mevcutBar.Bar.DateTime.UtcDateTime.ToLocalTime(),
                    Fiyat = mevcutBar.Bar.Open,
                    Bar = mevcutBar.Bar,
                    Durum = stratejiyeGorePozisyon,
                    DurumAciklama = durumAciklama,
                    IslemSayisi = stratejiyeGoreIslemSay
                });

                //Console.WriteLine(i.ToString() + "-" +
                //                  " Zaman:" + mevcutBar.Bar.DateTime.UtcDateTime.ToLocalTime() +
                //                  " - Fiyat:" + $"{mevcutBar.Bar.Open:N8}" +
                //                  " - Durum:" + stratejiyeGorePozisyon + " - DurumAck:" + durumAciklama + " - Alım Sayısı:" + stratejiyeGoreIslemSay);
            }

            islemListe.First().Sembol = coin;
            islemListe.First().Parametreler = string.Join("|", parametreler);
            islemListe.First().Periyot = periyot.GetDescription();

            if (islemListe.Count > 0)
            {
                if (islemListe.Last().Durum != "SAT")
                {
                    islemListe.Add(new IslemRaporModel
                    {
                        AcilisZamani = stratejiSonucListe.Last().Bar.DateTime.UtcDateTime.ToLocalTime(),
                        Durum = "SAT",
                        DurumAciklama = "SON-BAR-SAT",
                        Fiyat = stratejiSonucListe.Last().Bar.Open,
                        IslemSayisi = stratejiyeGoreIslemSay,
                        Bar = stratejiSonucListe.Last().Bar
                    });
                }
            }

            return islemListe;
        }

        public List<IslemRaporModel> AvciHesapla(string coin, ZamanPeriyotu periyot, List<IOhlcv> kandilListe, params object[] parametreler)
        {
            var islemListe = new List<IslemRaporModel>();
            var ssh = StratejiSonucHesapla(coin, kandilListe, parametreler);
            if (ssh == null || !ssh.Any()) return islemListe;

            var stratejiSonucListe = StratejiIlkAlSinyalineKadarSatYap(ssh);

            var sgIslemSay = 0;
            var sgPozisyon = "SAT";
            var durumAciklama = "SAT";
            decimal posHiRatio = 0;
            decimal sonAlFiyat = stratejiSonucListe.First().Bar.Open;

            for (int i = 1; i < stratejiSonucListe.Count; i++)
            {
                //var kapanmisBar = stratejiSonucListe[i - 1];
                var mevcutBar = stratejiSonucListe[i];

                //son islem al ise diye rev olabilir
                if (mevcutBar.IslemDurum == "AL")
                {
                    var islemListeOnay = islemListe.Count > 0 ? islemListe.Last().DurumAciklama == "SAT" : true;
                    if (sgPozisyon == "SAT" && islemListeOnay && mevcutBar.KesisimBuradaMi)
                    {
                        sgPozisyon = "AL";
                        durumAciklama = "AL";
                        sonAlFiyat = mevcutBar.Bar.Open;
                        sgIslemSay++;
                    }
                    else
                    {
                        var karOraniLo = !mevcutBar.KesisimBuradaMi ? (mevcutBar.Bar.Low - sonAlFiyat) / sonAlFiyat * 100 : 0;
                        var karOraniHi = !mevcutBar.KesisimBuradaMi ? (mevcutBar.Bar.High - sonAlFiyat) / sonAlFiyat * 100 : 0;
                        posHiRatio = posHiRatio > karOraniHi ? posHiRatio : karOraniHi;

                        //manuel stop kontrol
                        if (karOraniLo < 0 - _ayarlar.TakipEdenDurdurOran)
                        {
                            posHiRatio = 0;
                            sgPozisyon = "SAT";
                            durumAciklama = "STOP";
                        }

                        //manuel tp kontrol
                        if (karOraniHi > _ayarlar.KarAlOran)
                        {
                            posHiRatio = 0;
                            sgPozisyon = "SAT";
                            durumAciklama = "TAKE-PROFIT";
                        }

                        //trailing stop kontrol
                        if (posHiRatio - karOraniHi > _ayarlar.TakipEdenDurdurOran)
                        {
                            posHiRatio = 0;
                            sgPozisyon = "SAT";
                            durumAciklama = "TRAILING-STOP";
                        }
                    }
                }
                else
                {
                    sgPozisyon = "SAT";
                    durumAciklama = "SAT";
                }

                islemListe.Add(new IslemRaporModel
                {
                    IslemIndex = i - 1,
                    AcilisZamani = mevcutBar.Bar.DateTime.UtcDateTime.ToLocalTime(),
                    Fiyat = mevcutBar.Bar.Open,
                    Bar = mevcutBar.Bar,
                    Durum = sgPozisyon,
                    DurumAciklama = durumAciklama,
                    IslemSayisi = sgIslemSay
                });

                //Console.WriteLine(i.ToString() + "-" +
                //                  " Zaman:" + mevcutBar.Bar.DateTime.UtcDateTime.ToLocalTime() +
                //                  " - Fiyat:" + $"{mevcutBar.Bar.Open:N8}" +
                //                  " - Durum:" + sgPozisyon + " - DurumAck:" + durumAciklama + " - Alım Sayısı:" + sgIslemSay);
            }

            islemListe.First().Sembol = coin;
            islemListe.First().Parametreler = string.Join("|", parametreler);
            islemListe.First().Periyot = periyot.GetDescription();

            return islemListe;
        }

        public List<BardakiStratejiSonucu> StratejiSonucHesapla(string coin, List<IOhlcv> kandilListe, params object[] parametreler)
        {
            var sonucList = new List<BardakiStratejiSonucu>();

            //ma periyot | ott yuzde 
            //8|0.8

            var ottList = IndikatorHesapla.OttHesapla(
                kandilListe,
                Convert.ToInt32(Convert.ToDecimal(parametreler[0])),
                Convert.ToDecimal(parametreler[1]));


            if (ottList == null)
                return null;

            var cutOtt = ottList.Count > _ayarlar.BacktestBarSayisi ? ottList.GetRange(ottList.Count - _ayarlar.BacktestBarSayisi, _ayarlar.BacktestBarSayisi) : new List<IndikatorHesapla.OttSonuc>();

            foreach (var p in cutOtt)
            {
                var islemDurum = p.Durum;

                sonucList.Add(new BardakiStratejiSonucu
                {
                    Bar = p.Bar,
                    Parametreler = parametreler,
                    IslemDurum = islemDurum,
                    KesisimBuradaMi = sonucList.Count > 0 && sonucList.Last().IslemDurum != islemDurum,
                    GarantiAldiMi = sonucList.Count > 0 && sonucList.Last().IslemDurum == islemDurum
                });

                //Console.WriteLine("Zaman:" + p.Bar.DateTime.ToLocalTime() +
                //                  " - Kesisim: " + sonucList.Last().KesisimBuradaMi +
                //                  " - GarantiAldiMi: " + sonucList.Last().GarantiAldiMi +
                //                  " - Close:" + $"{p.Bar.Close:N8}" +
                //                  " - " + islemDurum);
            }

            return sonucList;
        }


        #region Ortak Metodlar

        public static List<BardakiStratejiSonucu> StratejiIlkAlSinyalineKadarSatYap(List<BardakiStratejiSonucu> bardakiStratejiSonucuListesi)
        {
            return Genel.IlkAlSinyalineKadarSatYap(bardakiStratejiSonucuListesi);
        }

        public static IslemOzet OzetHesapla(List<IslemRaporModel> islemListe, int barSayisi)
        {
            decimal coinAdet = 0;
            decimal emirBoyutu = 100;
            decimal sonuc = 0;

            decimal alFiyat = islemListe.First().Fiyat;
            decimal burutKarOran = 0;
            decimal burutZararOran = 0;

            bool ilkIslemeGirildiMi = false;
            var durum = islemListe.First().Durum;

            foreach (var islem in islemListe)
            {
                if (durum != islem.Durum)
                {
                    if (islem.Durum == "AL")
                    {
                        alFiyat = Math.Round(islem.Fiyat, 8);

                        coinAdet = emirBoyutu / alFiyat;
                        coinAdet -= coinAdet / 1000;
                        ilkIslemeGirildiMi = true;

                        //Console.WriteLine("Zaman:" + islem.AcilisZamani +
                        //                  " - Durum:" + islem.Durum + "  - CoinSay:" + $"{coinAdet:N8}" +
                        //                  " - Fiyat:" + $"{islem.Fiyat:N8}");
                    }
                    else
                    {
                        if (ilkIslemeGirildiMi)
                        {
                            decimal islemSonuc = Math.Round(islem.Fiyat, 8) * coinAdet;
                            islemSonuc -= islemSonuc / 1000;
                            sonuc += islemSonuc - emirBoyutu;

                            var fark = Math.Round(islem.Fiyat, 8) - alFiyat;
                            fark -= fark / 1000;
                            var farkOran = fark / alFiyat * 100;
                            if (fark > 0)
                            {
                                burutKarOran += farkOran;
                            }
                            else
                            {
                                burutZararOran += farkOran * (-1);
                            }

                            var asd = (sonuc - emirBoyutu) / emirBoyutu * 100;
                            //Console.WriteLine("Zaman:" + islem.AcilisZamani +
                            //                  " - Durum:" + islem.Durum + " - CoinSay:" + $"{coinAdet:N8}" +
                            //                  " - Fiyat:" + $"{islem.Fiyat:N8}" +
                            //                  " - karDolar:" + $"{sonuc:N8}" +
                            //                  //" - karOran:" + $"{asd:N2}" +
                            //                  " - farkOran:" + $"{farkOran:N2}" +
                            //                  " - burutKarOran:" + $"{burutKarOran:N2}" +
                            //                  " - burutZararOran:" + $"{burutZararOran:N2}");
                            //Console.WriteLine("==================================");
                        }
                    }

                    durum = islem.Durum;
                }
            }

            var kar = islemListe.Last().IslemSayisi > 0 ? sonuc : 0;
            var karOrani = islemListe.Last().IslemSayisi > 0 ? sonuc : 0;
            var islemSayisi = islemListe.Last().IslemSayisi;
            var islemSayilari = IslemSayilariGetir(islemListe);
            var profitFactor = burutZararOran > 0 ? burutKarOran / burutZararOran : 0;


            //Console.WriteLine("karOrani:" + $"{karOrani:N2}" +
            //                  " - karOran2:" + $"{burutKarOran - burutZararOran:N2}" +
            //                  " - burutKarOran:" + $"{burutKarOran:N2}" +
            //                  " - burutZararOran:" + $"{burutZararOran:N2}" +
            //                  " - profitFactor:" + $"{profitFactor:N2}");
            //Console.WriteLine("==================================");


            return new IslemOzet
            {
                Sembol = islemListe.First().Sembol,
                Kar = kar,
                Sermaye = emirBoyutu,
                Parametreler = islemListe.First().Parametreler,
                Periyot = islemListe.First().Periyot,
                NetKar = karOrani,
                BurutKar = burutKarOran,
                BurutZarar = burutZararOran,
                ProfitFactor = profitFactor,
                BarSayisi = barSayisi,
                IslemSayisi = islemSayisi,
                BasariliIslemSayisi = islemSayilari.BasariliIslemSayisi,
                KarAlSayisi = islemSayilari.KarAlSayisi,
                TakipEdenDurdurSayisi = islemSayilari.TakipEdenDurdurSayisi,
                ZararDurdurSayisi = islemSayilari.ZararDurdurSayisi,
                StratejiSatSayisi = islemSayilari.StratejiSatSayisi
            };
        }

        public static IslemSayilari IslemSayilariGetir(List<IslemRaporModel> islemListe)
        {
            int karAlSayisi = 0;
            int takipEdenDurdurSayisi = 0;
            int zararDurdurSayisi = 0;
            int stratejiSatSayisi = 0;
            int basariliIslemSayisi = 0;

            if (islemListe.Last().IslemSayisi > 0)
            {
                var durum = islemListe.First().Durum;
                decimal alFiyat = 0;

                foreach (var t in islemListe)
                {
                    if (t.Durum != durum)
                    {
                        durum = t.Durum;
                        if (durum == "AL")
                        {
                            alFiyat = t.Fiyat;
                        }

                        if (durum == "SAT")
                        {
                            if (t.Fiyat - alFiyat > 0)
                            {
                                basariliIslemSayisi++;
                            }

                            if (t.DurumAciklama == "STOP")
                            {
                                zararDurdurSayisi++;
                            }

                            if (t.DurumAciklama == "TAKE-PROFIT")
                            {
                                karAlSayisi++;
                            }

                            if (t.DurumAciklama == "TRAILING-STOP")
                            {
                                takipEdenDurdurSayisi++;
                            }

                            if (t.DurumAciklama == "SAT")
                            {
                                stratejiSatSayisi++;
                            }
                        }
                    }
                }
            }

            return new IslemSayilari
            {
                BasariliIslemSayisi = basariliIslemSayisi,
                KarAlSayisi = karAlSayisi,
                StratejiSatSayisi = stratejiSatSayisi,
                TakipEdenDurdurSayisi = takipEdenDurdurSayisi,
                ZararDurdurSayisi = zararDurdurSayisi
            };
        }

        public static IslemRaporModel SonSinyalDegisimiDon(List<IslemRaporModel> islemListe)
        {
            var islem = islemListe.First();
            var durum = islemListe.First().Durum;

            foreach (var t in islemListe)
            {
                if (t.Durum != durum)
                {
                    durum = t.Durum;
                    islem = t;
                }
            }

            return islem;
        }

        public static bool StratejiAlOnay(IslemOzet ozet, int sembolunSonKapanisindanGecenSure)
        {
            var dakikaBazli = Genel.PeriyottanDakikaDon(ozet.Periyot);
            //Al sinyali gelen barda tüccar işlemi kapattıysa sonraki barda işleme girmesin diye kontrol
            if (Convert.ToDateTime(ozet.AlBarAcilisTarihi).AddMinutes(dakikaBazli + 1) > DateTime.Now)
            {
                if (sembolunSonKapanisindanGecenSure > -1)
                {
                    var satisZamani = DateTime.Now.AddMinutes(sembolunSonKapanisindanGecenSure * (-1));
                    //Satılan barda tekrar almama kontrolü
                    if (Math.Floor((DateTime.Now - satisZamani).TotalMinutes) > dakikaBazli - 1)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                var takilanlar = new StratejiyeTakilanlar
                {
                    Sembol = ozet.Sembol,
                    Periyot = ozet.Periyot,
                    Parametreler = ozet.Parametreler,
                    AlSinyalFiyat = ozet.AlSinyalFiyat,
                    GecenBarSayisi = ozet.GecenBarSayisi,
                    MevcutFiyat = ozet.MevcutFiyat,
                    Sebep = "AL Sinyali Kaçtı"
                };

                DbIslemleri.Insert_StratejiyeTakilanlar(takilanlar);
            }

            return false;
        }

        #endregion
    }
}
