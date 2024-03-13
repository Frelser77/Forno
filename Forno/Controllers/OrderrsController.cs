using Forno.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;

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
        public ActionResult Create(Orderr order, List<int> selectedProductIds, List<int> quantities)
        {
            if (ModelState.IsValid)
            {
                order.Status = "In Lavorazione";
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
                            // Non hai il campo Price qui, quindi lo salteremo
                        };

                        orderDetails.Add(detail);
                        totalPrice += product.Price * quantities[i]; // Calcolo del totale qui
                    }
                }

                order.TotalPrice = totalPrice; // Impostazione del totale calcolato
                order.OrderDetail = orderDetails;

                db.Orderr.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            PrepareViewBag(); // Preparazione della ViewBag per la vista in caso di fallimento
            return View(order);
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
        public ActionResult Edit(Orderr order)
        {
            if (ModelState.IsValid)
            {
                // Usare db.Entry(order).State = EntityState.Modified solo se si è sicuri che tutti i campi dell'entità siano affidabili
                // Altrimenti, recuperare l'entità dal database e aggiornarla manualmente
                var existingOrder = db.Orderr.Include(o => o.OrderDetail).FirstOrDefault(o => o.OrderID == order.OrderID);
                if (existingOrder != null)
                {
                    // Aggiorna qui le proprietà di existingOrder con i valori di order
                    // db.Entry(existingOrder).CurrentValues.SetValues(order);

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            PrepareViewBag(); // Preparazione della ViewBag per la vista
            return View(order);
        }

        private void PrepareViewBag()
        {
            ViewBag.AppUserID = new SelectList(db.AppUser, "AppUserID", "Username");
            ViewBag.Products = db.Product.ToList();
            // Altre preparazioni della ViewBag necessarie
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

        private int GetCurrentUserId()
        {
            if (HttpContext.User.Identity is FormsIdentity identity)
            {
                FormsAuthenticationTicket ticket = identity.Ticket;
                // userData è nel formato "userID|role"
                var userDataParts = ticket.UserData.Split('|');
                if (userDataParts.Length > 0)
                {
                    return int.Parse(userDataParts[0]); // Il primo elemento è l'userID
                }
            }
            return -1; // oppure null o un'altra convenzione per indicare che l'utente non è autenticato
        }

        private Orderr GetOrCreateCart()
        {
            int? appUserId = GetCurrentUserId();

            if (!appUserId.HasValue || appUserId.Value <= 0)
            {
                throw new UnauthorizedAccessException("L'utente non è loggato.");
            }

            var cart = db.Orderr.FirstOrDefault(o => o.AppUserID == appUserId && o.Status == "In Lavorazione");

            if (cart == null)
            {
                cart = new Orderr
                {
                    AppUserID = appUserId.Value,
                    Status = "In Lavorazione",
                    OrderDate = DateTime.Now
                };
                db.Orderr.Add(cart);
                db.SaveChanges();
            }

            return cart;
        }


        // Metodo per aggiungere un prodotto al carrello
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int productId, int quantity, List<int> selectedIngredients)
        {
            var cart = GetOrCreateCart();
            var product = db.Product.Find(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Prodotto non trovato." });
            }

            var existingDetail = cart.OrderDetail.FirstOrDefault(od => od.ProductID == productId);

            if (existingDetail != null)
            {
                existingDetail.Quantity += quantity;
            }
            else
            {
                var orderDetail = new OrderDetail
                {
                    OrderrID = cart.OrderID,
                    ProductID = productId,
                    Quantity = quantity
                    // Aggiungi qui altre proprietà se necessario
                };
                db.OrderDetail.Add(orderDetail);
            }

            db.SaveChanges();

            var updatedCartItemCount = cart.OrderDetail.Sum(item => item.Quantity);
            return Json(new
            {
                success = true,
                cartItemCount = updatedCartItemCount
            });
        }

        // Metodo per mostrare il carrello dell'utente
        public ActionResult GetCart()
        {
            var cart = GetOrCreateCart();
            var orderDetails = cart.OrderDetail.Select(od => new CartViewModel
            {
                ProductName = od.Product.Name,
                Quantity = od.Quantity
            }).ToList();

            return View(orderDetails);
        }

        // Metodo per mostrare il numero degli articoli nel carrello
        public ActionResult CartItemCount()
        {
            var cart = GetOrCreateCart();
            var count = cart.OrderDetail.Sum(item => item.Quantity);
            return Json(new { cartItemCount = count }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout()
        {
            var cart = GetOrCreateCart();
            if (!cart.OrderDetail.Any())
            {
                TempData["error"] = "Il tuo carrello è vuoto.";
                return RedirectToAction("Index");
            }

            cart.Status = "Confermato";
            cart.TotalPrice = cart.OrderDetail.Sum(d => d.Quantity * d.Product.Price); // Calcolo del prezzo totale

            // Creazione di un nuovo oggetto Orderr
            var order = new Orderr
            {
                OrderDetail = cart.OrderDetail.ToList(),
                // Altri campi di Orderr, se necessario
            };

            // Salvataggio dell'ordine nel database
            db.Orderr.Add(order);
            db.SaveChanges();

            TempData["success"] = "Il checkout è stato completato con successo.";
            var result = new { Success = true, OrderId = order.OrderID };
            return Json(result);
        }

        // Metodo per mostrare la conferma dell'ordine
        public ActionResult OrderConfirmation(int orderId)
        {
            var order = db.Orderr.FirstOrDefault(o => o.OrderID == orderId && o.Status == "Confermato");
            if (order == null)
            {
                return HttpNotFound();
            }

            // Esegui il binding dei dati necessari per la vista di conferma
            return View(order);
        }

        // Metodo per rimuovere un prodotto dal carrello
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int orderDetailId)
        {
            var orderDetail = db.OrderDetail.Find(orderDetailId);
            if (orderDetail == null)
            {
                return Json(new { success = false, message = "Dettaglio dell'ordine non trovato." });
            }

            db.OrderDetail.Remove(orderDetail);
            db.SaveChanges();

            var cartItemCount = GetOrCreateCart().OrderDetail.Sum(item => item.Quantity);
            return Json(new { success = true, cartItemCount });
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
