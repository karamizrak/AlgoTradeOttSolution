using Operasyon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avci
{

    /// <summary>
    /// Tarayıca bakctest yapılmış optimize edilmiş değerleri sürekli al sinyali bekleyten robot
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var stratejiIslemleri = new StratejiIslemleri();
            stratejiIslemleri.AvciTaramaYap();
        }
    }
}
