using Forno.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Forno.Controllers
{
    public class OrderrsController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Orderrs
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult Index()
        {
            var orderr = db.Orderr.Include(o => o.AppUser);
            return View(orderr.ToList());
        }

        // GET: Orderrs/Details/5
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Include OrderDetail e Product per ottenere i dettagli dei prodotti dell'ordine
            Orderr orderr = db.Orderr
                .Include(o => o.OrderDetail.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (orderr == null)
            {
                return HttpNotFound();
            }
            return View(orderr);
        }



        // GET: Orderrs/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var products = db.Product.ToList();
            ViewBag.Products = products;
            ViewBag.AppUserID = new SelectList(db.AppUser, "AppUserID", "Username");
            return View();
        }

        // POST: Orderrs/Create
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding.
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderID,AppUserID,Status,OrderDate")] Orderr orderr, List<int> selectedProductIds, List<int> quantities)
        {

            if (ModelState.IsValid)
            {
                orderr.Status = "In Preparazione";
                decimal totalPrice = 0;
                var orderDetails = new List<OrderDetail>();

                for (int i = 0; i < selectedProductIds.Count; i++)
                {
                    var product = db.Product.Find(selectedProductIds[i]);
                    if (product != null)
                    {
                        var detail = new OrderDetail
                        {
                            ProductID = product.ProductID,
                            Quantity = quantities[i],
                            // prezzo modificato o sconti aggiungerli qui
                        };

                        orderDetails.Add(detail);
                        totalPrice += product.Price * quantities[i];
                    }
                }

                orderr.TotalPrice = totalPrice;
                orderr.OrderDetail = orderDetails; // Assumendo che OrderDetail sia una lista nel tuo modello Orderr

                db.Orderr.Add(orderr);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AppUserID = new SelectList(db.AppUser, "AppUserID", "Username", orderr.AppUserID);
            ViewBag.Products = db.Product.ToList();
            return View(orderr);
        }


        // GET: Orderrs/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var orderr = db.Orderr.Include(o => o.OrderDetail.Select(od => od.Product.Ingredient))
                                   .FirstOrDefault(o => o.OrderID == id);
            if (orderr == null)
            {
                return HttpNotFound();
            }

            // Prendi i dettagli dell'ordine per questo specifico ordine.
            var orderDetails = orderr.OrderDetail.ToList();

            // Costruisci una mappa che associa ogni ProductID ai suoi IngredientID.
            var productToIngredientIdsMap = orderDetails
                .SelectMany(od => od.Product.Ingredient.Select(i => new { od.ProductID, i.IngredientID }))
                .GroupBy(x => x.ProductID)
                .ToDictionary(g => g.Key, g => g.Select(x => x.IngredientID).ToArray());

            ViewBag.AppUserID = new SelectList(db.AppUser, "AppUserID", "Username", orderr.AppUserID);
            ViewBag.ProductToIngredientIdsMap = productToIngredientIdsMap;
            ViewBag.AllProducts = new SelectList(db.Product, "ProductID", "Name");
            ViewBag.AllIngredients = db.Ingredient.ToList();

            return View(orderr);
        }


        // POST: Orderrs/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "OrderID,AppUserID,OrderDate,Status,TotalPrice")] Orderr orderr)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderr).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AppUserID = new SelectList(db.AppUser, "AppUserID", "Username", orderr.AppUserID);
            return View(orderr);
        }

        // GET: Orderrs/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Include OrderDetail e Product per ottenere i dettagli dei prodotti dell'ordine
            Orderr orderr = db.Orderr
                .Include(o => o.OrderDetail.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (orderr == null)
            {
                return HttpNotFound();
            }
            return View(orderr);
        }

        // POST: Orderrs/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Orderr orderr = db.Orderr
                .Include(o => o.OrderDetail)
                .FirstOrDefault(o => o.OrderID == id);

            // Prima di rimuovere l'ordine, è necessario rimuovere anche i dettagli dell'ordine
            foreach (var detail in orderr.OrderDetail.ToList())
            {
                db.Entry(detail).State = EntityState.Deleted;
            }

            db.Entry(orderr).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // metodo per cambiare lo stato dell'ordine
        // POST: Orderrs/ChangeStatus/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeStatus(int id, string newStatus)
        {
            Orderr orderr = db.Orderr.Find(id);
            if (orderr != null)
            {
                orderr.Status = newStatus;
                db.Entry(orderr).State = EntityState.Modified;
                db.SaveChanges();
                // Restituisci un risultato semplice per la richiesta AJAX
                return Json(new { success = true, status = newStatus });
            }
            // In caso di errore restituisci un messaggio di errore
            return Json(new { success = false, message = "Order not found." });
        }


        // Metodo per ottenere i dettagli dell'ordine
        // GET: Orderrs/GetOrderDetails/5

        public ActionResult GetOrderDetails(int id)
        {
            List<OrderDetail> orderDetails = db.OrderDetail
                .Include(od => od.Product)
                .Where(od => od.OrderrID == id)
                .ToList();
            return PartialView("_OrderDetails", orderDetails);
        }

        // Metodo per mlodificare i dettagli dell'ordine
        // POST: Orderrs/EditOrderDetails/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditOrderDetails(int id, List<int> productIds, List<int> quantities, List<string> modifiedIngredients)
        {
            Orderr order = db.Orderr.Find(id);
            if (order != null)
            {
                List<OrderDetail> orderDetails = order.OrderDetail.ToList();
                for (int i = 0; i < productIds.Count; i++)
                {
                    var detail = orderDetails.FirstOrDefault(od => od.ProductID == productIds[i]);
                    if (detail != null)
                    {
                        detail.Quantity = quantities[i];
                        detail.ModifiedIngredients = modifiedIngredients[i];
                        db.Entry(detail).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Order not found." });
        }

        // Metodo per il carrello
        // GET: Orderrs/GetCart
        public ActionResult GetCart()
        {
            // Recupera l'ID dell'utente corrente
            int appUserId = 1; // Sostituire con il vero ID dell'utente corrente
            // Recupera l'ordine corrente dell'utente
            Orderr cart = db.Orderr
                .Include(o => o.OrderDetail.Select(od => od.Product))
                .FirstOrDefault(o => o.AppUserID == appUserId && o.Status == "In Preparazione");
            if (cart == null)
            {
                cart = new Orderr
                {
                    AppUserID = appUserId,
                    Status = "In Preparazione",
                    OrderDate = DateTime.Now
                };
                db.Orderr.Add(cart);
                db.SaveChanges();
            }
            return View(cart);
        }

        // Metodo per aggiungere un prodotto al carrello
        // POST: Orderrs/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int quantity)
        {
            // Recupera l'ID dell'utente corrente
            int appUserId = 1; // Sostituire con il vero ID dell'utente corrente
            // Recupera l'ordine corrente dell'utente
            Orderr cart = db.Orderr
                .Include(o => o.OrderDetail)
                .FirstOrDefault(o => o.AppUserID == appUserId && o.Status == "In Preparazione");
            if (cart == null)
            {
                cart = new Orderr
                {
                    AppUserID = appUserId,
                    Status = "In Preparazione",
                    OrderDate = DateTime.Now
                };
                db.Orderr.Add(cart);
            }
            // Aggiungi il prodotto all'ordine
            OrderDetail detail = cart.OrderDetail.FirstOrDefault(od => od.ProductID == productId);
            if (detail == null)
            {
                detail = new OrderDetail
                {
                    ProductID = productId,
                    Quantity = quantity
                };
                cart.OrderDetail.Add(detail);
            }
            else
            {
                detail.Quantity += quantity;
            }
            db.SaveChanges();
            return RedirectToAction("GetCart");
        }

        // metodo per prendere l'id dell'utente corrente
        private int GetCurrentUserId()
        {

            return 1; // Sostituire con il vero ID dell'utente corrente
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
