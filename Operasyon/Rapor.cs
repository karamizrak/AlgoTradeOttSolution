using Operasyon.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operasyon
{
    class Rapor
    {
        public static void TarayiciRaporla(List<string> bilgiListesi, List<string> hataListesi)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<b>Tarayıcı işlemini tamamladı</b>\n");
            TelegramTanimlari.MesajGonder(sb.ToString(), TaramaTip.Tarayici);

            if (bilgiListesi.Count > 0)
            {
                sb.Append("<code>BİLGİ</code>\n");
                foreach (var t in bilgiListesi)
                {
                    sb.Append(" - " + t + "\n");
                }
            }

            //if (hataListesi.Count > 0)
            //{
            //    sb.Append("<code>TARAMADA HATA</code>\n");
            //    foreach (var t in hataListesi)
            //    {
            //        sb.Append(" - " + t + "\n");
            //    }
            //}

            TelegramTanimlari.MesajGonder(sb.ToString(), TaramaTip.Tarayici);
        }

        public static void AvciRaporla(List<IslemRaporModel> yeniAcilanlarListesi, List<string> stratejiyeTakilanlarListesi)
        {
            StringBuilder sb = new StringBuilder();

            if (yeniAcilanlarListesi.Count > 0)
            {
                foreach (var t in yeniAcilanlarListesi)
                {
                    var islemTipi = t.IslemTipi == TicaretTipi.Sanal ? "Sanal" : "Gerçek";

                    sb.Append("<code>YENİ AÇILAN POZİSYON</code>\n <b>" + t.Sembol + "</b>\n - İşlem Tipi: " + islemTipi + "\n - Periyot: " + t.Periyot + "\n - Parametreler: " + t.Parametreler + "\n - Giriş Fiyatı: " + string.Format("{0:N8}", t.Fiyat) + "\n - Giriş Tarihi: " + Convert.ToDateTime(t.AcilisZamani).ToString("dd MMMM HH:mm:ss") + "\n");
                }

                TelegramTanimlari.MesajGonder(sb.ToString(), TaramaTip.Avci);
            }

            if (stratejiyeTakilanlarListesi.Count > 0)
            {
                StringBuilder sb2 = new StringBuilder();
                sb2.Append("<code>STRATEJİYE TAKILANLAR</code>\n");
                foreach (var t in stratejiyeTakilanlarListesi)
                {
                    sb2.Append(" - " + t + "\n");
                }

                TelegramTanimlari.MesajGonder(sb2.ToString(), TaramaTip.Tarayici);
            }
        }

        public static void TuccarRaporla(List<IslemTablosu> kapananIsemlerListesi)
        {
            StringBuilder sb = new StringBuilder();

            if (kapananIsemlerListesi.Count > 0)
            {
                foreach (var t in kapananIsemlerListesi)
                {
                    var islemTipi = t.IslemTipi == (int)TicaretTipi.Sanal ? "Sanal" : "Gerçek";

                    sb.Append("<code>KAPANAN POZİSYON</code>\n <b>" + t.Sembol + "</b>\n - İşlem Tipi: " + islemTipi + "\n - Periyot: " + t.Periyot + "\n - Parametreler: " + t.Parametreler + "\n - Giriş Fiyatı: " + string.Format("{0:N8}", Convert.ToDecimal(t.GirisFiyat)) + "\n - Giriş Tarihi: " + Convert.ToDateTime(t.GirisTarihi).ToString("dd MMMM HH:mm:ss") + "\n - Çıkış Tarihi: " + Convert.ToDateTime(t.KontrolTarihi).ToString("dd MMMM HH:mm:ss") + "\n - Pozisyon Süresi: " + Genel.PozisyonSuresiVer(Convert.ToDateTime(t.GirisTarihi), Convert.ToDateTime(t.KontrolTarihi)) + "\n - <b>Çıkış Şekli: " + t.KapanmaSebebi + "</b>\n - <b>Çıkış Fiyatı: " + string.Format("{0:N8}", Convert.ToDecimal(t.KontrolFiyat)) + "</b>\n - <b>Kar Oranı: " + t.KarOrani + "%</b>\n-----Tepe Nokta Bilgileri-----" + "\n - Pik Fiyatı: " + string.Format("{0:N8}", Convert.ToDecimal(t.PikFiyat)) + "\n - Pik Oranı: " + t.PikOrani + "%\n - Pik Kayıt Tarihi: " + Convert.ToDateTime(t.PikTarihi).ToString("dd MMMM HH:mm") + "\n\n");
                }

                TelegramTanimlari.MesajGonder(sb.ToString(), TaramaTip.Tuccar);
            }
        }

        public static void ZRaporu()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<code>" + DateTime.Now.ToString("dd MMMM ddd") + " Gün Sonu Raporu</code>");

            sb.Append("\n <b>=== Açık İşlemler ===</b>");
            var aciklar = DbIslemleri.AcikIslemleriGetir();
            if (aciklar.Any())
            {
                for (int i = 0; i < aciklar.Count; i++)
                {
                    var islemTipi = aciklar[i].IslemTipi == (int)TicaretTipi.Sanal ? "Sanal" : "Gerçek";

                    sb.Append("\n <b>" + (i + 1) + "</b> - " + aciklar[i].Sembol + " (" + aciklar[i].KarOrani + "% - " + islemTipi + ")");
                }
            }
            else
            {
                sb.Append("\n Açık işlem bulunmuyor.");
            }
            sb.Append("\n <b>=====================</b>\n");


            sb.Append("\n <b>=== Bugün Kapanan İşlemler ===</b>");
            var kapananlar = DbIslemleri.BugunKapananIslemleriGetir();
            if (kapananlar.Any())
            {
                for (int i = 0; i < kapananlar.Count; i++)
                {
                    var islemTipi = kapananlar[i].IslemTipi == (int)TicaretTipi.Sanal ? "Sanal" : "Gerçek";

                    sb.Append("\n <b>" + (i + 1) + "</b> - " + kapananlar[i].Sembol + " (" + kapananlar[i].KarOrani + "% - " + islemTipi + ")");
                }
            }
            else
            {
                sb.Append("\n Bugün kapanan işlem bulunmuyor.");
            }
            sb.Append("\n <b>=====================</b>");

            TelegramTanimlari.MesajGonder(sb.ToString(), TaramaTip.Tuccar);
        }
    }
}
