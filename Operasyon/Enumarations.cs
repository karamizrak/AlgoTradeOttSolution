using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Operasyon
{
    public enum ZamanPeriyotu
    {
        [Description("1m")] Dakika_1,
        [Description("3m")] Dakika_3,
        [Description("5m")] Dakika_5,
        [Description("15m")] Dakika_15,
        [Description("30m")] Dakika_30,
        [Description("1h")] Saat_1,
        [Description("2h")] Saat_2,
        [Description("4h")] Saat_4,
        [Description("6h")] Saat_6,
        [Description("8h")] Saat_8,
        [Description("12h")] Saat_12,
        [Description("1d")] Gun_1,
        [Description("3d")] Gun_3,
        [Description("1w")] Hafta_1,
        [Description("1M")] Ay_1,
    }

    public enum TaramaTip
    {
        Tarayici = 1,
        Avci = 2,
        Tuccar = 3
    }

    public enum TicaretTipi
    {
        Gercek = 1,
        Sanal = 2
    }
}
