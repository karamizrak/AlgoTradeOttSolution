using Operasyon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panik
{
    /// <summary>
    /// Çalıştığı anda tüm açık pozisyonları kapatacak.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var stratejiIslemleri = new StratejiIslemleri();
            stratejiIslemleri.PanikCalistir();
        }
    }
}
