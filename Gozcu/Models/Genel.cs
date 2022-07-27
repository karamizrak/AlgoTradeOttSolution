using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gozcu.Models
{
    public class Genel
    {
        public static string PozisyonSuresiVer(DateTime Giris, DateTime Cikis)
        {
            int Years = new DateTime(DateTime.Now.Subtract(Giris).Ticks).Year - 1;
            DateTime PastYearDate = Giris.AddYears(Years);
            int Months = 0;
            for (int i = 1; i <= 12; i++)
            {
                if (PastYearDate.AddMonths(i) == Cikis)
                {
                    Months = i;
                    break;
                }
                else if (PastYearDate.AddMonths(i) >= Cikis)
                {
                    Months = i - 1;
                    break;
                }
            }

            int Days = Cikis.Subtract(PastYearDate.AddMonths(Months)).Days;
            int Hours = Cikis.Subtract(PastYearDate).Hours;
            int Minutes = Cikis.Subtract(PastYearDate).Minutes;
            int Seconds = Cikis.Subtract(PastYearDate).Seconds;
            var sonuc = "";
            if (Months > 0)
                sonuc += Months + " Ay ";
            if (Days > 0)
                sonuc += Days + " Gn ";
            if (Hours > 0)
                sonuc += Hours + " Sa ";
            if (Minutes > 0)
                sonuc += Minutes + " Dk ";
            if (Seconds > 0)
                sonuc += Seconds + " Sn ";
            return sonuc;
        }
    }
}