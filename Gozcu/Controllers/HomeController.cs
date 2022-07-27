using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gozcu.DAL;
using Gozcu.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Newtonsoft.Json;

namespace Gozcu.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var context = new AlgoTradeOttDbEntities();

            var acikIslemler = new SayfaModelleri.OzetIslemListeModel();
            var bugunKapananIslemler = new SayfaModelleri.OzetIslemListeModel();

            var ayar = context.AyarlarTablosu.AsEnumerable().ToList();
            decimal usdtEmirBoyutu = Convert.ToDecimal(ayar.First().EmirBoyutuUSDT);

            var acikIslemlerGetir = context.IslemTablosu.AsEnumerable().Where(x => !Convert.ToBoolean(x.IslemKapandiMi)).OrderBy(x => x.GirisTarihi).ToList();
            if (acikIslemlerGetir.Any())
            {
                var topKar = acikIslemlerGetir.Sum(x => x.KarOrani);
                var avgKar = acikIslemlerGetir.Average(x => x.KarOrani);
                var topPik = acikIslemlerGetir.Sum(x => x.PikOrani);
                var avgPik = acikIslemlerGetir.Average(x => x.PikOrani);

                acikIslemler.Islemler = acikIslemlerGetir;
                acikIslemler.OzetKarRapor = new SayfaModelleri.OzetKarRaporModel
                {
                    RaporAdi = "Açık Pozisyonlar",
                    IslemSayisi = acikIslemlerGetir.Count,
                    EmirBoyutu = usdtEmirBoyutu,
                    OrtalamaKarOran = Convert.ToDecimal(avgKar),
                    ToplamKarOran = Convert.ToDecimal(topKar),
                    OrtalamaPikOran = Convert.ToDecimal(avgPik),
                    ToplamPikOran = Convert.ToDecimal(topPik)
                };
            }
            else
            {
                acikIslemler.OzetKarRapor = new SayfaModelleri.OzetKarRaporModel
                {
                    RaporAdi = "Açık Pozisyonlar",
                    IslemSayisi = 0,
                    EmirBoyutu = usdtEmirBoyutu,
                    OrtalamaKarOran = Convert.ToDecimal(0),
                    ToplamKarOran = Convert.ToDecimal(0),
                    OrtalamaPikOran = Convert.ToDecimal(0),
                    ToplamPikOran = Convert.ToDecimal(0)
                };
            }

            var bugunKapananIslemlerGetir = context.IslemTablosu.AsEnumerable().Where(x => Convert.ToBoolean(x.IslemKapandiMi) && Convert.ToDateTime(x.KontrolTarihi) > Convert.ToDateTime(DateTime.Now.ToString("dd-MM-yyyy 00:00"))).OrderByDescending(x => x.KontrolTarihi).ToList();
            if (bugunKapananIslemlerGetir.Any())
            {
                var topKar = bugunKapananIslemlerGetir.Sum(x => x.KarOrani);
                var avgKar = bugunKapananIslemlerGetir.Average(x => x.KarOrani);
                var topPik = bugunKapananIslemlerGetir.Sum(x => x.PikOrani);
                var avgPik = bugunKapananIslemlerGetir.Average(x => x.PikOrani);

                bugunKapananIslemler.Islemler = bugunKapananIslemlerGetir;
                bugunKapananIslemler.OzetKarRapor = new SayfaModelleri.OzetKarRaporModel
                {
                    RaporAdi = "Bugün Kapanan Pozisyonlar",
                    IslemSayisi = bugunKapananIslemlerGetir.Count,
                    EmirBoyutu = usdtEmirBoyutu,
                    OrtalamaKarOran = Convert.ToDecimal(avgKar),
                    ToplamKarOran = Convert.ToDecimal(topKar),
                    OrtalamaPikOran = Convert.ToDecimal(avgPik),
                    ToplamPikOran = Convert.ToDecimal(topPik)
                };
            }
            else
            {
                bugunKapananIslemler.OzetKarRapor = new SayfaModelleri.OzetKarRaporModel
                {
                    RaporAdi = "Bugün Kapanan Pozisyonlar",
                    IslemSayisi = 0,
                    EmirBoyutu = usdtEmirBoyutu,
                    OrtalamaKarOran = Convert.ToDecimal(0),
                    ToplamKarOran = Convert.ToDecimal(0),
                    OrtalamaPikOran = Convert.ToDecimal(0),
                    ToplamPikOran = Convert.ToDecimal(0)
                };
            }

            var model = new AnaSayfaModel
            {
                AcikIslemler = acikIslemler,
                BugunKapananIslemler = bugunKapananIslemler
            };

            return View(model);
        }

        public ActionResult AcikPozisyonlar()
        {
            var nesne = new List<KeyValuePair<IslemTablosu, List<FiyatKontrolTablosu>>>();
            var context = new AlgoTradeOttDbEntities();
            var islemler = context.IslemTablosu.AsEnumerable().Where(x => !Convert.ToBoolean(x.IslemKapandiMi)).OrderBy(x => x.Id).ToList();

            foreach (var islem in islemler)
            {
                var detaylar = context.FiyatKontrolTablosu.AsEnumerable().Where(x => x.IslemID == islem.Id).OrderBy(x => x.Id).ToList();
                nesne.Add(new KeyValuePair<IslemTablosu, List<FiyatKontrolTablosu>>(islem, detaylar));
            }

            return View(nesne);
        }

        public ActionResult StartejiyeTakilanlar()
        {
            var context = new AlgoTradeOttDbEntities();
            var islemler = context.StratejiyeTakilanlar.AsEnumerable()
                .OrderBy(x => x.Id).ToList();

            return View(islemler);
        }

        public ActionResult AlinmamisPozisyonlar()
        {
            var context = new AlgoTradeOttDbEntities();
            var islemler = context.StratejiyeTakilanlar.AsEnumerable()
                .OrderBy(x => x.Id).ToList();

            return View(islemler);
        }

        public ActionResult BugunKapananlar()
        {
            var nesne = new List<KeyValuePair<IslemTablosu, List<FiyatKontrolTablosu>>>();
            var context = new AlgoTradeOttDbEntities();
            var islemler = context.IslemTablosu.AsEnumerable().Where(x => Convert.ToBoolean(x.IslemKapandiMi) && Convert.ToDateTime(x.KontrolTarihi) > Convert.ToDateTime(DateTime.Now.ToString("dd-MM-yyyy 00:00"))).OrderByDescending(x => x.Id).ToList();

            foreach (var islem in islemler)
            {
                var detaylar = context.FiyatKontrolTablosu.AsEnumerable().Where(x => x.IslemID == islem.Id).OrderBy(x => x.Id).ToList();
                nesne.Add(new KeyValuePair<IslemTablosu, List<FiyatKontrolTablosu>>(islem, detaylar));
            }

            return View(nesne);
        }

        public ActionResult TumKapananlar()
        {
            return View();
        }

        public string TumKapananlarDon(string baslangic, string bitis)
        {
            var context = new AlgoTradeOttDbEntities();
            var islemler = context.IslemTablosu.AsEnumerable().Where(x => Convert.ToBoolean(x.IslemKapandiMi) && Convert.ToDateTime(x.KontrolTarihi) >= Convert.ToDateTime(baslangic) && Convert.ToDateTime(x.KontrolTarihi) < Convert.ToDateTime(bitis)).ToList();

            return JsonConvert.SerializeObject(islemler);
        }

        public ActionResult PozisyonDetayDon(int islemID)
        {
            var context = new AlgoTradeOttDbEntities();
            var islem = context.IslemTablosu.AsEnumerable().First(x => x.Id == islemID);
            var detaylar = context.FiyatKontrolTablosu.AsEnumerable().Where(x => x.IslemID == islemID).OrderBy(x => x.Id).ToList();
            var nesne = new KeyValuePair<IslemTablosu, List<FiyatKontrolTablosu>>(islem, detaylar);

            return PartialView("~/Views/Home/Shared/_PozisyonOzetPartial.cshtml", nesne);
        }

        public ActionResult StratejiPuanlari()
        {
            var lst = new List<RehberTablo>();
            var context = new AlgoTradeOttDbEntities();
            var semboller = context.RehberTablo.ToList();

            return View(semboller);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Robotların işlem özetleri";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "https://twitter.com/crypto_melih";

            return View();
        }

        public ActionResult Ayarlar()
        {
            var context = new AlgoTradeOttDbEntities();
            var ayarlar = context.AyarlarTablosu.First();

            return View(ayarlar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AyarKaydet(AyarlarTablosu ayarlar)
        {
            var context = new AlgoTradeOttDbEntities();
            var result = context.AyarlarTablosu.SingleOrDefault(x => x.Id == ayarlar.Id);
            if (result != null)
            {
                result.BinanceApiKey = ayarlar.BinanceApiKey;
                result.BinanceApiSecret = ayarlar.BinanceApiSecret;
                result.EmirBoyutuUSDT = ayarlar.EmirBoyutuUSDT;
                result.HaricSemboller = ayarlar.HaricSemboller;
                result.IslemPeriyot = ayarlar.IslemPeriyot;
                result.BacktestBarSayisi = ayarlar.BacktestBarSayisi;
                result.BacktestEsikKarOrani = ayarlar.BacktestEsikKarOrani;
                result.KarAlOran = ayarlar.KarAlOran;
                result.TakipEdenDurdurOran = ayarlar.TakipEdenDurdurOran;
                result.TelegramApiToken = ayarlar.TelegramApiToken;
                result.TelegramIslemChannel = ayarlar.TelegramIslemChannel;
                result.TelegramLogChannel = ayarlar.TelegramLogChannel;
                result.IslemTipi = ayarlar.IslemTipi;

                context.SaveChanges();


                return RedirectToAction("Ayarlar", "Home");
            }

            return View(ayarlar);
        }

        public ActionResult StratejiParametreleri()
        {
            return View();
        }
        public string StratejiParametreleriGetir()
        {
            var context = new AlgoTradeOttDbEntities();
            var semboller = context.StratejiParametreleri.ToList();

            return JsonConvert.SerializeObject(semboller);
        }

        public string StratejiParametreleriKayitGetir(int islemID)
        {
            var context = new AlgoTradeOttDbEntities();
            var semboller = context.StratejiParametreleri.First(x => x.Id == islemID);

            return JsonConvert.SerializeObject(semboller);
        }

        public void StratejiParametreleriKaydet(int id, decimal opt1, decimal opt2, decimal opt3)
        {
            var context = new AlgoTradeOttDbEntities();

            var kayit = context.StratejiParametreleri.First(x => x.Id == id);
            kayit.AltDeger = opt1;
            kayit.UstDeger = opt2;
            kayit.Adim = opt3;

            context.SaveChanges();
        }

    }
}