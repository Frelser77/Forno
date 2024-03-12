using Forno.Models;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Forno.Controllers
{
    public class ProductsController : Controller
    {
        private ModelDbContext db = new ModelDbContext();

        // GET: Products
        public ActionResult Index()
        {
            return View(db.Product.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
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
            return View(product);
        }


        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.IngredientID = new MultiSelectList(db.Ingredient.ToList(), "IngredientID", "Name");
            return View(new Product { SelectedIngredientIDs = new int[] { } });
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Price,DeliveryTime,SelectedIngredientIDs,ImageUrl")] Product product)
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

                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Se il ModelState non è valido, ripopola i dati necessari per la vista
            ViewBag.IngredientID = new MultiSelectList(db.Ingredient.ToList(), "IngredientID", "Name", product.SelectedIngredientIDs);
            return View(product);
        }

        // GET: Products/Edit/5
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

            // Prepari la lista di tutti gli ingredienti disponibili.
            ViewBag.AllIngredients = new SelectList(db.Ingredient.ToList(), "IngredientID", "Name", product.Ingredient.Select(i => i.IngredientID).ToList());

            return View(product);
        }




        // POST: Products/Edit/5
        // Per la protezione da attacchi di overposting, abilitare le proprietà a cui eseguire il binding. 
        // Per altri dettagli, vedere https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,Name,Price,DeliveryTime,ImageUrl,SelectedIngredientIDs")] Product productToUpdate)
        {
            if (ModelState.IsValid)
            {

                Product dbProduct = db.Product.Include(p => p.Ingredient).FirstOrDefault(p => p.ProductID == productToUpdate.ProductID);
                if (dbProduct != null)
                {

                    dbProduct.Name = productToUpdate.Name;
                    dbProduct.Price = productToUpdate.Price;
                    dbProduct.DeliveryTime = productToUpdate.DeliveryTime;
                    dbProduct.ImageUrl = productToUpdate.ImageUrl;

                    dbProduct.Ingredient.Clear();
                    if (productToUpdate.SelectedIngredientIDs != null)
                    {
                        foreach (var ingredientId in productToUpdate.SelectedIngredientIDs)
                        {
                            Ingredient ingredientToAdd = db.Ingredient.Find(ingredientId);
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
            ViewBag.AllIngredients = new MultiSelectList(db.Ingredient.ToList(), "IngredientID", "Name", productToUpdate.SelectedIngredientIDs);
            return View(productToUpdate);
        }


        // GET: Products/Delete
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
