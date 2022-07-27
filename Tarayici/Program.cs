using Operasyon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarayici
{
    /// <summary>
    /// Bakctest yapacak olan robot
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var stratejiIslemleri = new StratejiIslemleri();
            stratejiIslemleri.TarayiciTaramaYap();
        }
    }
}
