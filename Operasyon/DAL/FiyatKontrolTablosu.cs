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
    
    public partial class FiyatKontrolTablosu
    {
        public int Id { get; set; }
        public Nullable<int> IslemID { get; set; }
        public Nullable<System.DateTime> KontrolTarihi { get; set; }
        public Nullable<decimal> KontrolFiyat { get; set; }
        public Nullable<decimal> KarOrani { get; set; }
    }
}
