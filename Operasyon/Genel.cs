using Binance.API.Csharp.Client.Models.Enums;
using Binance.API.Csharp.Client.Models.Market;
using Operasyon.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trady.Core.Infrastructure;

namespace Operasyon
{
   public static class Genel
    {
        public static readonly DateTime StartUnixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static ZamanPeriyotu StringdenZamanPeriyotuDon(string periyot)
        {
            switch (periyot)
            {
                case "1m":
                    return ZamanPeriyotu.Dakika_1;
                case "3m":
                    return ZamanPeriyotu.Dakika_3;
                case "5m":
                    return ZamanPeriyotu.Dakika_5;
                case "15m":
                    return ZamanPeriyotu.Dakika_15;
                case "30m":
                    return ZamanPeriyotu.Dakika_30;
                case "1h":
                    return ZamanPeriyotu.Saat_1;
                case "2h":
                    return ZamanPeriyotu.Saat_2;
                case "4h":
                    return ZamanPeriyotu.Saat_4;
                case "6h":
                    return ZamanPeriyotu.Saat_6;
                case "8h":
                    return ZamanPeriyotu.Saat_8;
                case "12h":
                    return ZamanPeriyotu.Saat_12;
                case "1d":
                    return ZamanPeriyotu.Gun_1;
                case "3d":
                    return ZamanPeriyotu.Gun_3;
                case "1w":
                    return ZamanPeriyotu.Hafta_1;
                case "1M":
                    return ZamanPeriyotu.Ay_1;
            }

            return ZamanPeriyotu.Gun_1;
        }

        public static TimeInterval StringdenTimeIntervalDon(string periyot)
        {
            switch (periyot)
            {
                case "1m":
                    return TimeInterval.Minutes_1;
                case "3m":
                    return TimeInterval.Minutes_3;
                case "5m":
                    return TimeInterval.Minutes_5;
                case "15m":
                    return TimeInterval.Minutes_15;
                case "30m":
                    return TimeInterval.Minutes_30;
                case "1h":
                    return TimeInterval.Hours_1;
                case "2h":
                    return TimeInterval.Hours_2;
                case "4h":
                    return TimeInterval.Hours_4;
                case "6h":
                    return TimeInterval.Hours_6;
                case "8h":
                    return TimeInterval.Hours_8;
                case "12h":
                    return TimeInterval.Hours_12;
                case "1d":
                    return TimeInterval.Days_1;
                case "3d":
                    return TimeInterval.Days_3;
                case "1w":
                    return TimeInterval.Weeks_1;
                case "1M":
                    return TimeInterval.Months_1;
            }

            return TimeInterval.Days_1;
        }

        public static int PeriyottanDakikaDon(string periyot)
        {
            switch (periyot)
            {
                case "1m":
                    return 1;
                case "3m":
                    return 3;
                case "5m":
                    return 5;
                case "15m":
                    return 15;
                case "30m":
                    return 30;
                case "1h":
                    return 60;
                case "2h":
                    return 120;
                case "4h":
                    return 240;
                case "6h":
                    return 360;
                case "8h":
                    return 480;
                case "12h":
                    return 720;
                case "1d":
                    return 1440;
                case "3d":
                    return 4320;
                case "1w":
                    return 10080;
                case "1M":
                    return 43200;
            }

            return 1440;
        }

        public static string PozisyonSuresiVer(DateTime giris, DateTime cikis)
        {
            int Years = new DateTime(DateTime.Now.Subtract(giris).Ticks).Year - 1;
            DateTime PastYearDate = giris.AddYears(Years);
            int Months = 0;
            for (int i = 1; i <= 12; i++)
            {
                if (PastYearDate.AddMonths(i) == cikis)
                {
                    Months = i;
                    break;
                }
                else if (PastYearDate.AddMonths(i) >= cikis)
                {
                    Months = i - 1;
                    break;
                }
            }

            int Days = cikis.Subtract(PastYearDate.AddMonths(Months)).Days;
            int Hours = cikis.Subtract(PastYearDate).Hours;
            int Minutes = cikis.Subtract(PastYearDate).Minutes;
            int Seconds = cikis.Subtract(PastYearDate).Seconds;
            var sonuc = "";
            if (Months > 0)
                sonuc += Months + " Ay ";
            if (Days > 0)
                sonuc += Days + " Gün ";
            if (Hours > 0)
                sonuc += Hours + " Saat ";
            if (Minutes > 0)
                sonuc += Minutes + " Dakika ";
            if (Seconds > 0)
                sonuc += Seconds + " Saniye";
            return sonuc;
        }

        public static int SembolunSonKapanisindanGecenDakika(string sembol)
        {
            //var sonHareket = DbIslemleri.SembolunSonIsleminiGetir(sembol);
            //if (sonHareket != null)
            //{
            //    return Convert.ToInt32((DateTime.Now - sonHareket.KontrolTarihi).TotalMinutes);
            //}

            return -1;
        }

        public static List<BardakiStratejiSonucu> IlkAlSinyalineKadarSatYap(List<BardakiStratejiSonucu> bardakiStratejiSonucuListesi)
        {
            if (bardakiStratejiSonucuListesi.Count == 0) return bardakiStratejiSonucuListesi;

            var ilkSatGeldiMi = bardakiStratejiSonucuListesi.First().IslemDurum == "SAT";
            var donusListe = new List<BardakiStratejiSonucu>();
            foreach (var eleman in bardakiStratejiSonucuListesi)
            {
                donusListe.Add(new BardakiStratejiSonucu
                {
                    Bar = eleman.Bar,
                    Parametreler = eleman.Parametreler,
                    IslemDurum = !ilkSatGeldiMi ? "SAT" : eleman.IslemDurum,
                    KesisimBuradaMi = eleman.KesisimBuradaMi,
                    GarantiAldiMi = eleman.GarantiAldiMi
                });
                if (!ilkSatGeldiMi && eleman.IslemDurum == "SAT")
                {
                    ilkSatGeldiMi = true;
                }
            }

            return donusListe;
        }

        public static List<decimal> ParametreListesiDonList(StratejiParametreleri parametre)
        {
            var returnList = new List<decimal>();
            var altDeger = Convert.ToDecimal(parametre.AltDeger);

            returnList.Add(altDeger);
            while (true)
            {
                altDeger += Convert.ToDecimal(parametre.Adim);

                if (altDeger > Convert.ToDecimal(parametre.UstDeger))
                {
                    break;
                }

                returnList.Add(altDeger);
            }

            if (Convert.ToDecimal(parametre.UstDeger) > 0)
            {
                if (!returnList.Contains(Convert.ToDecimal(parametre.UstDeger)))
                {
                    returnList.Add(Convert.ToDecimal(parametre.UstDeger));
                }
            }

            return returnList;
        }
    }

    public class BardakiStratejiSonucu
    {
        public object[] Parametreler { get; set; }
        public string IslemDurum { get; set; }
        public bool KesisimBuradaMi { get; set; }
        public bool GarantiAldiMi { get; set; }
        public IOhlcv Bar { get; set; }
    }

    public class IslemOzet : AvciSonuclari
    {
        public decimal NetKar { get; set; }
        public decimal BurutKar { get; set; }
        public decimal BurutZarar { get; set; }
        public decimal ProfitFactor { get; set; }
        public int BasariliIslemSayisi { get; set; }
        public int KarAlSayisi { get; set; }
        public int TakipEdenDurdurSayisi { get; set; }
        public int ZararDurdurSayisi { get; set; }
        public int StratejiSatSayisi { get; set; }
    }

    public class IslemRaporModel
    {
        public int IslemIndex { get; set; }
        public string Sembol { get; set; }
        public string Periyot { get; set; }
        public string Parametreler { get; set; }
        public DateTime AcilisZamani { get; set; }
        public string Durum { get; set; }
        public string DurumAciklama { get; set; }
        public decimal Fiyat { get; set; }
        public int IslemSayisi { get; set; }
        public IOhlcv Bar { get; set; }
        public TicaretTipi IslemTipi { get; set; }
    }

    public class IslemSayilari
    {
        public int BasariliIslemSayisi { get; set; }
        public int KarAlSayisi { get; set; }
        public int TakipEdenDurdurSayisi { get; set; }
        public int ZararDurdurSayisi { get; set; }
        public int StratejiSatSayisi { get; set; }
    }

    public class RehberTablo
    {
        public int Id { get; set; }
        public SymbolPrice Sembol { get; set; }
        public ZamanPeriyotu Periyot { get; set; }
        public string Parametreler { get; set; }
    }

    public class EmirSonuc
    {
        public bool BasariliMi { get; set; }
        public long TransactTime { get; set; }
        public bool ZorlaKapat { get; set; }
    }

    public class TicaretBilgisi
    {
        public decimal Fiyat { get; set; }
        public decimal Adet { get; set; }
    }

}
