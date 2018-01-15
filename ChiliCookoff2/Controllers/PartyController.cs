using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ChiliCookoff2.Models;
using System.Configuration;

namespace ChiliCookoff2.Controllers
{
    [RoutePrefix("api/Party")]
    public class PartyController : ApiController
    {
        private ChiliCookoffEntities db = new ChiliCookoffEntities();
        private Random randomNumberGenerator = new Random();

        // GET: api/Party
        [ResponseType(typeof(PersonDTO))]
        public IHttpActionResult GetParties()
        {
            var isAuthenticationOn = Convert.ToBoolean(ConfigurationManager.AppSettings["IsAuthenticationOn"]);

            if (isAuthenticationOn && !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            Guid userId;

            if (isAuthenticationOn)
            {
                var userIdString = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
                userId = Guid.Parse(userIdString);

            }
            else
                userId = Guid.Parse("885cfa23-87c3-43cb-9326-8f317087e4f9");

            var person = db.People.Where(p => p.UserId == userId).FirstOrDefault();
            var personDto = new PersonDTO();

            if (person != null)
            {
                personDto.HostedParties = db.Parties.Where(p => p.HostId == person.Id).ToList();

                var joinedParties = db.Parties.Where(p => db.PartyPersons.Where(pp => pp.PersonId == person.Id).Select(pp => pp.PartyId).Contains(p.Id));

                var joinedPartyDtos = new List<PartyDTO>();

                foreach (var party in joinedParties)
                {
                    var partyDto = new PartyDTO();
                    partyDto.PartyId = party.Id;
                    partyDto.PartyName = party.Name;
                    partyDto.PartyDate = party.Date;
                    partyDto.HostName = db.People.Where(p => p.Id == party.HostId).Select(p => new { HostName = p.FirstName + " " + p.LastName }).First().HostName;
                    partyDto.HasSubmittedBallot = db.Ballots.Any(b => b.VoterId == db.PartyPersons.Where(pp => pp.PersonId == person.Id && pp.PartyId == party.Id).Select(pp => pp.Id).FirstOrDefault());

                    var partyPerson = db.PartyPersons.Where(p => p.PartyId == party.Id && p.PersonId == person.Id).FirstOrDefault();
                    var chef = db.Chefs.Where(c => c.PartyPersonId == partyPerson.Id).FirstOrDefault();

                    if (chef != null)
                    {
                        var chili = db.Chilis.Where(c => c.ChefId == chef.Id && c.PartyId == party.Id).FirstOrDefault();

                        partyDto.EnterChili = chili != null;
                        partyDto.ChiliName = chili.Name;
                        partyDto.ChiliNumber = chili.Number;
                    }
                    else
                    {
                        partyDto.EnterChili = false;
                    }

                    joinedPartyDtos.Add(partyDto);
                }

                personDto.JoinedParties = joinedPartyDtos;
            }

            return Ok(personDto);
        }

        // GET: api/Party/5
        [ResponseType(typeof(Party))]
        public IHttpActionResult GetParty(Guid id)
        {
            Party party = db.Parties.Find(id);
            if (party == null)
            {
                return NotFound();
            }

            return Ok(party);
        }

        // PUT: api/Party/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutParty(Guid id, Party party)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != party.Id)
            {
                return BadRequest();
            }

            db.Entry(party).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PartyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Party
        [ResponseType(typeof(PartyDTO))]
        public IHttpActionResult PostParty(PartyDTO partyDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            var userId = Guid.Parse(userIdString);

            var person = db.People.Where(p => p.UserId == userId).FirstOrDefault();

            if (person != null)
            {
                var party = db.Parties.Where(p => p.Id == partyDto.PartyId).FirstOrDefault();

                if (party != null)
                {
                    if (partyDto.PartyDate != null)
                        party.Date = partyDto.PartyDate;

                    if (partyDto.PartyName != null)
                    {
                        party.Name = partyDto.PartyName;
                    }

                    if(partyDto.MaxChilis > 0)
                    {
                        party.MaxChilis = partyDto.MaxChilis;
                    }

                    party.HasVotingStarted = partyDto.HasVotingStarted;

                    // If votes haven't been tallied yet but the host initiated it, tally the results
                    if (!party.HaveVotesBeenTallied && partyDto.HaveVotesBeenTallied)
                    {
                        var chiliPoints = new Dictionary<Guid, int>();
                        db.Votes.Where(v => db.Ballots.Where(b => db.PartyPersons.Where(pp => pp.PartyId == party.Id).Select(pp => pp.Id).Contains(b.VoterId)).Select(b => b.Id).Contains(v.BallotId)).GroupBy(v => v.ChiliId).ToList().ForEach(g => chiliPoints.Add(g.First().ChiliId, g.Sum(v => v.Points)));
                        chiliPoints.Keys.ToList().ForEach(k => db.Chilis.Where(c => c.Id == k).FirstOrDefault().Votes = chiliPoints[k]);

                        db.SaveChanges();

                        var partyChilis = db.Chilis.Where(c => c.PartyId == party.Id).OrderByDescending(c => c.Votes).ToList();

                        // Check for tie for first place
                        var firstPlaceChilis = partyChilis.Where(pc => pc.Votes == partyChilis.Max(p => p.Votes));

                        if(firstPlaceChilis.Count() > 1)
                        {
                            // See if any one of the first place chilis has more first place votes (points == 3)
                            var chilisAndTheirNumberOfFirstPlaceVotes = new Dictionary<Guid, int>();

                            var firstPlaceChiliIds = firstPlaceChilis.Select(f => f.Id);

                            db.Votes.Where(v => firstPlaceChiliIds.Contains(v.ChiliId) && v.Points == 3).GroupBy(v => v.ChiliId).ToList().ForEach(g => chilisAndTheirNumberOfFirstPlaceVotes.Add(g.First().ChiliId, g.Count()));
                            
                            if(chilisAndTheirNumberOfFirstPlaceVotes.Count > 1)
                            {
                                // if there's more than one first place chili that got first place votes, see if one has more than others
                                if (chilisAndTheirNumberOfFirstPlaceVotes.Count(c => c.Value == chilisAndTheirNumberOfFirstPlaceVotes.Max(ch => ch.Value)) > 1)
                                {
                                    // If still tied, see if any one chili has more second place votes (points == 2)
                                    var chilisAndTheirNumberOfSecondPlaceVotes = new Dictionary<Guid, int>();

                                    db.Votes.Where(v => firstPlaceChiliIds.Contains(v.ChiliId) && v.Points == 2).GroupBy(v => v.ChiliId).ToList().ForEach(g => chilisAndTheirNumberOfSecondPlaceVotes.Add(g.First().ChiliId, g.Count()));

                                    if (chilisAndTheirNumberOfSecondPlaceVotes.Count > 1)
                                    {
                                        // if there's more than one first place chili that got second place votes, see if one has more than others
                                        if (chilisAndTheirNumberOfSecondPlaceVotes.Count(c => c.Value == chilisAndTheirNumberOfSecondPlaceVotes.Max(ch => ch.Value)) > 1)
                                        {
                                            // If still tied, well there's a first place tie then
                                            partyChilis.Where(p => chilisAndTheirNumberOfSecondPlaceVotes.Where(c => c.Value == chilisAndTheirNumberOfSecondPlaceVotes.Max(ch => ch.Value)).Select(c => c.Key).Contains(p.Id)).ToList().ForEach(ch => ch.Place = 1);
                                        }
                                        else
                                            partyChilis.Where(p => p.Id == chilisAndTheirNumberOfSecondPlaceVotes.OrderByDescending(c => c.Value).Select(c => c.Key).First()).First().Place = 1;
                                    }
                                    else
                                        partyChilis.Where(p => p.Id == chilisAndTheirNumberOfSecondPlaceVotes.OrderByDescending(c => c.Value).Select(c => c.Key).First()).First().Place = 1;

                                }
                                else
                                    partyChilis.Where(p => p.Id == chilisAndTheirNumberOfFirstPlaceVotes.OrderByDescending(c => c.Value).Select(c => c.Key).First()).First().Place = 1;
                            }
                            else
                                partyChilis.Where(p => p.Id == chilisAndTheirNumberOfFirstPlaceVotes.OrderByDescending(c => c.Value).Select(c => c.Key).First()).First().Place = 1;

                            //Reorder partyChilis by place then by votes
                            partyChilis = partyChilis.Where(c => c.PartyId == party.Id).OrderByDescending(c => c.Place).OrderByDescending(c => c.Votes).ToList();
                        }

                        for (var i = 0; i < partyChilis.Count; i++)
                        {
                            if(partyChilis[i].Place == null)
                                partyChilis[i].Place = i+1;
                        }                        
                    }

                    party.HaveVotesBeenTallied = partyDto.HaveVotesBeenTallied;

                    var partyPerson = db.PartyPersons.Where(pp => pp.PartyId == party.Id && pp.PersonId == person.Id).FirstOrDefault();

                    if (!partyDto.IsHostUpdate)
                    {
                        if (partyPerson != null)
                        {
                            var chef = db.Chefs.Where(c => c.PartyPersonId == partyPerson.Id).FirstOrDefault();

                            if (chef != null)
                            {
                                var chili = db.Chilis.Where(c => c.ChefId == chef.Id && c.PartyId == party.Id).FirstOrDefault();

                                if (chili != null)
                                {
                                    if (partyDto.EnterChili.HasValue && partyDto.EnterChili.Value)
                                    {
                                        if (chili != null)
                                        {
                                            chili.Name = partyDto.ChiliName;

                                            partyDto.ChiliNumber = chili.Number;
                                        }
                                    }
                                    else
                                    {
                                        db.Chilis.Remove(chili);
                                        db.Chefs.Remove(chef);
                                    }
                                }
                            }
                            else if (partyDto.EnterChili.HasValue && partyDto.EnterChili.Value)
                            {
                                var newChef = new Chef { Id = Guid.NewGuid(), PartyPersonId = partyPerson.Id };

                                db.Chefs.Add(newChef);

                                var chiliNumber = 0;

                                var numberOfChilis = db.Chilis.Count(c => c.PartyId == party.Id);

                                if (numberOfChilis >= party.MaxChilis)
                                    chiliNumber = numberOfChilis + 1;
                                else
                                {
                                    chiliNumber = randomNumberGenerator.Next(1, party.MaxChilis+1);

                                    while (db.Chilis.Where(c => c.Number == chiliNumber && c.PartyId == party.Id).FirstOrDefault() != null)
                                    {
                                        chiliNumber = randomNumberGenerator.Next(1, party.MaxChilis+1);
                                    }
                                }

                                var chili = new Chili { Id = Guid.NewGuid(), ChefId = newChef.Id, Name = partyDto.ChiliName, Number = chiliNumber, PartyId = party.Id };

                                db.Chilis.Add(chili);

                                partyDto.ChiliNumber = chiliNumber;
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }

            return Ok(partyDto);
        }

        // POST: api/Party/Host
        [ResponseType(typeof(PartyDTO))]
        [Route("Host")]
        public IHttpActionResult HostParty(PartyDTO partyDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            var userId = Guid.Parse(userIdString);

            var person = db.People.Where(p => p.UserId == userId).FirstOrDefault();

            var returnedPartyDto = new PartyDTO();

            var partyCode = Utilities.Utilities.GeneratePartyCode(4);

            while (db.Parties.Where(p => p.Code == partyCode).FirstOrDefault() != null)
            {
                partyCode = Utilities.Utilities.GeneratePartyCode(4);
            }

            var party = new Party { Id = Guid.NewGuid(), Name = partyDto.PartyName, Date = partyDto.PartyDate, Code = partyCode, HostId = person.Id, MaxChilis = partyDto.MaxChilis };

            db.Parties.Add(party);

            returnedPartyDto.PartyCode = partyCode;

            var partyPerson = new PartyPerson { Id = Guid.NewGuid(), PartyId = party.Id, PersonId = person.Id };

            db.PartyPersons.Add(partyPerson);

            if (partyDto.EnterChili.HasValue && partyDto.EnterChili.Value)
            {
                var chef = new Chef { Id = Guid.NewGuid(), PartyPersonId = partyPerson.Id };

                db.Chefs.Add(chef);

                var chiliNumber = 0;

                var numberOfChilis = db.Chilis.Count(c => c.PartyId == party.Id);

                if (numberOfChilis >= party.MaxChilis)
                    chiliNumber = numberOfChilis + 1;
                else
                {
                    chiliNumber = randomNumberGenerator.Next(1, party.MaxChilis+1);

                    while (db.Chilis.Where(c => c.Number == chiliNumber && c.PartyId == party.Id).FirstOrDefault() != null)
                    {
                        chiliNumber = randomNumberGenerator.Next(1, party.MaxChilis+1);
                    }
                }

                var chili = new Chili { Id = Guid.NewGuid(), ChefId = chef.Id, Name = partyDto.ChiliName, Number = chiliNumber, PartyId = party.Id };

                db.Chilis.Add(chili);

                returnedPartyDto.ChiliNumber = chiliNumber;
            }

            db.SaveChanges();

            return Ok(returnedPartyDto);
        }

        // DELETE: api/Party/5
        [ResponseType(typeof(Party))]
        public IHttpActionResult DeleteParty(Guid id)
        {
            Party party = db.Parties.Find(id);
            if (party == null)
            {
                return NotFound();
            }

            db.Parties.Remove(party);
            db.SaveChanges();

            return Ok(party);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PartyExists(Guid id)
        {
            return db.Parties.Count(e => e.Id == id) > 0;
        }
    }
}