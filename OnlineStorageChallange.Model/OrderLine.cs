//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineStorageChallange.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderLine
    {
        public int OrderID { get; set; }
        public int OrderLineID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public int LineCost { get; set; }
    
        public virtual OrderHeader OrderHeader { get; set; }
        public virtual Product Product { get; set; }
    }
}
