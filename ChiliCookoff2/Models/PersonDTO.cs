using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ChiliCookoff2.Models
{
    public class PersonDTO : IValidatableObject
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public bool EnterChili { get; set; }
        public string ChiliName { get; set; }
        public string PartyCode { get; set; }
        public bool? IsAccountProfileUpdate { get; set; }

        public List<PartyDTO> JoinedParties { get; set; }

        public List<Party> HostedParties { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EnterChili && string.IsNullOrEmpty(ChiliName))
                yield return new ValidationResult("Chili Name is required.");

            if ((!IsAccountProfileUpdate.HasValue || !IsAccountProfileUpdate.Value) && string.IsNullOrEmpty(PartyCode))
                yield return new ValidationResult("Party Code is required.");
        }
    }
}