using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChiliCookoff2.Models
{
    public class BallotDTO
    {
        public Guid PartyId { get; set; }
        public List<Vote> Votes { get; set; }
    }
}