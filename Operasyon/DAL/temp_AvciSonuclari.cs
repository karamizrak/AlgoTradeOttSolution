//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Operasyon.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class temp_AvciSonuclari
    {
        public int Id { get; set; }
        public string Sembol { get; set; }
        public string Periyot { get; set; }
        public Nullable<decimal> Sermaye { get; set; }
        public Nullable<decimal> Kar { get; set; }
        public Nullable<decimal> KarOran { get; set; }
        public Nullable<int> BarSayisi { get; set; }
        public Nullable<int> IslemSayisi { get; set; }
        public string SonDurum { get; set; }
        public Nullable<System.DateTime> AlBarAcilisTarihi { get; set; }
        public Nullable<int> GecenBarSayisi { get; set; }
        public Nullable<decimal> AlSinyalFiyat { get; set; }
        public Nullable<decimal> MevcutFiyat { get; set; }
        public Nullable<decimal> AlVeMevcutFarkOran { get; set; }
        public int IslemTipi { get; set; }
        public string Parametreler { get; set; }
    }
}
