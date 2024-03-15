using Forno.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Forno.Controllers
{
    public class AppUsersController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: AppUsers
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.AppUser.ToList());
        }

        // GET: AppUsers/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser appUser = db.AppUser.Find(id);
            if (appUser == null)
            {
                return HttpNotFound();
            }
            return View(appUser);
        }

        // GET: AppUsers/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: AppUsers/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "AppUserID,Username,Password,ShippingAddress,Note")] AppUser appUser)
        {
            ModelState.Remove("Role");
            appUser.Role = "Utente";

            if (ModelState.IsValid)
            {
                db.AppUser.Add(appUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                // Log degli errori di ModelState
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToList() })
                    .ToList();

                // Registrare nel log di sistema o in un file
                foreach (var entry in errors)
                {
                    foreach (var error in entry.Errors)
                    {
                        // Utilizza il tuo sistema di logging preferito qui
                        System.Diagnostics.Debug.WriteLine($"Errore nel campo {entry.Key}: {error}");
                    }
                }
            }

            return View(appUser);
        }


        // GET: AppUsers/Edit/5
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser appUser = db.AppUser.Find(id);
            if (appUser == null)
            {
                return HttpNotFound();
            }
            return View(appUser);
        }

        // POST: AppUsers/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult Edit([Bind(Include = "AppUserID,Username,Password,ShippingAddress,Note")] AppUser appUser)
        {
            if (ModelState.IsValid)
            {
                var userToUpdate = db.AppUser.Find(appUser.AppUserID);

                if (userToUpdate != null)
                {
                    userToUpdate.Username = appUser.Username;
                    userToUpdate.Password = appUser.Password;
                    userToUpdate.ShippingAddress = appUser.ShippingAddress;
                    userToUpdate.Note = appUser.Note;
                    db.Entry(userToUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(appUser);
        }

        // GET: AppUsers/Delete/5
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser appUser = db.AppUser.Find(id);
            if (appUser == null)
            {
                return HttpNotFound();
            }
            return View(appUser);
        }

        // POST: AppUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            AppUser appUser = db.AppUser.Find(id);
            db.AppUser.Remove(appUser);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: AppUsers/ChangeRole/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeRole(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AppUser appUser = db.AppUser.Find(id);
            if (appUser == null)
            {
                return HttpNotFound();
            }

            // Cambio del ruolo
            if (appUser.Role.Equals("Utente", StringComparison.InvariantCultureIgnoreCase))
            {
                appUser.Role = "Admin";
            }
            else if (appUser.Role.Equals("Admin", StringComparison.InvariantCultureIgnoreCase))
            {
                appUser.Role = "Utente";
            }

            db.Entry(appUser).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
