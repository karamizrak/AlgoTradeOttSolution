using Operasyon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zraporu
{
    /// <summary>
    /// Açık olan tüm işlemleri, kar oranlarını, o gün kapanmış olan işlemleri telegramdan postalayacak.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var stratejiIslemleri = new StratejiIslemleri();
            stratejiIslemleri.ZRaporuAl();
        }
    }
}
