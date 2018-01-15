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
    [RoutePrefix("api/Chili")]
    public class ChiliController : ApiController
    {
        private ChiliCookoffEntities db = new ChiliCookoffEntities();

        // GET: api/Chili
        public IHttpActionResult GetChilis(Guid? partyId = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userIdString = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            var userId = Guid.Parse(userIdString);

            var person = db.People.Where(p => p.UserId == userId).FirstOrDefault();

            var chef = db.Chefs.Where(c => c.PartyPersonId == db.PartyPersons.Where(p => p.PartyId == partyId.Value && p.PersonId == person.Id).Select(p => p.Id).FirstOrDefault()).FirstOrDefault();

            if(chef == null)
                return Ok(db.Chilis.Where(c => c.PartyId == partyId.Value).OrderBy(c => c.Number));
            else
                return Ok(db.Chilis.Where(c => c.PartyId == partyId.Value && c.ChefId != chef.Id).OrderBy(c => c.Number));
        }

        // GET: api/Chili/Results
        [ResponseType(typeof(List<ChiliDTO>))]
        [Route("Results")]
        public IHttpActionResult GetChiliResults(Guid? partyId = null)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure that the host has tallied the votes for the party
            var party = db.Parties.Where(p => p.Id == partyId.Value).FirstOrDefault();

            if (party == null)
            {
                ModelState.AddModelError("Party", "Party Not Found.");
                return BadRequest(ModelState);
            }
            else if (!party.HaveVotesBeenTallied)
            {
                ModelState.AddModelError("Party", "The host must officially start the tallying process before you can view the results and has not done so yet.");
                return BadRequest(ModelState);
            }

            List<ChiliDTO> chilis = new List<ChiliDTO>();

            db.Chilis.Where(c => c.PartyId == partyId.Value).ToList().ForEach(p => chilis.Add(
                new ChiliDTO
                {
                    ChiliId = p.Id,
                    Place = p.Place.Value,
                    ChefName = db.People.Where(pe => pe.Id == db.PartyPersons.Where(pp => pp.Id == db.Chefs.Where(ch => ch.Id == p.ChefId).Select(ch => ch.PartyPersonId).FirstOrDefault()).Select(pp => pp.PersonId).FirstOrDefault()).Select(pe => pe.FirstName + " " + pe.LastName).First(),
                    ChiliNumber = p.Number,
                    ChiliName = p.Name,
                    TotalPoints = p.Votes,
                    FirstPlaceVotes = db.Votes.Count(v => v.ChiliId == p.Id && v.Points == 3),
                    SecondPlaceVotes = db.Votes.Count(v => v.ChiliId == p.Id && v.Points == 2),
                    ThirdPlaceVotes = db.Votes.Count(v => v.ChiliId == p.Id && v.Points == 1)
                }
            ));

            return Ok(chilis.OrderBy(c => c.Place).OrderByDescending(c => c.TotalPoints).ToList());
        }

        // PUT: api/Chili/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutChili(Guid id, Chili chili)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userId = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != chili.Id)
            {
                return BadRequest();
            }

            db.Entry(chili).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChiliExists(id))
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

        // POST: api/Chili
        [ResponseType(typeof(Chili))]
        public IHttpActionResult PostChili(Chili chili)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Chilis.Add(chili);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (ChiliExists(chili.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = chili.Id }, chili);
        }

        // DELETE: api/Chili/5
        [ResponseType(typeof(Chili))]
        public IHttpActionResult DeleteChili(Guid id)
        {
            Chili chili = db.Chilis.Find(id);
            if (chili == null)
            {
                return NotFound();
            }

            db.Chilis.Remove(chili);
            db.SaveChanges();

            return Ok(chili);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ChiliExists(Guid id)
        {
            return db.Chilis.Count(e => e.Id == id) > 0;
        }
    }
}