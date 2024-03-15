using Forno.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Frelsex.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(MyLogin model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                using (ModelDbContext db = new ModelDbContext())
                {
                    AppUser user = db.AppUser.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

                    if (user != null)
                    {
                        FormsAuthentication.SetAuthCookie(user.Username, false);
                        // Ottieni i ruoli per l'utente
                        string role = user.Role;

                        // Combina l'ID dell'utente e i ruoli in una stringa di UserData
                        string userData = $"{user.AppUserID}|{role}";

                        // Crea il ticket di autenticazione
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                            1, // version
                            user.Username, // username
                            DateTime.Now, // issue date
                            DateTime.Now.AddMinutes(30), // expiration
                            false, // persistent
                            userData, // user data
                            FormsAuthentication.FormsCookiePath);

                        string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                        HttpContext.Response.Cookies.Add(authCookie);

                        TempData["success"] = "Login successful.";

                        // Usa il ruolo recuperato dall'utente per decidere il reindirizzamento
                        if (role == "Admin") // Assicurati che il valore "Admin" sia esatto come nel database
                        {
                            return Json(new { success = true, redirectUrl = Url.Action("DailyReport", "Orderrs") });
                        }
                        else
                        {
                            return Json(new { success = true, redirectUrl = Url.Action("Index", "Products") });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Tentativo di accesso non valido.");
                    }
                }
            }

            // Se siamo arrivati fin qui, qualcosa è fallito, quindi ri-mostra il form
            return View(model);
        }



        // Metodo per il Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            TempData["success"] = "Logout successful.";
            return RedirectToAction("Login", "Account");
        }




        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

    }
}