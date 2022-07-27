using Binance.API.Csharp.Client.Models.Market;
using Operasyon.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operasyon
{
    class DbIslemleri
    {
        #region Get
        public static AyarlarTablosu AyarlariGetir()
        {
            var context = new AlgoTradeOttDbEntities();
            return context.AyarlarTablosu.FirstOrDefault();
        }

        public static List<StratejiParametreleri> StratejiParametreleriGetir()
        {
            var context = new AlgoTradeOttDbEntities();
            return context.StratejiParametreleri.ToList();
        }

        public static List<RehberTablo> RehberTabloGetir()
        {
            var rehberTabloList = new List<RehberTablo>();

            var context = new AlgoTradeOttDbEntities();
            var acikIslemler = context.IslemTablosu.Where(x => !x.IslemKapandiMi).Select(x => x.Sembol).ToList();

            var tickerPrices = BinanceIslemleri.SembolleriVeFiyatlariDon("USDT", "");

            var tarayiciRehber = context.RehberTablo.Where(x => !acikIslemler.Contains(x.Sembol));
            if (tarayiciRehber.Any())
            {
                foreach (var rehber in tarayiciRehber)
                {
                    rehberTabloList.Add(new RehberTablo
                    {
                        Id = rehber.Id,
                        Sembol = new SymbolPrice { Symbol = rehber.Sembol, Price = tickerPrices.First(x => x.Symbol == rehber.Sembol).Price },
                        Periyot = Genel.StringdenZamanPeriyotuDon(rehber.Periyot),
                        Parametreler = rehber.Parametreler
                    });
                }
            }


            return rehberTabloList;
        }

        public static IslemTablosu SembolunSonIsleminiGetir(string sembol)
        {
            var context = new AlgoTradeOttDbEntities();
            var sonIslem = context.IslemTablosu.Where(x => x.IslemKapandiMi && x.Sembol == sembol).OrderByDescending(x => x.KontrolTarihi).Take(1);

            return sonIslem.Any() ? sonIslem.First() : null;
        }

        public static List<IslemTablosu> AcikIslemleriGetir()
        {
            var context = new AlgoTradeOttDbEntities();
            return context.IslemTablosu.Where(x => !x.IslemKapandiMi).OrderBy(x => x.Sembol).ToList();
        }

        public static List<IslemTablosu> BugunKapananIslemleriGetir()
        {
            var context = new AlgoTradeOttDbEntities();
            var baslangic = DateTime.Today;
            var bitis = DateTime.Today.AddDays(1);
            return context.IslemTablosu.Where(x => x.IslemKapandiMi && x.KontrolTarihi > baslangic && x.KontrolTarihi < bitis).OrderBy(x => x.Sembol).ToList();
        }

        #endregion

        #region Truncate

        public static void Truncate_temp_RehberTablo()
        {
            try
            {
                var context = new AlgoTradeOttDbEntities();
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE temp_RehberTablo");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Truncate_temp_AvciSonuclari()
        {
            try
            {
                var context = new AlgoTradeOttDbEntities();
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE temp_AvciSonuclari");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Truncate_StratejiyeTakilanlar()
        {
            try
            {
                var context = new AlgoTradeOttDbEntities();
                context.Database.ExecuteSqlCommand("TRUNCATE TABLE StratejiyeTakilanlar");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Insert

        public static void Insert_temp_RehberTablo(IslemOzet islemOzet)
        {
            try
            {
                var context = new AlgoTradeOttDbEntities();
                context.temp_RehberTablo.Add(new temp_RehberTablo
                {
                    Sembol = islemOzet.Sembol,
                    Periyot = islemOzet.Periyot,
                    Parametreler = islemOzet.Parametreler,
                    NetKar = islemOzet.NetKar,
                    BurutKar = islemOzet.BurutKar,
                    BurutZarar = islemOzet.BurutZarar,
                    ProfitFactor = islemOzet.ProfitFactor,
                    BarSayisi = islemOzet.BarSayisi,
                    IslemSayisi = islemOzet.IslemSayisi,
                    BasariliIslemSayisi = islemOzet.BasariliIslemSayisi,
                    KarAlSayisi = islemOzet.KarAlSayisi,
                    TakipEdenDurdurSayisi = islemOzet.TakipEdenDurdurSayisi,
                    ZararDurdurSayisi = islemOzet.ZararDurdurSayisi,
                    StratejiSatSayisi = islemOzet.StratejiSatSayisi
                });
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Insert_temp_AvciSonuclari(IslemOzet islemOzet)
        {
            try
            {
                var context = new AlgoTradeOttDbEntities();
                context.temp_AvciSonuclari.Add(new temp_AvciSonuclari
                {
                    Sembol = islemOzet.Sembol,
                    Periyot = islemOzet.Periyot,
                    Parametreler = islemOzet.Parametreler,
                    Sermaye = islemOzet.Sermaye,
                    Kar = islemOzet.Kar,
                    KarOran = islemOzet.KarOran,
                    BarSayisi = islemOzet.BarSayisi,
                    IslemSayisi = islemOzet.IslemSayisi,
                    SonDurum = islemOzet.SonDurum,
                    AlBarAcilisTarihi = islemOzet.AlBarAcilisTarihi,
                    GecenBarSayisi = islemOzet.GecenBarSayisi,
                    AlSinyalFiyat = islemOzet.AlSinyalFiyat,
                    MevcutFiyat = islemOzet.MevcutFiyat,
                    AlVeMevcutFarkOran = islemOzet.AlVeMevcutFarkOran,
                    IslemTipi = islemOzet.IslemTipi
                });
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Insert_StratejiyeTakilanlar(StratejiyeTakilanlar stratejiyeTakilan)
        {
            try
            {
                var context = new AlgoTradeOttDbEntities();
                context.StratejiyeTakilanlar.Add(stratejiyeTakilan);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Insert_FiyatKontrolTablosu(int islemId, decimal fiyat, decimal karOran)
        {
            try
            {
                var context = new AlgoTradeOttDbEntities();
                context.FiyatKontrolTablosu.Add(new FiyatKontrolTablosu
                {
                    IslemID = islemId,
                    KontrolTarihi = DateTime.Now,
                    KontrolFiyat = fiyat,
                    KarOrani = karOran
                });
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void TaramaSonucKaydet(string parite, int esikKarOrani)
        {
            try
            {
                var query = @"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                            DECLARE CRS_Rehber CURSOR FOR
                            SELECT Sembol FROM temp_RehberTablo
                            where Sembol like '%{0}'
                            Group by Sembol
                            order by Sembol

                            DECLARE	@Sembol NVARCHAR(50);
                            Create TABLE #Temp (Id int, Sembol nvarchar(50), Periyot nvarchar(50), Parametreler nvarchar(MAX), NetKar decimal(12, 2),BurutKar decimal(12, 2),BurutZarar decimal(12, 2),ProfitFactor decimal(12, 2), BarSayisi int, IslemSayisi int, BasariliIslemSayisi int, KarAlSayisi int, TakipEdenDurdurSayisi int, ZararDurdurSayisi int, StratejiSatSayisi int);

                            OPEN CRS_Rehber
                            FETCH NEXT FROM CRS_Rehber INTO @Sembol

                            WHILE @@FETCH_STATUS =0
	                            BEGIN
	                            INSERT INTO #Temp (Id,Sembol,Periyot,Parametreler,NetKar,BurutKar,BurutZarar,ProfitFactor,BarSayisi,IslemSayisi,BasariliIslemSayisi,KarAlSayisi,TakipEdenDurdurSayisi,ZararDurdurSayisi,StratejiSatSayisi)
	                                                        
	                            SELECT TOP 1 * FROM temp_RehberTablo 
	                            WHERE Sembol=@Sembol
	                            and NetKar>{1}
	                            and BasariliIslemSayisi>0
	                            and CAST(BasariliIslemSayisi as decimal(6,2))/CAST(IslemSayisi as decimal(6,2))>0.5 
	                            and CAST(ZararDurdurSayisi as decimal(6,2))/CAST(IslemSayisi as decimal(6,2))<0.5 
	                            ORDER BY NetKar desc;
		                                                        
		                            FETCH NEXT FROM CRS_Rehber INTO @Sembol
	                            END

                            CLOSE CRS_Rehber
                            DEALLOCATE CRS_Rehber
                            DELETE FROM RehberTablo WHERE Sembol LIKE '%{0}'
                            INSERT INTO RehberTablo (Sembol,Periyot,Parametreler,NetKar,BurutKar,BurutZarar,BarSayisi,IslemSayisi,BasariliIslemSayisi,ProfitFactor,KarAlSayisi,TakipEdenDurdurSayisi,ZararDurdurSayisi,StratejiSatSayisi)
                            SELECT Sembol,Periyot,Parametreler,NetKar,BurutKar,BurutZarar,BarSayisi,IslemSayisi,BasariliIslemSayisi,ProfitFactor,KarAlSayisi,TakipEdenDurdurSayisi,ZararDurdurSayisi,StratejiSatSayisi FROM #Temp
                            Drop Table #Temp";

                var context = new AlgoTradeOttDbEntities();
                context.Database.ExecuteSqlCommand(string.Format(query, parite, esikKarOrani));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void AvciKaydet()
        {
            try
            {
                var query = @"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                            DECLARE Karli_Sec_Cursor CURSOR FOR
                            SELECT Sembol FROM temp_AvciSonuclari
                            GROUP BY Sembol
                            UNION
                            SELECT Sembol FROM IslemTablosu
                            WHERE IslemKapandiMi=0
                            GROUP BY Sembol

                            DECLARE	@Sembol NVARCHAR(50);

                            TRUNCATE TABLE AvciSonuclari
                            OPEN Karli_Sec_Cursor
                            FETCH NEXT FROM Karli_Sec_Cursor INTO @Sembol
                            WHILE @@FETCH_STATUS =0
	                            BEGIN
	                            INSERT INTO AvciSonuclari 
	                            SELECT TOP 1 Sembol, Periyot, Sermaye, Kar, KarOran, BarSayisi, IslemSayisi, SonDurum, AlBarAcilisTarihi, GecenBarSayisi, AlSinyalFiyat, MevcutFiyat, AlVeMevcutFarkOran, IslemTipi, Parametreler 
	                            FROM temp_AvciSonuclari
	                            WHERE Sembol=@Sembol
	                            ORDER BY KarOran DESC
		                            FETCH NEXT FROM Karli_Sec_Cursor INTO @Sembol
	                            END
                            CLOSE Karli_Sec_Cursor
                            DEALLOCATE Karli_Sec_Cursor
                            SELECT * FROM AvciSonuclari ORDER BY KarOran DESC;";

                var context = new AlgoTradeOttDbEntities();
                context.Database.ExecuteSqlCommand(query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static List<IslemRaporModel> IslemAcilisKaydet()
        {
            var islemListe = new List<IslemRaporModel>();

            try
            {
                var context = new AlgoTradeOttDbEntities();
                var ayarlar = AyarlariGetir();
                var acikIslemler = context.IslemTablosu.Where(x => !x.IslemKapandiMi).Select(x => x.Sembol).ToList();
                var avciSonuclari = context.AvciSonuclari.Where(x => !acikIslemler.Contains(x.Sembol));

                if (avciSonuclari.Any())
                {
                    foreach (var avciSonuc in avciSonuclari)
                    {
                        var mevcutFiyat = Convert.ToDecimal(avciSonuc.MevcutFiyat);
                        var coinAdet = 1 / mevcutFiyat;
                        var fiyat = mevcutFiyat;
                        var adet = coinAdet;
                        var pariteKarsiligi = coinAdet * mevcutFiyat;
                        var now = DateTime.Now;
                        var islemTipi = ayarlar.IslemTipi;

                        if (ayarlar.IslemTipi == (int)TicaretTipi.Gercek)
                        {
                            var ticaret = BinanceIslemleri.AlisYap(avciSonuc.Sembol);
                            if (ticaret.BasariliMi)
                            {
                                var ticaretFiyat = BinanceIslemleri.TicaretFiyatiDon(avciSonuc.Sembol, ticaret.TransactTime);
                                fiyat = ticaretFiyat.Fiyat > -1 ? ticaretFiyat.Fiyat : mevcutFiyat;
                                adet = ticaretFiyat.Fiyat > -1 ? ticaretFiyat.Adet : coinAdet;
                                pariteKarsiligi = ticaretFiyat.Fiyat > -1
                                    ? ticaretFiyat.Adet * ticaretFiyat.Fiyat
                                    : coinAdet * mevcutFiyat;
                            }
                            else
                            {
                                islemTipi = (int)TicaretTipi.Sanal;
                            }
                        }
                        else
                        {
                            islemTipi = (int)TicaretTipi.Sanal;
                        }

                        islemListe.Add(new IslemRaporModel
                        {
                            Sembol = avciSonuc.Sembol,
                            Periyot = avciSonuc.Periyot,
                            Fiyat = fiyat,
                            AcilisZamani = now,
                            IslemTipi = (TicaretTipi)islemTipi,
                            Parametreler = avciSonuc.Parametreler
                        });

                        var islem = new IslemTablosu
                        {
                            Sembol = avciSonuc.Sembol,
                            Periyot = avciSonuc.Periyot,
                            Parametreler = avciSonuc.Parametreler,
                            GirisTarihi = now,
                            GirisBarAcilisTarihi = Convert.ToDateTime(avciSonuc.AlBarAcilisTarihi),
                            EmirBoyutu = ayarlar.EmirBoyutuUSDT,
                            CoinAdeti = adet,
                            GirisFiyat = fiyat,
                            GirisPariteKarsiligi = Convert.ToDecimal(pariteKarsiligi),
                            KontrolTarihi = now,
                            KontrolFiyat = fiyat,
                            KontrolPariteKarsiligi = Convert.ToDecimal(pariteKarsiligi),
                            KarPariteKarsiligi = Convert.ToDecimal(0),
                            KarOrani = Convert.ToDecimal(0),
                            PikFiyat = fiyat,
                            PikOrani = Convert.ToDecimal(0),
                            PikTarihi = now,
                            IslemKapandiMi = false,
                            IslemTipi = islemTipi
                        };

                        var context2 = new AlgoTradeOttDbEntities();
                        context2.IslemTablosu.Add(islem);
                        context2.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return islemListe;
        }

        public static void IslemBilgileriGuncelle(int islemId, decimal fiyat, decimal coinAdeti, decimal kontrolPariteKarsiligi, decimal karPariteKarsiligi, decimal karOran, decimal pikFiyat, decimal pikOrani, DateTime pikTarihi, int islemDurum, string kapanmaSebebi)
        {
            var context = new AlgoTradeOttDbEntities();
            var result = context.IslemTablosu.SingleOrDefault(x => x.Id == islemId);
            if (result != null)
            {
                result.CoinAdeti = coinAdeti;
                result.KontrolTarihi = DateTime.Now;
                result.KontrolFiyat = fiyat;
                result.KontrolPariteKarsiligi = kontrolPariteKarsiligi;
                result.KarPariteKarsiligi = karPariteKarsiligi;
                result.KarOrani = karOran;
                result.PikFiyat = pikFiyat;
                result.PikOrani = pikOrani;
                result.PikTarihi = pikTarihi;
                result.IslemKapandiMi = Convert.ToBoolean(islemDurum);
                result.KapanmaSebebi = kapanmaSebebi;

                context.SaveChanges();
            }
        }

        #endregion

        #region Update

        #endregion
    }
}
