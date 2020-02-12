using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialMediaDB.Models
{
    public class MovieDisplay
    {
        public MovieDetail movieInfo { get; set; }
        public List<Cast> cast { get; set; }
        public List<Review> reviews { get; set; }
    }
}