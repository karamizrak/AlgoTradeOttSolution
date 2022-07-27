using Operasyon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tuccar
{
    /// <summary>
    /// Alınmış olan sembolleri satmak için bekleyen robot. Sat stop loss take profit trailing stop işlerini yapan robot
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var stratejiIslemleri = new StratejiIslemleri();
            stratejiIslemleri.TuccarTaramaYap();

            //Console.ReadLine();
        }
    }
}
