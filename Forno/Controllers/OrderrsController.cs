using Forno.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;

namespace Forno.Controllers
{
    public class OrderrsController : Controller
    {
        private readonly ModelDbContext db = new ModelDbContext();

        // GET: Orderrs
        // GET: Orderrs
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult Index()
        {
            var userId = GetCurrentUserId();

            if (User.IsInRole("Utente"))
            {
                // Per gli utenti, restituisci solo i loro ordini, separati per stato
                ViewBag.ConfirmedOrders = db.Orderr
                                            .Where(o => o.AppUserID == userId && o.Status == "Confermato")
                                            .Include(o => o.AppUser)
                                            .ToList();
                ViewBag.ProcessedOrders = db.Orderr
                                           .Where(o => o.AppUserID == userId && o.Status == "Evaso")
                                           .Include(o => o.AppUser)
                                           .ToList();
            }
            else if (User.IsInRole("Admin"))
            {
                ViewBag.ConfirmedOrders = db.Orderr
                                            .Where(o => o.Status == "Confermato")
                                            .Include(o => o.AppUser)
                                            .ToList();
                ViewBag.ProcessedOrders = db.Orderr
                                           .Where(o => o.Status == "Evaso")
                                           .Include(o => o.AppUser)
                                           .ToList();
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View();
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
                        };

                        orderDetails.Add(detail);
                        totalPrice += product.Price * quantities[i];
                    }
                }

                order.TotalPrice = totalPrice;
                order.OrderDetail = orderDetails;

                db.Orderr.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            PrepareViewBag();
            return View(order);
        }


        // GET: Orderrs/Edit/5
        //[Authorize(Roles = "Admin")]
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    var orderr = db.Orderr.Include(o => o.OrderDetail.Select(od => od.Product.Ingredient))
        //                           .FirstOrDefault(o => o.OrderID == id);
        //    if (orderr == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Prendi i dettagli dell'ordine per questo specifico ordine.
        //    var orderDetails = orderr.OrderDetail.ToList();

        //    // Costruisci una mappa che associa ogni ProductID ai suoi IngredientID.
        //    var productToIngredientIdsMap = orderDetails
        //        .SelectMany(od => od.Product.Ingredient.Select(i => new { od.ProductID, i.IngredientID }))
        //        .GroupBy(x => x.ProductID)
        //        .ToDictionary(g => g.Key, g => g.Select(x => x.IngredientID).ToArray());

        //    ViewBag.AppUserID = new SelectList(db.AppUser, "AppUserID", "Username", orderr.AppUserID);
        //    ViewBag.ProductToIngredientIdsMap = productToIngredientIdsMap;
        //    ViewBag.AllProducts = new SelectList(db.Product, "ProductID", "Name");
        //    ViewBag.AllIngredients = db.Ingredient.ToList();

        //    return View(orderr);
        //}


        // POST: Orderrs/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin")]
        //public ActionResult Edit(Orderr order, Dictionary<int, int[]> productIngredientsMap)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                var existingOrder = db.Orderr.Include(o => o.OrderDetail).FirstOrDefault(o => o.OrderID == order.OrderID);
        //                if (existingOrder != null)
        //                {
        //                    // Assegnazione dei nuovi valori dell'ordine all'ordine esistente
        //                    db.Entry(existingOrder).CurrentValues.SetValues(order);

        //                    // Aggiornare i dettagli dell'ordine
        //                    foreach (var existingDetail in existingOrder.OrderDetail.ToList())
        //                    {
        //                        var orderDetail = order.OrderDetail.SingleOrDefault(od => od.OrderDetailID == existingDetail.OrderDetailID);
        //                        if (orderDetail != null)
        //                        {
        //                            // Aggiorna i dettagli esistenti
        //                            db.Entry(existingDetail).CurrentValues.SetValues(orderDetail);

        //                            // Gestire gli ingredienti modificati per ogni dettaglio dell'ordine
        //                            // Assumendo che orderDetail.ModifiedIngredients sia una stringa contenente gli ID degli ingredienti separati da virgola
        //                            if (productIngredientsMap.ContainsKey(orderDetail.ProductID))
        //                            {
        //                                // Costruisci la stringa degli ID degli ingredienti modificati per il dettaglio corrente
        //                                var modifiedIngredientIds = productIngredientsMap[orderDetail.ProductID];
        //                                existingDetail.ModifiedIngredients = String.Join(",", modifiedIngredientIds);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            // Rimuovi i dettagli dell'ordine che non sono più presenti
        //                            db.OrderDetail.Remove(existingDetail);
        //                        }
        //                    }

        //                    // Aggiungi o aggiorna dettagli dell'ordine
        //                    foreach (var orderDetail in order.OrderDetail)
        //                    {
        //                        var existingDetail = existingOrder.OrderDetail.SingleOrDefault(od => od.OrderDetailID == orderDetail.OrderDetailID);
        //                        if (existingDetail == null)
        //                        {
        //                            // Se è un nuovo dettaglio, aggiungilo
        //                            existingOrder.OrderDetail.Add(orderDetail);
        //                        }
        //                        else
        //                        {
        //                            // Altrimenti, aggiorna il dettaglio esistente
        //                            db.Entry(existingDetail).CurrentValues.SetValues(orderDetail);
        //                        }
        //                    }

        //                    // Aggiornare le associazioni Product-Ingredient
        //                    foreach (var kvp in productIngredientsMap)
        //                    {
        //                        int productId = kvp.Key;
        //                        int[] newIngredientIds = kvp.Value;

        //                        // Rimuovere tutte le associazioni attuali per questo prodotto
        //                        db.Database.ExecuteSqlCommand("DELETE FROM ProductIngredient WHERE ProductID = @ProductID", new SqlParameter("@ProductID", productId));

        //                        // Inserire le nuove associazioni
        //                        foreach (int ingredientId in newIngredientIds)
        //                        {
        //                            db.Database.ExecuteSqlCommand("INSERT INTO ProductIngredient (ProductID, IngredientID) VALUES (@ProductID, @IngredientID)",
        //                            new SqlParameter("@ProductID", productId),
        //                            new SqlParameter("@IngredientID", ingredientId));
        //                        }
        //                    }

        //                    db.SaveChanges();
        //                    dbContextTransaction.Commit();
        //                }
        //                else
        //                {
        //                    dbContextTransaction.Rollback();
        //                    ModelState.AddModelError("", "Ordine non trovato.");
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                dbContextTransaction.Rollback();
        //                ModelState.AddModelError("", "Un errore è avvenuto durante l'aggiornamento del database.");
        //            }
        //        }
        //        return RedirectToAction("Index");
        //    }

        //    PrepareViewBag();
        //    return View(order);
        //}

        // Metodo di supporto per preparare i dati necessari per la vista
        private void PrepareViewBag()
        {
            ViewBag.AppUserID = new SelectList(db.AppUser, "AppUserID", "Username");
            ViewBag.AllProducts = new SelectList(db.Product, "ProductID", "Name");
            ViewBag.AllIngredients = db.Ingredient.ToList();
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
        public ActionResult ChangeStatus(int id)
        {
            var order = db.Orderr.Find(id);
            if (order == null)
            {
                TempData["error"] = "Ordine non trovato.";
                return RedirectToAction("Index");
            }

            if (order.Status == "Evaso")
            {
                TempData["error"] = "Questo ordine è già stato evaso.";
            }
            else if (order.Status == "Confermato")
            {
                order.Status = "Evaso";
                db.SaveChanges();
                TempData["success"] = "Lo stato dell'ordine è stato aggiornato a 'Evaso'.";
            }
            else
            {
                TempData["error"] = "Azione non consentita.";
            }

            return RedirectToAction("Index");
        }




        // Metodo per ottenere i dettagli dell'ordine
        // GET: Orderrs/GetOrderDetails/5

        //public ActionResult GetOrderDetails(int id)
        //{
        //    List<OrderDetail> orderDetails = db.OrderDetail
        //        .Include(od => od.Product)
        //        .Where(od => od.OrderrID == id)
        //        .ToList();
        //    return PartialView("_OrderDetails", orderDetails);
        //}

        //// Metodo per mlodificare i dettagli dell'ordine
        //// POST: Orderrs/EditOrderDetails/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditOrderDetails(int id, List<int> productIds, List<int> quantities, List<string> modifiedIngredients)
        //{
        //    Orderr order = db.Orderr.Find(id);
        //    if (order != null)
        //    {
        //        List<OrderDetail> orderDetails = order.OrderDetail.ToList();
        //        for (int i = 0; i < productIds.Count; i++)
        //        {
        //            var detail = orderDetails.FirstOrDefault(od => od.ProductID == productIds[i]);
        //            if (detail != null)
        //            {
        //                detail.Quantity = quantities[i];
        //                detail.ModifiedIngredients = modifiedIngredients[i];
        //                db.Entry(detail).State = EntityState.Modified;
        //            }
        //        }
        //        db.SaveChanges();
        //        return Json(new { success = true });
        //    }
        //    return Json(new { success = false, message = "Order not found." });
        //}

        [Authorize(Roles = "Utente,Admin")]
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
            return -1;
        }

        // Metodo per ottenere o creare un carrello per l'utente corrente
        [Authorize(Roles = "Utente,Admin")]
        private Orderr GetOrCreateCart()
        {
            int? appUserId = GetCurrentUserId();

            if (!appUserId.HasValue || appUserId.Value <= 0)
            {
                return null;
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
        [Authorize(Roles = "Utente,Admin")]
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
                    Quantity = quantity,
                    ModifiedIngredients = string.Join(",", selectedIngredients)
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
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult GetCart()
        {
            var cart = GetOrCreateCart();

            // Controlla se il carrello è null o se non ha dettagli
            if (cart == null || !cart.OrderDetail.Any())
            {
                // Se il carrello è null o vuoto, ritorna un modello vuoto alla vista
                return View(new List<CartViewModel>());
            }

            // Altrimenti, costruisci il modello da passare alla vista
            var orderDetails = cart.OrderDetail.Select(od => new CartViewModel
            {
                OrderDetailID = od.OrderDetailID,
                ProductName = od.Product.Name,
                Quantity = od.Quantity,
                ImageUrl = od.Product.ImageUrl
            }).ToList();

            return View(orderDetails);
        }

        // Metodo per mostrare il numero degli articoli nel carrello
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult CartItemCount()
        {
            var cart = GetOrCreateCart();
            var count = cart.OrderDetail.Sum(item => item.Quantity);
            return Json(new { cartItemCount = count }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult Checkout()
        {
            try
            {
                var cart = GetOrCreateCart();
                if (!cart.OrderDetail.Any())
                {
                    TempData["error"] = "Il tuo carrello è vuoto.";
                    return RedirectToAction("Index");
                }

                cart.TotalPrice = cart.OrderDetail.Sum(d => d.Quantity * d.Product.Price);

                var order = new Orderr
                {
                    // Assicurati che tutte le proprietà richieste siano assegnate qui
                    OrderDetail = cart.OrderDetail.ToList(),
                    Status = "Confermato",
                    OrderDate = DateTime.Now,
                    AppUserID = cart.AppUserID,
                    TotalPrice = cart.TotalPrice,
                    OrderID = cart.OrderID,
                    AppUser = cart.AppUser,
                };

                db.Orderr.Add(order);
                db.SaveChanges();

                TempData["success"] = "Il checkout è stato completato con successo.";
                var result = new { Success = true, OrderId = order.OrderID };
                return Json(result);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                // Ottieni i dettagli degli errori di validazione
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                // Unisci tutti i messaggi di errore in una stringa; usa per loggare
                var fullErrorMessage = string.Join("; ", errorMessages);
                Debug.WriteLine(fullErrorMessage);
                // Ritorna o reindirizza a una pagina di errore con i messaggi di errore
                TempData["error"] = "Non è stato possibile completare il checkout";
                return RedirectToAction("Index"); // O ritorna un JsonResult con l'errore
            }
        }

        // Metodo per rimuovere un prodotto dal carrello
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult RemoveFromCart(int orderDetailId)
        {
            var orderDetail = db.OrderDetail.Find(orderDetailId);
            if (orderDetail == null)
            {
                return RedirectToAction("Index", "Error");
            }

            db.OrderDetail.Remove(orderDetail);
            db.SaveChanges();

            var cartItemCount = GetOrCreateCart().OrderDetail.Sum(item => item.Quantity);
            return RedirectToAction("GetCart");
        }

        // Metodo per mostrare la conferma dell'ordine
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult OrderConfirmation(int orderId)
        {
            // Recupera l'ordine dal database usando l'ID
            Orderr order = db.Orderr.FirstOrDefault(o => o.OrderID == orderId);

            // Controlla se l'ordine esiste
            if (order == null)
            {
                // Gestisci il caso in cui l'ordine non esiste
                return HttpNotFound("Ordine non trovato.");
            }

            // Passa l'ordine alla vista
            return View(order);
        }

        // Metodo per aumentare la quantità di un prodotto nel carrello
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult IncreaseProductQuantity(int orderDetailId)
        {
            var orderDetail = db.OrderDetail.Find(orderDetailId);
            if (orderDetail != null)
            {
                orderDetail.Quantity++;
                db.SaveChanges();
                // Potresti voler aggiungere una logica qui per gestire quando la quantità supera il magazzino disponibile
            }

            return RedirectToAction("GetCart");
        }

        // Metodo per diminuire la quantità di un prodotto nel carrello
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Utente,Admin")]
        public ActionResult DecreaseProductQuantity(int orderDetailId)
        {
            var orderDetail = db.OrderDetail.Find(orderDetailId);
            if (orderDetail != null)
            {
                // Diminuisci la quantità di 1
                orderDetail.Quantity -= 1;

                // Se la quantità è scesa a 0, rimuovi l'elemento dal carrello
                if (orderDetail.Quantity <= 0)
                {
                    db.OrderDetail.Remove(orderDetail);
                }

                // Salva le modifiche nel database
                db.SaveChanges();
            }

            // Reindirizza l'utente alla vista del carrello
            return RedirectToAction("GetCart");
        }


        // Metodo per svuotare il carrello
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmptyCart()
        {
            var cart = GetOrCreateCart();
            db.OrderDetail.RemoveRange(cart.OrderDetail);
            db.SaveChanges();

            return RedirectToAction("GetCart");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DailyReport()
        {
            // Totale utenti
            ViewBag.TotalUsers = db.AppUser.Count();

            // Totale ordini
            ViewBag.TotalOrders = db.Orderr.Count();

            // Totale prodotti
            ViewBag.TotalProducts = db.Product.Count();

            // Totale incasso di sempre
            ViewBag.TotalRevenue = db.Orderr.Sum(o => o.TotalPrice);

            // I primi 3 prodotti più venduti
            ViewBag.TopProducts = db.OrderDetail
             .GroupBy(od => od.ProductID)
             .Select(group => new { ProductID = group.Key, Count = group.Count() })
             .OrderByDescending(g => g.Count)
             .Take(3)
             .ToList()
             .Select(x => new TopProductViewModel
             {
                 ProductID = x.ProductID,
                 Count = x.Count,
                 Name = db.Product.Where(p => p.ProductID == x.ProductID).Select(p => p.Name).FirstOrDefault()
             }).ToList();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public JsonResult GetDailyReport(DateTime day)
        {
            var orders = db.Orderr
                .Where(o => DbFunctions.TruncateTime(o.OrderDate) == day.Date && o.Status == "Evaso")
                .ToList();

            var totalOrders = orders.Count();
            var totalRevenue = orders.Sum(o => o.TotalPrice);

            return Json(new { totalOrders, totalRevenue }, JsonRequestBehavior.AllowGet);
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
