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

namespace ChiliCookoff2.Controllers
{
    public class BallotController : ApiController
    {
        private ChiliCookoffEntities db = new ChiliCookoffEntities();

        // GET: api/Ballot
        public IQueryable<Ballot> GetBallots()
        {
            return db.Ballots;
        }

        // GET: api/Ballot/5
        [ResponseType(typeof(Ballot))]
        public IHttpActionResult GetBallot(Guid id)
        {
            Ballot ballot = db.Ballots.Find(id);
            if (ballot == null)
            {
                return NotFound();
            }

            return Ok(ballot);
        }

        // PUT: api/Ballot/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutBallot(Guid id, Ballot ballot)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != ballot.Id)
            {
                return BadRequest();
            }

            db.Entry(ballot).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BallotExists(id))
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

        // POST: api/Ballot
        [ResponseType(typeof(Ballot))]
        public IHttpActionResult PostBallot(BallotDTO ballotDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure party is valid and that the host has started the voting process
            var party = db.Parties.Where(p => p.Id == ballotDto.PartyId).FirstOrDefault();

            if(party == null)
            {
                ModelState.AddModelError("Party", "Party Not Found.");
                return BadRequest(ModelState);
            }
            else if (!party.HasVotingStarted)
            {
                ModelState.AddModelError("Party", "The host must officially start the voting process before you can cast your ballot and has not done so yet.");
                return BadRequest(ModelState);
            }

            var userIdString = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            var userId = Guid.Parse(userIdString);

            var person = db.People.Where(p => p.UserId == userId).FirstOrDefault();

            if (person != null)
            {
                var partyPerson = db.PartyPersons.Where(pp => pp.PersonId == person.Id && pp.PartyId == ballotDto.PartyId).FirstOrDefault();

                if (partyPerson != null)
                {
                    if (!db.Ballots.Any(b => b.VoterId == partyPerson.Id))
                    {
                        var ballot = new Ballot { Id = Guid.NewGuid(), VoterId = partyPerson.Id };

                        db.Ballots.Add(ballot);

                        foreach (var vote in ballotDto.Votes)
                        {
                            vote.Id = Guid.NewGuid();
                            vote.BallotId = ballot.Id;

                            db.Votes.Add(vote);
                        }

                        db.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("PartyPerson", "You've already submitted your ballot for this party!  Only one ballot per person per party.");
                        return BadRequest(ModelState);
                    }
                }
            }

            return Ok();
        }

        // DELETE: api/Ballot/5
        [ResponseType(typeof(Ballot))]
        public IHttpActionResult DeleteBallot(Guid id)
        {
            Ballot ballot = db.Ballots.Find(id);
            if (ballot == null)
            {
                return NotFound();
            }

            db.Ballots.Remove(ballot);
            db.SaveChanges();

            return Ok(ballot);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BallotExists(Guid id)
        {
            return db.Ballots.Count(e => e.Id == id) > 0;
        }
    }
}