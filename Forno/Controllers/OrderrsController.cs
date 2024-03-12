using Forno.Models;
using System.Collections.Generic;
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
        public ActionResult Index()
        {
            var orderr = db.Orderr.Include(o => o.AppUser);
            return View(orderr.ToList());
        }

        // GET: Orderrs/Details/5
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeStatus(int id, string newStatus)
        {
            var orderr = db.Orderr.Find(id);
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
