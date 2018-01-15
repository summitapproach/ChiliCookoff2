using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChiliCookoff2.Models
{
    public class ChiliDTO
    {
        public Guid ChiliId { get; set; }
        public int Place { get; set; }
        public string ChefName { get; set; }
        public int ChiliNumber { get; set; }
        public string ChiliName { get; set; }
        public int TotalPoints { get; set; }
        public int FirstPlaceVotes { get; set; }
        public int SecondPlaceVotes { get; set; }
        public int ThirdPlaceVotes { get; set; }
    }
}