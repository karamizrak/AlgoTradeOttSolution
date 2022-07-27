using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gozcu.DAL;

namespace Gozcu.Models
{
    public class SayfaModelleri
    {
        public class DetayliIslemListeModel
        {
            public IslemTablosu Islem { get; set; }
            public OzetKarRaporModel OzetKarRapor { get; set; }
            public List<FiyatKontrolTablosu> Detay { get; set; }
        }

        public class OzetIslemListeModel
        {
            public List<IslemTablosu> Islemler { get; set; }
            public OzetKarRaporModel OzetKarRapor { get; set; }
        }

        public class OzetKarRaporModel
        {
            public string RaporAdi { get; set; }
            public int IslemSayisi { get; set; }
            public decimal EmirBoyutu { get; set; }
            public int OrtalamaIslemSayisi { get; set; }
            public decimal OrtalamaKarOran { get; set; }
            public decimal ToplamKarOran { get; set; }
            public decimal TetherToplamKar { get; set; }
            public decimal OrtalamaPikOran { get; set; }
            public decimal ToplamPikOran { get; set; }
            public decimal TetherToplamPik { get; set; }
        }
    }

    public class AnaSayfaModel
    {
        public SayfaModelleri.OzetIslemListeModel AcikIslemler { get; set; }
        public SayfaModelleri.OzetIslemListeModel BugunKapananIslemler { get; set; }
    }
}