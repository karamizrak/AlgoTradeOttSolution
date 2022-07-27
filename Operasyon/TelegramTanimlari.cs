using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Operasyon
{
    public class TelegramTanimlari
    {
        public static void MesajGonder(string mesaj, TaramaTip taramaTip)
        {
            try
            {
                var ayarlar = DbIslemleri.AyarlariGetir();
                var urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}&parse_mode=html";

                var apiToken = ayarlar.TelegramApiToken;
                var chatId = taramaTip == TaramaTip.Tarayici
                    ? ayarlar.TelegramLogChannel
                    : ayarlar.TelegramIslemChannel;

                urlString = string.Format(urlString, apiToken, chatId, mesaj);
                var request = WebRequest.Create(urlString);
                var rs = request.GetResponse().GetResponseStream();
                var reader = new StreamReader(rs);
                var line = "";
                var sb = new StringBuilder();
                while (line != null)
                {
                    line = reader.ReadLine();
                    if (line != null)
                        sb.Append(line);
                }

                var response = sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //MesajGonder("Telegrama gönderimde hata: " + ex.Message, taramaTip);
            }
        }
    }
}
