using InventoryManagementSystem.Models;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class AdminController : Controller
    {

        //voids and jsons
        #region
        public void getSPS()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                List<series> seriesList = new List<series>();
                List<product> productList = new List<product>();
                List<stock> stockList = new List<stock>();

                var series = db.series.Where(x => x.series_name != null).Select(x => x.series_id);
                foreach (var i in series)
                {
                    seriesList.Add(db.series.Where(x => x.series_id == i).Single());
                }

                ViewBag.Series = seriesList;

                
                var products = db.products.Where(x => x.product_name != null).Select(x => x.product_id);
                foreach (var i in products)
                {
                    var ingredientString = "";
                    product prodModel = db.products.Where(x => x.product_id == i).Single();
                    var ingredients = db.products.Where(x => x.product_id == i).Select(x => x.product_ingredients).FirstOrDefault().Split('|');

                    var ctr = 0;
                    foreach(var item in ingredients)
                    {
                        if(item != "")
                        {
                            try
                            {
                                var parse = Int32.Parse(ingredients[ctr]);
                                var stock = db.stocks.Where(x => x.stock_id == parse).Select(x => x.stock_name).FirstOrDefault();
                                ingredientString += stock + "|";
                            }
                            catch (Exception e)
                            {
                                ingredientString += "NULL" + "|";
                            }
                            
                        }
                        ctr++;

                    }
                    ctr = 0;

                    prodModel.ingredientString = ingredientString;
                    productList.Add(prodModel);
                } 
                ViewBag.Products = productList;

                var stocks = db.stocks.Where(x => x.stock_category == "ingredient").Select(x => x.stock_id);
                foreach (var i in stocks)
                {
                    stockList.Add(db.stocks.Where(x => x.stock_id == i).Single());
                }

                ViewBag.Stocks = stockList;
            }
        }

        [HttpPost]
        public JsonResult getNotifications()
        {
            var ctr = 0;
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {

                stock stockModel = new stock();
                List<stock> stockList = new List<stock>();
                var stocks = db.stocks.Where(x => x.stock_name != null).Select(x => x.stock_id);
                foreach (var i in stocks)
                {
                    var stock_capacity = db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_capacity).FirstOrDefault();
                    var stock_quantity = db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_quantity).FirstOrDefault();

                    if (stock_quantity <= stock_capacity * (decimal)0.75)
                    {
                        ctr++;
                    }
                }
            }
            return Json(ctr);
        }
        #endregion

        //Notifications
        #region
        [HttpGet]
        public ActionResult Notifications()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                stock stockModel = new stock();
                List<stock> stockList = new List<stock>();
                var stocks = db.stocks.Where(x => x.stock_name != null).Select(x => x.stock_id);
                foreach (var i in stocks)
                {
                    var stock_capacity = db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_capacity).FirstOrDefault();
                    var stock_quantity = db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_quantity).FirstOrDefault();
                    stockModel = db.stocks.Where(x => x.stock_id == i).Single();
                    stockModel.stock_date = db.stockTrans.Where(x => x.stockTrans_stock == i).OrderByDescending(x => x.stockTrans_timestamp).Select(x => x.stockTrans_timestamp).FirstOrDefault();

                    if (stock_quantity <= stock_capacity * (decimal)0.75 && stock_quantity >= stock_capacity * (decimal)0.5)
                    {
                        stockModel.stock_warning = 1; //less than 75% and greater than 50% of stock capacity (LOW)
                        stockList.Add(stockModel);
                    }
                    else if (stock_quantity <= stock_capacity * (decimal)0.5 && stock_quantity >= stock_capacity * (decimal)0.15)
                    {
                        stockModel.stock_warning = 2; //less than 50% and greater than 25% of stock capacity (MEDIUM)
                        stockList.Add(stockModel);
                    }
                    else if (stock_quantity <= stock_capacity * (decimal)0.15 && stock_quantity >= stock_capacity * (decimal)0.01)
                    {
                        stockModel.stock_warning = 3; //less than 25% and greater than 10% of stock capacity (HIGH)
                        stockList.Add(stockModel);
                    }
                    else if (stock_quantity <= 0)
                    {
                        stockModel.stock_warning = 4; //Out of stock
                        stockList.Add(stockModel);
                    }
                }
                ViewBag.Stocks = stockList.OrderByDescending(x => x.stock_warning);
            }
            return View();
        }
        #endregion

        //#PointOfSales
        #region
        [HttpGet]
        public ActionResult PointOfSales()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                List<series> seriesList = new List<series>();
                List<product> productList = new List<product>();

                var series = db.series.Where(x => x.series_name != null).Select(x => x.series_id);
                foreach (var i in series)
                {
                    seriesList.Add(db.series.Where(x => x.series_id == i).Single());
                }

                ViewBag.Series = seriesList;

                
                var products = db.products.Where(x => x.product_name != null).Select(x => x.product_id);
                foreach (var i in products)
                {
                    var ingredientString = "";
                    product productModel = db.products.Where(x => x.product_id == i).Single();
                    var prodIngredients = db.products.Where(x => x.product_id == i).Select(x => x.product_ingredients).Single().Split('|');
                     
                    var ctr = 0;
                    foreach (var item in prodIngredients)
                    {
                        if (item != "")
                        {
                            var parse = Int32.Parse(prodIngredients[ctr]);
                            var stock = db.stocks.Where(x => x.stock_id == parse).Select(x => x.stock_name).FirstOrDefault();
                            ingredientString += stock + "|";
                        }
                        ctr++;
                    }
                    ctr = 0;
                    

                    int[] stockIds = new int[prodIngredients.Length - 1];
                    ctr = 0;
                    foreach (var u in prodIngredients)
                    {
                        if (u != "")
                        {
                            stockIds[ctr] = (db.stocks.Where(x => x.stock_name == u).Select(x => x.stock_id).FirstOrDefault());
                            ctr++;
                        }
                    }

                    var prodAmounts = db.products.Where(x => x.product_id == i).Select(x => x.product_amounts).Single().Split('|');
                    int[] prodArray = new int[prodAmounts.Length - 1];
                    ctr = 0;
                    foreach (var u in prodAmounts)
                    {
                        if (u != "")
                        {
                            prodArray[ctr] = Int32.Parse(u);
                            ctr++;
                        }
                    }
                    ctr = 0;

                    //Check if product is on stock 
                    int flag = 0;
                    foreach (var item in stockIds)
                    {
                        var itemQuantity = db.stocks.Where(x => x.stock_id == item).Select(x => x.stock_quantity).FirstOrDefault(); 
                        if (itemQuantity < prodArray[ctr])
                        {
                            flag++;
                        }
                        ctr++;
                    }
                    ctr = 0;

                    if (flag == 0)
                    {
                        productModel = db.products.Where(x => x.product_id == i).Single();
                        productModel.isAvailable = true;
                        productModel.ingredientString = ingredientString;
                        productList.Add(productModel);
                    }
                    else
                    {
                        productModel = db.products.Where(x => x.product_id == i).Single();
                        productModel.isAvailable = false;
                        productModel.ingredientString = ingredientString;
                        productList.Add(productModel);
                    }
                }
                ViewBag.Products = productList;
            }
            return View();
        }

        [HttpPost]
        public JsonResult GetOrderDetails(product productModel)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                return Json(new
                {
                    name = db.products.Where(x => x.product_id == productModel.product_id).Select(x => x.product_name).FirstOrDefault(),
                    series = db.series.Where(x => x.series_id == db.products.Where(u => u.product_id == productModel.product_id).Select(u => u.product_series).FirstOrDefault()).Select(x => x.series_name).FirstOrDefault(),
                    price = "₱" + db.products.Where(x => x.product_id == productModel.product_id).Select(x => x.product_price).FirstOrDefault() * productModel.product_price,
                    date = DateTime.Now.ToString("yyyy-mm-dd hh:mm tt")
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult InsertOrders(List<order> orders)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                //Check for NULL.
                if (orders == null)
                {
                    return Json("Please add orders first!");
                }
                else
                { 
                    foreach (order o in orders)
                    {
                        o.order_productID = db.products.Where(x => x.product_name == o.order_productName).Select(x => x.product_id).FirstOrDefault();
                        o.order_price = db.products.Where(x => x.product_id == o.order_productID).Select(x => x.product_price).FirstOrDefault() * o.order_quantity;
                        o.order_timestamp = DateTime.Now;
                        var prodIngredients = db.products.Where(x => x.product_id == db.products.Where(u => u.product_name == o.order_productName).Select(u => u.product_id).FirstOrDefault()).Select(x => x.product_ingredients).Single().Split('|');
                        int[] stockIds = new int[prodIngredients.Length - 1];
                        int ctr = 0;
                        foreach (var i in prodIngredients)
                        {
                            if (i != "")
                            {
                                var parse = Int32.Parse(prodIngredients[ctr]);
                                stockIds[ctr] = (db.stocks.Where(x => x.stock_id == parse).Select(x => x.stock_id).FirstOrDefault());
                                ctr++;
                            }
                        }
                        var prodAmounts = db.products.Where(x => x.product_id == db.products.Where(u => u.product_name == o.order_productName).Select(u => u.product_id).FirstOrDefault()).Select(x => x.product_amounts).Single().Split('|');
                        int[] prodArray = new int[prodAmounts.Length - 1];

                        ctr = 0;
                        foreach (var i in prodAmounts)
                        {
                            if (i != "")
                            {
                                prodArray[ctr] = Int32.Parse(i);
                                ctr++;
                            }
                        }
                        ctr = 0;

                        //Update stockstrams
                        stockTran stockTransModel = new stockTran();
                        foreach (var item in stockIds)
                        {
                            var itemName = db.stocks.Where(x => x.stock_id == item).Select(x => x.stock_name).FirstOrDefault();
                            var itemUnit = db.stocks.Where(x => x.stock_id == item).Select(x => x.stock_unit).FirstOrDefault();
                            stockTransModel.stockTrans_stock = (byte)stockIds[ctr];
                            stockTransModel.stockTrans_narration = o.order_productName + " used " + prodArray[ctr] * o.order_quantity + " " + itemUnit + " of " + itemName ;
                            stockTransModel.stockTrans_timestamp = DateTime.Now;
                            stockTransModel.stockTrans_type = "OUT";
                            stockTransModel.stockTrans_value = prodArray[ctr] * o.order_quantity;

                            stock stocksModel = db.stocks.Find((byte)stockIds[ctr]);
                            var stockQty = stocksModel.stock_quantity;
                            var stockID = (byte)stockIds[ctr];
                            stocksModel.stock_quantity = db.stocks.Where(x => x.stock_id == stockID).Select(x => x.stock_quantity).FirstOrDefault() - (prodArray[ctr] * o.order_quantity);
                             

                            db.stocks.Attach(stocksModel);
                            db.Entry(stocksModel).Property(x => x.stock_quantity).IsModified = true;

                            db.stockTrans.Add(stockTransModel);
                            db.SaveChanges();
                            ctr++;
                        }
                        db.orders.Add(o);
                    }
                    int insertedRecords = db.SaveChanges();
                    return Json("Orders recorded!");
                }
            }
        }
        #endregion

        //#ProductManagement 
        #region
        public ActionResult ProductManagement()
        {
            getSPS();
            return View(); 
        }


        public ActionResult _AddNewProduct(int product_series)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                List<String> ingredientsList = new List<String>();
                var ingredients = db.stocks.Where(x => x.stock_category == "ingredient").Select(x => x.stock_id);
                foreach (var i in ingredients)
                {
                    var item = db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_name).FirstOrDefault();
                    var unit = db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_unit).FirstOrDefault();

                    ingredientsList.Add(item + " (" + unit + ")");
                }

                ViewBag.Ingredients = ingredientsList;
                return View();
            }
        }

        [HttpPost]
        public ActionResult _AddNewProduct(product productModel, HttpPostedFileBase product_imagePath)
        {
            if (productModel.ingredients.Count != productModel.ingredients.Distinct().Count())
            {
                Session["StatusMsg"] = "Failed to add new product, please refrain from duplicating ingredients";
                return RedirectToAction("ProductManagement", "Admin");
            }
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                var ingredientString = "";
                var amountString = "";
                var unitString = "";
                var stockStrings = db.stocks.Where(x => x.stock_name != null).Select(x => x.stock_name);
                if (product_imagePath != null)
                {
                    productModel.product_image = new byte[product_imagePath.ContentLength];
                    product_imagePath.InputStream.Read(productModel.product_image, 0, product_imagePath.ContentLength);
                }

                if (productModel.ingredients != null)
                {
                    foreach (var ingredients in productModel.ingredients)
                    {
                        if (!ingredients.Equals(""))
                        {
                            var item = ingredients.Split(' ')[0];
                            foreach (var stock in stockStrings)
                            {
                                if (stock == item)
                                {
                                    var unit = db.stocks.Where(x => x.stock_name == item).Select(x => x.stock_unit).FirstOrDefault();
                                    unitString += unit + "|";
                                    productModel.product_units = unitString;
                                }
                            }
                            ingredientString += db.stocks.Where(x=> x.stock_name == item).Select(x=> x.stock_id).FirstOrDefault() + "|";
                            productModel.product_ingredients = ingredientString;
                        }
                    }
                    foreach (var item in productModel.amounts)
                    {
                        if (!item.Equals(""))
                        {
                            amountString += item + "|";
                            productModel.product_amounts = amountString;
                        }
                    }
                }
                 
                productModel.product_created = DateTime.Now;
                db.products.Add(productModel);
                db.SaveChanges();
                Session["StatusMsg"] = "Product successfully added!";
            }
            return RedirectToAction("ProductManagement", "Admin");

        }

        [HttpPost]
        public ActionResult RemoveProduct(int id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                product productObj = db.products.Where(x => x.product_id == id).FirstOrDefault();
                if (productObj != null)
                {
                    db.products.Remove(productObj);
                    db.SaveChanges();
                    Session["StatusMsg"] = "Product successfully Deleted!";
                }
            }
            return RedirectToAction("ProductManagement", "Admin");
        }

        [HttpGet]
        public ActionResult _EditProduct(int id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                List<String> ingredientsList = new List<String>();

                var ingredients = db.stocks.Where(x => x.stock_category == "ingredient").Select(x => x.stock_id);
                foreach (var i in ingredients)
                {
                    var item = db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_name).FirstOrDefault();
                    var unit = db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_unit).FirstOrDefault();

                    ingredientsList.Add(item + " (" + unit + ")");
                }

                ViewBag.Ingredients = ingredientsList;


                //List<String> prodingredientsList = new List<String>();
                //var prodIngredients = db.products.Where(x => x.product_id == id).Select(x => x.product_ingredients).Single().Split('|');

                //foreach(var i in prodIngredients)
                //{
                //    if(i != "")
                //    prodingredientsList.Add(i);
                //}
                //ViewBag.prodIngredients = prodingredientsList;

                //List<String> prodamountsList = new List<String>();
                //var prodAmounts = db.products.Where(x => x.product_id == id).Select(x => x.product_amounts).Single().Split('|');

                //foreach (var i in prodAmounts)
                //{
                //    if (i != "")
                //        prodamountsList.Add(i);
                //}
                //ViewBag.prodAmounts = prodamountsList;

                return View(db.products.Where(x => x.product_id == id).FirstOrDefault());
            }
        }

        [HttpPost]
        public ActionResult _EditProduct(product productModel, HttpPostedFileBase product_imagePath)
        {
            if (productModel.ingredients.Count != productModel.ingredients.Distinct().Count())
            {
                Session["StatusMsg"] = "Failed to edit product, please refrain from duplicating ingredients";
                return RedirectToAction("ProductManagement", "Admin");
            }
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                bool imgModified = true;
                bool ingModified = false;
                var ingredientString = "";
                var amountString = "";
                var unitString = "";

                if (product_imagePath != null)
                {
                    productModel.product_image = new byte[product_imagePath.ContentLength];
                    product_imagePath.InputStream.Read(productModel.product_image, 0, product_imagePath.ContentLength);
                }
                else
                {
                    imgModified = false;
                }

                if (productModel.ingredients != null)
                {
                    var stockStrings = db.stocks.Where(x => x.stock_name != null).Select(x => x.stock_name);
                    foreach (var ingredients in productModel.ingredients)
                    {
                        if (!ingredients.Equals(""))
                        {
                            var item = ingredients.Split(' ')[0];
                            foreach (var stock in stockStrings)
                            {
                                if (stock.Equals(item))
                                {
                                    var unit = db.stocks.Where(x => x.stock_name == item).Select(x => x.stock_unit).FirstOrDefault();
                                    unitString += unit + "|";
                                    productModel.product_units = unitString;
                                }
                            } 
                            ingredientString += db.stocks.Where(x=> x.stock_name == item).Select(x=> x.stock_id).FirstOrDefault() + "|";
                            productModel.product_ingredients = ingredientString;
                            ingModified = true;
                        }
                    }
                    foreach (var item in productModel.amounts)
                    {
                        if (!item.Equals(""))
                        {
                            amountString += item + "|";
                            productModel.product_amounts = amountString;
                            ingModified = true;
                        }
                    }
                }

                db.Entry(productModel).State = EntityState.Modified;

                db.Entry(productModel).Property(x => x.product_created).IsModified = false;
                if (imgModified == false)
                {
                    db.Entry(productModel).Property(x => x.product_image).IsModified = false;
                }
                if (ingModified == false)
                {
                    db.Entry(productModel).Property(x => x.product_ingredients).IsModified = false;
                    db.Entry(productModel).Property(x => x.product_amounts).IsModified = false;
                    db.Entry(productModel).Property(x => x.product_units).IsModified = false;
                }

                db.SaveChanges();
                Session["StatusMsg"] = productModel.product_name + " is successfully edited!";
                return RedirectToAction("ProductManagement", "Admin");
            }
        }

        [HttpPost]
        public ActionResult AddSeries(series seriesModel)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                seriesModel.series_created = DateTime.Now;
                db.series.Add(seriesModel);
                db.SaveChanges();
                Session["StatusMsg"] = "Series successfully added!";
            }
            return RedirectToAction("ProductManagement", "Admin");
        }

        [HttpPost]
        public ActionResult RemoveSeries(int id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                series seriesObj = db.series.Where(x => x.series_id == id).FirstOrDefault();
                var products = db.products.Where(x => x.product_series == seriesObj.series_id).Select(x => x.product_id);
                if (products != null)
                {
                    foreach (var item in products)
                    {
                        db.products.Remove(db.products.Where(x => x.product_id == item).Single());
                    }

                }

                db.series.Remove(seriesObj);
                db.SaveChanges();
                Session["StatusMsg"] = "Series successfully deleted!";
            }
            return RedirectToAction("ProductManagement", "Admin");
        }

        [HttpGet]
        public ActionResult _EditSeries(int id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                return View(db.series.Where(x => x.series_id == id).FirstOrDefault());
            }

        }

        [HttpPost]
        public ActionResult _EditSeries(series seriesModel)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                db.Entry(seriesModel).State = EntityState.Modified;
                db.Entry(seriesModel).Property(x => x.series_created).IsModified = false;
                db.SaveChanges();
                Session["StatusMsg"] = seriesModel.series_name + " is successfully edited!";
                return RedirectToAction("ProductManagement", "Admin");
            }
        }
        #endregion

        //#UserManagement
        #region 
        public ActionResult UserManagement()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                return View(db.users.ToList());
            }
        }

        public ActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(user user)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                if (db.users.Where(u => u.user_username == user.user_username).Any())
                {
                    ModelState.AddModelError("user_username", "Username already taken");
                    return View(user);
                }
                else
                { 
                    if (ModelState.IsValid)
                    {
                        if (user.ImageFile != null)
                        {
                            user.user_img = new byte[user.ImageFile.ContentLength];
                            user.ImageFile.InputStream.Read(user.user_img, 0, user.ImageFile.ContentLength);
                        }  
                        user.user_timestamp = "New Account";  
                        db.users.Add(user);
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "New Record Added Successfuly!";
                        return RedirectToAction("UserManagement", "Admin");
                    } 
                }
            }
            return View(user);
        }

        public ActionResult EditUser(byte? id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                user user = db.users.Find(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                return View(user);
            } 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser([Bind(Include = "user_id,user_username,user_pass,user_role,user_name,user_address,user_email,user_status,user_contact,user_timestamp,user_nickname")] user user, HttpPostedFileBase imgEdit)
        {
            if (ModelState.IsValid)
            { 
                using (IMSDatabaseEntities db = new IMSDatabaseEntities())
                { 
                    bool imgModified = true;

                    db.Entry(user).State = EntityState.Modified;
                    db.Entry(user).Property(x => x.user_timestamp).IsModified = false;

                    if (imgEdit != null)
                    {

                        user.user_img = new byte[imgEdit.ContentLength];
                        imgEdit.InputStream.Read(user.user_img, 0, imgEdit.ContentLength);
                        db.Entry(user).Property(x => x.user_img).IsModified = true;

                        if (user.user_id.ToString() == Session["userID"].ToString())
                        {
                            var base64 = Convert.ToBase64String(user.user_img);
                            Session["userImage"] = string.Format("data:image/gif; base64, {0}", base64);
                        }
                    }
                    else
                    {
                        imgModified = false;
                    }
                    /////////////////////////////////
                    if (user.user_id.ToString() == Session["userID"].ToString())
                    {
                        Session["userNickname"] = user.user_nickname;

                    }
                    //////////////////////////////////
                    if (imgModified == false)
                    {
                        db.Entry(user).Property(x => x.user_img).IsModified = false;
                    }

                    db.SaveChanges();
                    TempData["UserStatus"] = "Record Edited Successfuly!";
                    return RedirectToAction("UserManagement", "Admin");
                }
            } 
            return View(user);
        }
        [HttpGet]
        public ActionResult DeleteUser(byte? id)
        {
            using(IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                user user = db.users.Find(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                return View(user);
            } 
        } 
        [HttpPost]
        public ActionResult DeleteUser(byte id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                user user = db.users.Find(id);
                db.users.Remove(user);
                db.SaveChanges();

                TempData["UserStatus"] = "Record Deleted Successfuly!";
                return RedirectToAction("UserManagement", "Admin");
            }
        }
        #endregion

        //Inventory Dashboard
        #region
        public ActionResult InventoryDashboard()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                List<string> years = new List<string>();
                var orders = db.orders.Where(x => x.order_timestamp != null).Select(x => x.order_timestamp.Value.Year).Distinct();
                foreach(var i in orders)
                {
                    years.Add(i.ToString());
                }
                ViewBag.availableYears = years;

                  
                List<ProductsSoldRepository> productSoldList = new List<ProductsSoldRepository>();
                var productCount = db.products.ToList(); 
                foreach(var product in productCount)
                { 
                    
                    ProductsSoldRepository psr = new ProductsSoldRepository(); 
                    psr.product = product.product_name;
                    psr.price = (decimal) product.product_price;

                    var sales = db.orders.Where(x => x.order_productID == product.product_id).Sum(x => x.order_quantity); 
                    if(sales != null)
                    {
                        psr.sales = (int) sales;
                    }
                    else
                    {
                        psr.sales = 0;
                    }
                    productSoldList.Add(psr);
                }
                ViewBag.ProductsSold = productSoldList;

                var dateNow = DateTime.Now;


                ViewBag.SalesToday = db.orders.Where(x => x.order_timestamp.Value.Day == dateNow.Day && x.order_timestamp.Value.Month == dateNow.Month && x.order_timestamp.Value.Year == dateNow.Year).Count();
                var salesTodayGross = db.orders.Where(x => x.order_timestamp.Value.Day == dateNow.Day && x.order_timestamp.Value.Month == dateNow.Month && x.order_timestamp.Value.Year == dateNow.Year).Sum(x => x.order_price);
                if(salesTodayGross == null)
                {
                    ViewBag.SalesTodayGross = "₱0.00";
                }
                else
                {
                    ViewBag.SalesTodayGross = "₱" + salesTodayGross;
                }

                ViewBag.SalesThisMonth = db.orders.Where(x => x.order_timestamp.Value.Month == dateNow.Month && x.order_timestamp.Value.Year == dateNow.Year).Count();
                var SalesThisMonthGross = db.orders.Where(x => x.order_timestamp.Value.Month == dateNow.Month && x.order_timestamp.Value.Year == dateNow.Year).Sum(x => x.order_price);
                if (SalesThisMonthGross == null)
                {
                    ViewBag.SalesThisMonthGross = "₱0.00";
                }
                else
                {
                    ViewBag.SalesThisMonthGross = "₱" + SalesThisMonthGross;
                }

                ViewBag.SalesThisYear = db.orders.Where(x => x.order_timestamp.Value.Year == dateNow.Year).Count();
                var SalesThisYearGross = db.orders.Where(x => x.order_timestamp.Value.Year == dateNow.Year).Sum(x => x.order_price);
                if (SalesThisYearGross == null)
                {
                    ViewBag.SalesThisYearGross = "₱0.00";
                }
                else
                {
                    ViewBag.SalesThisYearGross = "₱" + SalesThisYearGross;
                }

                return View();
            }
        } 

        [HttpPost]
        public JsonResult MonthlySales(JsonConstructor jc)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {  
                var totalSales = db.orders.Where(x => x.order_timestamp.Value.Month == jc.month).Sum(x => x.order_price);
                if(totalSales == null)
                {
                    totalSales = 0;
                } 

                var daysofthemonth = DateTime.DaysInMonth(jc.year, jc.month);
                string[] monthSales = new string[daysofthemonth];
                string[] monthSalesLabels = new string[daysofthemonth];
                for (int i = 1; i <= daysofthemonth; i++)
                { 
                    monthSales[i-1] = db.orders.Where(x => x.order_timestamp.Value.Year == jc.year && x.order_timestamp.Value.Month == jc.month && x.order_timestamp.Value.Day == i)
                        .Sum(x => x.order_price)
                        .ToString();

 
                        monthSalesLabels[i-1] = i.ToString();
 
                } 
                return Json(new
                {
                    total = totalSales,
                    totalLabel = "Overall sales in the month of",
                    datalabels = monthSalesLabels,
                    dataset = monthSales
                }, JsonRequestBehavior.AllowGet); 
            } 
        }
        [HttpPost]
        public JsonResult YearlySales(JsonConstructor jc)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                var ytotalSales = db.orders.Where(x => x.order_timestamp.Value.Year == jc.year).Sum(x => x.order_price);
                if (ytotalSales == null)
                {
                    ytotalSales = 0;
                }
                  
                string[] yearlySales = new string[12];
                string[] yearlySalesLabels = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                for (int i = 1; i <= 12; i++)
                {
                    yearlySales[i-1] = db.orders.Where(x => x.order_timestamp.Value.Year == jc.year && x.order_timestamp.Value.Month == i)
                        .Sum(x => x.order_price)
                        .ToString(); 
                }
                return Json(new
                {
                    total = ytotalSales,
                    totalLabel = "Overall sales in the year of",
                    datalabels = yearlySalesLabels,
                    dataset = yearlySales
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ProductSales(JsonConstructor jc)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                //if products
                if(jc.option == 1)
                {
                    var productCount = db.products.ToList();
                    decimal[] productSales = new decimal[productCount.Count];
                    string[] productSalesLabels = new string[productCount.Count];
                    int ctr = 0;
                    foreach (var product in productCount)
                    {
                        var sales = db.orders.Where(x => x.order_productID == product.product_id).Sum(x => x.order_price);
                        if(sales != null)
                        {
                            productSales[ctr] = (decimal)sales;
                        }
                        else
                        {
                            productSales[ctr] = 0;
                        }
                        
                        productSalesLabels[ctr] = product.product_name;
                        ctr++;
                    }
                    int maxIndex = productSales.ToList().IndexOf(productSales.Max());

                    return Json(new
                    {
                        total = productSales.Max(),
                        totalLabel = productSalesLabels[maxIndex],
                        datalabels = productSalesLabels,
                        dataset = productSales

                    }, JsonRequestBehavior.AllowGet);
                }
                else //if series
                {
                    var seriesCount = db.series.ToList();
                    decimal[] seriesSales = new decimal[seriesCount.Count];
                    string[] seriesSalesLabels = new string[seriesCount.Count];
                    int ctr = 0;
                    foreach (var product in seriesCount)
                    {
                        decimal productSeries = 0;

                        foreach(var i in db.products.Where(x=>x.product_series == product.series_id))
                        {
                            var sales = db.orders.Where(x => x.order_productID == i.product_id).Sum(x => x.order_price);
                            if(sales != null)
                            {
                                productSeries += (decimal) sales;
                            }
                            else
                            {
                                productSeries += 0;
                            }
                            
                        }

                        seriesSales[ctr] = productSeries;
                        seriesSalesLabels[ctr] = product.series_name;
                        ctr++;
                    } 
                    int maxIndex = seriesSales.ToList().IndexOf(seriesSales.Max()); 
                    return Json(new
                    {
                        total = seriesSales.Max(),
                        totalLabel = seriesSalesLabels[maxIndex],
                        datalabels = seriesSalesLabels,
                        dataset = seriesSales

                    }, JsonRequestBehavior.AllowGet);
                } 
            }
        } 

        [HttpGet]
        public ActionResult TransactionHistory()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                order orderObject = new order();
                List<order> ordersList = new List<order>();
                var orders = db.orders.Where(x => x.order_timestamp != null).Select(x => x.order_id);
                foreach (var item in orders)
                { 
                    orderObject = db.orders.Where(x => x.order_id == item).Single();
                    orderObject.order_productName = db.products.Where(x => x.product_id == orderObject.order_productID).Select(x => x.product_name).FirstOrDefault();
                    orderObject.order_series = db.series.Where(x => x.series_id == db.products.Where(u => u.product_id == orderObject.order_productID).Select(u => u.product_series).FirstOrDefault()).Select(x => x.series_name).FirstOrDefault();
                    ordersList.Add(orderObject); 
                } 
                return View(ordersList);  
            }  
        } 

        [HttpGet]
        public ActionResult InventoryHistory()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                stockTran stockTransObject = new stockTran();
                List<stockTran> stockTransList = new List<stockTran>();
                var stockTrans = db.stockTrans.Where(x => x.stockTrans_stock != null).Select(x => x.stockTrans_id);
                foreach (var item in stockTrans)
                {
                    var itemName = db.stocks.Where(x => x.stock_id == db.stockTrans.Where(u => u.stockTrans_id == item).Select(u => u.stockTrans_stock).FirstOrDefault()).Select(x => x.stock_name).FirstOrDefault();
                    var itemUnit = db.stocks.Where(x => x.stock_id == db.stockTrans.Where(u => u.stockTrans_id == item).Select(u => u.stockTrans_stock).FirstOrDefault()).Select(x => x.stock_unit).FirstOrDefault();
                    stockTransObject = db.stockTrans.Where(x => x.stockTrans_id == item).Single();
                    stockTransObject.stockTrans_stockName = itemName;
                    stockTransObject.stockTrans_stockUnit = itemUnit;
                    stockTransObject.stockTrans_stockCategory = db.stocks.Where(x => x.stock_id == db.stockTrans.Where(u => u.stockTrans_id == item).Select(u => u.stockTrans_stock).FirstOrDefault()).Select(x => x.stock_category).FirstOrDefault();
                    stockTransList.Add(stockTransObject);
                }
                return View(stockTransList);
            }
        }
        public void ProdsExportToCSV()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                List<ProductsSoldRepository> productSoldList = new List<ProductsSoldRepository>();
                var productCount = db.products.ToList();
                foreach (var product in productCount)
                {
                    ProductsSoldRepository psr = new ProductsSoldRepository();
                    psr.product = product.product_name;
                    psr.price = (decimal)product.product_price;
                    psr.sales = (int)db.orders.Where(x => x.order_productID == product.product_id).Sum(x => x.order_quantity);
                    productSoldList.Add(psr);
                }

                //CSV
                StringWriter sw = new StringWriter();
                //Header
                sw.WriteLine("\"Product\", \"Price\", \"Sold\"");
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment;filename=Exported_Transactions.csv");
                Response.ContentType = "text/csv";

                foreach (var x in productSoldList)
                {
                    sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\"",
                        x.product,
                        "Php. " + x.price,
                        x.sales));
                }
                Response.Write(sw.ToString());
                Response.End();
            }
        }
        public void TransExportToCSV()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                order orderObject = new order();
                List<order> ordersList = new List<order>();
                var orders = db.orders.Where(x => x.order_timestamp != null).Select(x => x.order_id);
                foreach (var item in orders)
                {
                    orderObject = db.orders.Where(x => x.order_id == item).Single();
                    orderObject.order_productName = db.products.Where(x => x.product_id == orderObject.order_productID).Select(x => x.product_name).FirstOrDefault();
                    orderObject.order_series = db.series.Where(x => x.series_id == db.products.Where(u => u.product_id == orderObject.order_productID).Select(u => u.product_series).FirstOrDefault()).Select(x => x.series_name).FirstOrDefault();
                    ordersList.Add(orderObject);
                }
                //CSV
                StringWriter sw = new StringWriter();
                //Header
                sw.WriteLine("\"Datetime\", \"Transaction ID\", \"Product\", \"Series\",\"Quantity\",\"Price\"");
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment;filename=Exported_Transactions.csv");
                Response.ContentType = "text/csv";

                foreach (var x in ordersList)
                {
                    sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"",
                        x.order_timestamp,
                        x.order_id,
                        x.order_productName,
                        x.order_series,
                        x.order_quantity,
                        "Php. " + x.order_price));
                }
                Response.Write(sw.ToString());
                Response.End();
            }
        }
        public void InvExportToCSV()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                stockTran stockTransObject = new stockTran();
                List<stockTran> stockTransList = new List<stockTran>();
                var stockTrans = db.stockTrans.Where(x => x.stockTrans_stock != null).Select(x => x.stockTrans_id);
                foreach (var item in stockTrans)
                {
                    var itemName = db.stocks.Where(x => x.stock_id == db.stockTrans.Where(u => u.stockTrans_id == item).Select(u => u.stockTrans_stock).FirstOrDefault()).Select(x => x.stock_name).FirstOrDefault();
                    var itemUnit = db.stocks.Where(x => x.stock_id == db.stockTrans.Where(u => u.stockTrans_id == item).Select(u => u.stockTrans_stock).FirstOrDefault()).Select(x => x.stock_unit).FirstOrDefault();
                    stockTransObject = db.stockTrans.Where(x => x.stockTrans_id == item).Single();
                    stockTransObject.stockTrans_stockName = itemName;
                    stockTransObject.stockTrans_stockUnit = itemUnit;
                    stockTransObject.stockTrans_stockCategory = db.stocks.Where(x => x.stock_id == db.stockTrans.Where(u => u.stockTrans_id == item).Select(u => u.stockTrans_stock).FirstOrDefault()).Select(x => x.stock_category).FirstOrDefault();
                    stockTransList.Add(stockTransObject);
                }
                //CSV
                StringWriter sw = new StringWriter();
                //Header
                sw.WriteLine("\"Timestamp\", \"Stock Trans ID\", \"Stock\", \"Category\", \"Quantity\", \"Transaction Type\", \"Narration\"");
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment;filename=Exported_InventoryTransactions.csv");
                Response.ContentType = "text/csv";

                foreach (var x in stockTransList)
                {
                    sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                        x.stockTrans_timestamp,
                        x.stockTrans_id,
                        x.stockTrans_stockName,
                        x.stockTrans_stockCategory,
                        x.stockTrans_value,
                        x.stockTrans_type,
                        x.stockTrans_narration
                        ));
                } 
                Response.Write(sw.ToString());
                Response.End();
            }
        }
        #endregion

        //InventoryOverview
        #region
        public ActionResult InventoryOverview()
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                List<stock> stockList = new List<stock>();
                stock stockModel = new stock();
                var stocks = db.stocks.Where(x => x.stock_name != null).Select(x => x.stock_id);
                foreach (var i in stocks)
                {
                    stockModel = db.stocks.Where(x => x.stock_id == i).Single();
                    stockModel.percentage = (decimal)(db.stocks.Where(x => x.stock_id == i).Select(x=> x.stock_quantity).FirstOrDefault() / db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_capacity).FirstOrDefault()) * 100;
                    stockList.Add(stockModel);
                }
                ViewBag.Stocks = stockList;

                return View();
            }
        }

        /* RE STOCK */
        [HttpGet]
        public ActionResult ReStock(int id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                return View(db.stocks.Where(x => x.stock_id == id).FirstOrDefault());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReStock(stock model)
        {
             using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                var stockQuantity = (decimal) db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_quantity).FirstOrDefault() + model.toAdd;
                var stockCapacity = (decimal) db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_capacity).FirstOrDefault();
                if (stockQuantity > stockCapacity)
                {
                    Session["StatusMsg"] = "Stock cant exceed max capacity";
                    return RedirectToAction("InventoryOverview", "Admin");
                }
                else
                {
                    stock stocksModel = db.stocks.Where(x => x.stock_id == model.stock_id).Single();
                    stocksModel.stock_quantity = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_quantity).FirstOrDefault() + model.toAdd;

                    db.stocks.Attach(stocksModel);
                    db.Entry(stocksModel).Property(x => x.stock_quantity).IsModified = true;

                    stockTran stockTransModel = new stockTran();
                    var itemName = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_name).FirstOrDefault();
                    var itemUnit = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_unit).FirstOrDefault();
                    stockTransModel.stockTrans_stock = model.stock_id;
                    stockTransModel.stockTrans_narration = "Restocked " + model.toAdd + " "+ model.stock_unit +" of "+ itemName;
                    stockTransModel.stockTrans_timestamp = DateTime.Now;
                    stockTransModel.stockTrans_type = "IN";
                    stockTransModel.stockTrans_value = model.toAdd;
                    db.stockTrans.Add(stockTransModel);

                    db.SaveChanges();
                    Session["StatusMsg"] = "Stock successfully restocked!";
                    return RedirectToAction("InventoryOverview", "Admin");
                }  
            }   
        }

        /* ADD STOCK */

        [HttpGet]
        public ActionResult AddStock()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStock(stock model)
        { 
                IMSDatabaseEntities db = new IMSDatabaseEntities();
                stock stk = new stock
                {
                    stock_name = model.stock_name,
                    stock_category = model.stock_category,
                    stock_quantity = model.stock_quantity,
                    stock_capacity = model.stock_capacity,
                    stock_unit = model.stock_unit,
                    stock_narration = model.stock_narration
                };

                if(stk.stock_name == "" | stk.stock_category == "" | stk.stock_quantity == null | stk.stock_capacity == null | stk.stock_unit == "" )
                {

                    Session["StatusMsg"] = "Please fill out all the required forms!";
                }
                else
                {
                db.stocks.Add(stk);

                stockTran stockTransModel = new stockTran();
                var itemName = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_name).FirstOrDefault();
                var itemUnit = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_unit).FirstOrDefault();
                stockTransModel.stockTrans_stock = model.stock_id;
                stockTransModel.stockTrans_narration = "Added " + itemName;
                stockTransModel.stockTrans_timestamp = DateTime.Now;
                stockTransModel.stockTrans_type = "Add";
                stockTransModel.stockTrans_value = model.stock_quantity;
                db.stockTrans.Add(stockTransModel);

                db.SaveChanges();
                Session["StatusMsg"] = "Stock successfully Added!";
            }
                
 
            return RedirectToAction("InventoryOverview", "Admin");
        }


        /* REMOVE STOCK */  
        [HttpPost] 
        public ActionResult RemoveStock(int id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                var stockObj = db.stocks.Where(x => x.stock_id == id).FirstOrDefault();
                if (stockObj != null)
                {
                    db.stocks.Remove(db.stocks.Where(x => x.stock_id == id).FirstOrDefault());
                    db.SaveChanges();
                    Session["StatusMsg"] = "Stock successfully Deleted!";
                }
            }
            return RedirectToAction("InventoryOverview", "Admin");
        } 

        /* MODIFY STOCK */ 
        [HttpGet]
        public ActionResult EditStock(int id)
        {
            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                return View(db.stocks.Where(x => x.stock_id == id).FirstOrDefault());
            }
        } 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStock(stock model)
        {
            if (ModelState.IsValid)
            {
                IMSDatabaseEntities db = new IMSDatabaseEntities();
                bool nameModified = true;
                bool catModified = true;
                bool qtyModified = true;
                bool unitModified = true;
                bool narModified = true;
                bool capacityModified = true;


                if (model.stock_name != null)
                {
                    nameModified = true;
                }
                else
                {
                    nameModified = false;
                }
                if (model.stock_category != null)
                {
                    catModified = true;
                }
                else
                {
                    catModified = false;
                }
                if (model.stock_quantity != null)
                {
                    qtyModified = true;
                }
                else
                {
                    qtyModified = false;
                }
                if (model.stock_unit != null)
                {
                    unitModified = true;
                }
                else
                {
                    unitModified = false;
                }
                if (model.stock_capacity != null)
                {
                    capacityModified = true;
                }
                else
                {
                    capacityModified = false;
                }
                if (model.stock_narration != null)
                {
                    narModified = true;
                }
                else
                {
                    narModified = false;
                }

                db.Entry(model).State = EntityState.Modified;

                if (nameModified == false)
                {
                    db.Entry(model).Property(x => x.stock_name).IsModified = false;
                }
                if (catModified == false)
                {
                    db.Entry(model).Property(x => x.stock_category).IsModified = false;
                }
                if (qtyModified == false)
                {
                    db.Entry(model).Property(x => x.stock_quantity).IsModified = false;
                }
                if (unitModified == false)
                {
                    db.Entry(model).Property(x => x.stock_unit).IsModified = false;
                }
                if (narModified == false)
                {
                    db.Entry(model).Property(x => x.stock_narration).IsModified = false;
                }
                if (capacityModified == false)
                {
                    db.Entry(model).Property(x => x.stock_narration).IsModified = false;
                }

                stockTran stockTransModel = new stockTran();
                var itemName = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_name).FirstOrDefault();
                var itemUnit = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_unit).FirstOrDefault();
                stockTransModel.stockTrans_stock = model.stock_id;
                stockTransModel.stockTrans_narration = "ADMIN EDITED " + itemName;
                stockTransModel.stockTrans_timestamp = DateTime.Now;
                stockTransModel.stockTrans_type = "EDIT";
                stockTransModel.stockTrans_value = 0; 
                db.stockTrans.Add(stockTransModel);
                db.SaveChanges();
                Session["StatusMsg"] = "Stock successfully edited!";
                return RedirectToAction("InventoryOverview", "Admin"); 
            }
            else
            {
                return View();
            } 
        }
        #endregion
    }
}