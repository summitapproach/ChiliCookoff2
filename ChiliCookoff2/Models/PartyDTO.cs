using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ChiliCookoff2.Models
{
    public class PartyDTO : IValidatableObject
    {
        public Guid PartyId { get; set; }
        [Required]
        public string PartyName { get; set; }
        public DateTime? PartyDate { get; set; }
        public bool? EnterChili { get; set; }
        public string ChiliName { get; set; }
        public string HostName { get; set; }
        public int? ChiliNumber { get; set; }
        public string PartyCode { get; set; }
        public int MaxChilis { get; set; }
        public bool HasVotingStarted { get; set; }
        public bool HaveVotesBeenTallied { get; set; }
        public bool HasSubmittedBallot { get; set; }
        public bool IsHostUpdate { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (EnterChili.HasValue && EnterChili.Value && string.IsNullOrEmpty(ChiliName))
                yield return new ValidationResult("Chili Name is required.");
        }
    }
}