//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gozcu.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class IslemTablosu
    {
        public int Id { get; set; }
        public string Sembol { get; set; }
        public string Periyot { get; set; }
        public System.DateTime GirisTarihi { get; set; }
        public System.DateTime GirisBarAcilisTarihi { get; set; }
        public decimal EmirBoyutu { get; set; }
        public decimal CoinAdeti { get; set; }
        public decimal GirisFiyat { get; set; }
        public decimal GirisPariteKarsiligi { get; set; }
        public System.DateTime KontrolTarihi { get; set; }
        public decimal KontrolFiyat { get; set; }
        public decimal KontrolPariteKarsiligi { get; set; }
        public decimal KarPariteKarsiligi { get; set; }
        public decimal KarOrani { get; set; }
        public bool IslemKapandiMi { get; set; }
        public Nullable<decimal> PikFiyat { get; set; }
        public Nullable<decimal> PikOrani { get; set; }
        public Nullable<System.DateTime> PikTarihi { get; set; }
        public string KapanmaSebebi { get; set; }
        public int IslemTipi { get; set; }
        public string Parametreler { get; set; }
    }
}
