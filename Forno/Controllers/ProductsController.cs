using Forno.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Forno.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ModelDbContext db = new ModelDbContext();

        // GET: Products
        [Authorize]
        public ActionResult Index(string searchString)
        {
            var products = from p in db.Product
                           select p;

            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.Name.Contains(searchString)
                                               || s.Ingredient.Any(i => i.Name.Contains(searchString)));
            }

            return View(products.ToList());
        }

        // GET: Products/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var product = db.Product.Include(p => p.Ingredient).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            var allIngredients = db.Ingredient.ToList();

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                AllIngredients = allIngredients
            };

            if (Request.IsAjaxRequest())
            {
                // Restituisce solo la vista parziale se la richiesta è AJAX
                return PartialView("_Details", viewModel);
            }
            // Restituisce la vista completa se non è una richiesta AJAX
            ViewBag.AllProducts = db.Product.ToList();
            return View(viewModel);
        }



        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.IngredientID = new MultiSelectList(db.Ingredient.ToList(), "IngredientID", "Name");
            return View(new Product { SelectedIngredientIDs = new int[] { } });
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "Name,Price,DeliveryTime,SelectedIngredientIDs,ImageUrl")] Product product, HttpPostedFileBase ImageUpload)
        {
            if (ModelState.IsValid)
            {
                // La logica per aggiungere gli ingredienti al prodotto
                foreach (var ingredientId in product.SelectedIngredientIDs ?? new int[] { })
                {
                    var ingredient = db.Ingredient.Find(ingredientId);
                    if (ingredient != null)
                    {
                        product.Ingredient.Add(ingredient);
                    }
                }

                if (ImageUpload != null && ImageUpload.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(ImageUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images/"), fileName);
                    ImageUpload.SaveAs(path);

                    product.ImageUrl = fileName; // Salva solo il nome del file
                }

                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Se il ModelState non è valido, ripopola i dati necessari per la vista
            ViewBag.IngredientID = new MultiSelectList(db.Ingredient.ToList(), "IngredientID", "Name", product.SelectedIngredientIDs);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Include(p => p.Ingredient).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.IngredientID = new MultiSelectList(db.Ingredient.ToList(), "IngredientID", "Name", product.Ingredient.Select(i => i.IngredientID).ToList());
            ViewBag.SelectedIngredientIDs = product.Ingredient.Select(i => i.IngredientID).ToArray();

            return View(product);
        }




        // POST: Products/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Product productToUpdate, HttpPostedFileBase ImageUpload, int[] SelectedIngredientIDs)
        {
            if (ModelState.IsValid)
            {
                var dbProduct = db.Product.Include(p => p.Ingredient).FirstOrDefault(p => p.ProductID == productToUpdate.ProductID);
                if (dbProduct != null)
                {
                    dbProduct.Name = productToUpdate.Name;
                    dbProduct.Price = productToUpdate.Price;
                    dbProduct.DeliveryTime = productToUpdate.DeliveryTime;

                    if (ImageUpload != null && ImageUpload.ContentLength > 0)
                    {
                        // Rimuovi l'immagine esistente se presente
                        if (!string.IsNullOrWhiteSpace(dbProduct.ImageUrl))
                        {
                            var existingImagePath = Path.Combine(Server.MapPath("~/Images/"), dbProduct.ImageUrl);
                            if (System.IO.File.Exists(existingImagePath))
                            {
                                System.IO.File.Delete(existingImagePath);
                            }
                        }

                        var fileName = Path.GetFileNameWithoutExtension(ImageUpload.FileName) + DateTime.Now.Ticks + Path.GetExtension(ImageUpload.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/Images/"), fileName);
                        ImageUpload.SaveAs(path);

                        dbProduct.ImageUrl = fileName; // Salva solo il nome del file
                    }

                    // Controlla se sono stati forniti ID degli ingredienti
                    if (SelectedIngredientIDs != null && SelectedIngredientIDs.Length > 0)
                    {
                        dbProduct.Ingredient.Clear();
                        foreach (var ingredientId in SelectedIngredientIDs)
                        {
                            var ingredientToAdd = db.Ingredient.Find(ingredientId);
                            if (ingredientToAdd != null)
                            {
                                dbProduct.Ingredient.Add(ingredientToAdd);
                            }
                        }
                    }

                    db.Entry(dbProduct).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            ViewBag.IngredientID = new MultiSelectList(db.Ingredient.ToList(), "IngredientID", "Name", productToUpdate.SelectedIngredientIDs);
            return View(productToUpdate);
        }

        // GET: Products/Delete
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }


        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Product.Include(p => p.Ingredient).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Rimuove le relazioni con gli ingredienti
            product.Ingredient.Clear();

            // Ora è possibile rimuovere il prodotto poiché non ha più relazioni
            db.Product.Remove(product);
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
