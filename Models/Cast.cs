//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SocialMediaDB.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Cast
    {
        public string character { get; set; }
        public string name { get; set; }
        public string profile_path { get; set; }
        public Nullable<int> MovieID { get; set; }
        public int id { get; set; }
    
        public virtual MovieDetail MovieDetail { get; set; }
    }
}
