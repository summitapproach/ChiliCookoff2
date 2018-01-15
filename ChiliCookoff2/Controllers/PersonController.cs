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
    [RoutePrefix("api/Person")]
    public class PersonController : ApiController
    {
        private ChiliCookoffEntities db = new ChiliCookoffEntities();
        private Random randomNumberGenerator = new Random();

        // GET: api/Person
        [ResponseType(typeof(PersonDTO))]
        public IHttpActionResult GetPersonByUserId()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var userIdString = Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity);
            var userId = Guid.Parse(userIdString);

            var person = db.People.Where(p => p.UserId == userId).FirstOrDefault();
            var user = db.AspNetUsers.Where(u => u.Id == userIdString).FirstOrDefault();
            var personDto = new PersonDTO();

            if (person != null && user != null)
            {
                personDto.EnterChili = false;
                personDto.FirstName = person.FirstName;
                personDto.LastName = person.LastName;
                personDto.EmailAddress = user.Email;  //Microsoft.AspNet.Identity.IdentityExtensions.GetUserName(User.Identity);
            }

            return Ok(personDto);
        }

        // GET: api/Person/5
        [ResponseType(typeof(Person))]
        public IHttpActionResult GetPerson(Guid id)
        {
            Person person = db.People.Find(id);
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        // PUT: api/Person/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPerson(Guid id, Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != person.Id)
            {
                return BadRequest();
            }

            db.Entry(person).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
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

        // POST: api/Person/Join
        [ResponseType(typeof(PartyDTO))]
        [Route("Join")]
        public IHttpActionResult JoinParty(PersonDTO personDto)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = Guid.Parse(Microsoft.AspNet.Identity.IdentityExtensions.GetUserId(User.Identity));
            
            var party = db.Parties.Where(p => p.Code == personDto.PartyCode).FirstOrDefault();

            var partyDto = new PartyDTO();

            if(party == null)
            {
                ModelState.AddModelError("PartyCode", string.Format("Party Code '{0}' is not a valid party code.  Confirm the party code with your party host.", personDto.PartyCode));
                return BadRequest(ModelState);
            }

            partyDto.PartyName = party.Name;

            var person = db.People.Where(p => p.UserId == userId).FirstOrDefault();

            if(db.PartyPersons.Where(p => p.PartyId == party.Id && p.PersonId == person.Id).FirstOrDefault() != null)
            {
                ModelState.AddModelError("PartyPerson", string.Format("You've already joined this party!  Go to your Account Profile page to manage your info.", personDto.PartyCode));
                return BadRequest(ModelState);
            }

            var partyPerson = new PartyPerson { Id = Guid.NewGuid(), PartyId = party.Id, PersonId = person.Id };
            db.PartyPersons.Add(partyPerson);

            if (personDto.EnterChili)
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

                var chili = new Chili { Id = Guid.NewGuid(), ChefId = chef.Id, Name = personDto.ChiliName, Number = chiliNumber, PartyId = party.Id };
                db.Chilis.Add(chili);
                
                partyDto.ChiliNumber = chiliNumber;
            }

            db.SaveChanges();

            return Ok(partyDto);
        }

        // POST: api/Person
        [ResponseType(typeof(PersonDTO))]
        public IHttpActionResult PostPerson(PersonDTO personDto)
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

            // if a different user already used this email address, return error
            if(db.AspNetUsers.Any(u => u.Email == personDto.EmailAddress && u.Id != userIdString))
            {
                ModelState.AddModelError("EmailAddress", string.Format("Email '{0}' is already taken.", personDto.EmailAddress));
                return BadRequest(ModelState);
            }

            var user = db.AspNetUsers.Where(u => u.Id == userIdString).FirstOrDefault();

            if(user != null)
            {
                user.Email = user.UserName = personDto.EmailAddress;
            }
            else
            {
                ModelState.AddModelError("User", "User not found.");
                return BadRequest(ModelState);
            }

            var userId = Guid.Parse(userIdString);
            var person = db.People.Where(p => p.UserId == userId).FirstOrDefault();

            if(person != null)
            {
                person.FirstName = personDto.FirstName;
                person.LastName = personDto.LastName;          
            }
            else
            {
                ModelState.AddModelError("Person", "Person not found.");
                return BadRequest(ModelState);
            }

            //var chef = db.Chefs.Where(c => c.PersonId == person.Id).FirstOrDefault();

            //if(personDto.EnterChili)
            //{
            //    if (chef == null)
            //    {
            //        chef = new Chef { Id = Guid.NewGuid(), PersonId = person.Id };
            //        db.Chefs.Add(chef);
            //    }

            //    var chili = db.Chilis.Where(c => c.ChefId == chef.Id).FirstOrDefault();

            //    if(chili == null)
            //    {
            //        var chiliNumber = randomNumberGenerator.Next(minChiliValue, maxChiliValue);

            //        while (db.Chilis.Where(c => c.Number == chiliNumber).FirstOrDefault() != null)
            //        {
            //            chiliNumber = randomNumberGenerator.Next(minChiliValue, maxChiliValue);
            //        }

            //        chili = new Chili { Id = Guid.NewGuid(), ChefId = chef.Id, Name = string.IsNullOrWhiteSpace(personDto.ChiliName) ? "No Name" : personDto.ChiliName, Number = chiliNumber };
            //        db.Chilis.Add(chili);
            //    }
            //    else
            //        chili.Name = personDto.ChiliName;
            //}
            //else
            //{
            //    if(chef != null)
            //    {
            //        db.Chefs.Remove(chef);

            //        var chili = db.Chilis.Where(c => c.ChefId == chef.Id).FirstOrDefault();

            //        if (chili != null)
            //            db.Chilis.Remove(chili);
            //    }
            //}

            db.SaveChanges();

            return Ok(personDto);
        }

        // DELETE: api/Person/5
        [ResponseType(typeof(Person))]
        public IHttpActionResult DeletePerson(Guid id)
        {
            Person person = db.People.Find(id);
            if (person == null)
            {
                return NotFound();
            }

            db.People.Remove(person);
            db.SaveChanges();

            return Ok(person);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PersonExists(Guid id)
        {
            return db.People.Count(e => e.Id == id) > 0;
        }
    }
}