using InventoryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq; 
using System.Web.Mvc;

namespace InventoryManagementSystem.Controllers
{
    public class EmployeeController : Controller
    {
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
                    foreach (var item in ingredients)
                    {
                        if (item != "")
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
                    price = "Php. " + db.products.Where(x => x.product_id == productModel.product_id).Select(x => x.product_price).FirstOrDefault() * productModel.product_price,
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
                    //Loop and insert records.
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
                                stockIds[ctr] = (db.stocks.Where(x => x.stock_name == i).Select(x => x.stock_id).FirstOrDefault());
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

                        //Update stocks 
                        stockTran stockTransModel = new stockTran();
                        foreach (var item in stockIds)
                        {
                            var itemName = db.stocks.Where(x => x.stock_id == item).Select(x => x.stock_name).FirstOrDefault();
                            var itemUnit = db.stocks.Where(x => x.stock_id == item).Select(x => x.stock_unit).FirstOrDefault();
                            stockTransModel.stockTrans_stock = (byte)stockIds[ctr];
                            stockTransModel.stockTrans_narration = itemName + " used " + prodArray[ctr] * o.order_quantity + " " + itemUnit;
                            stockTransModel.stockTrans_timestamp = DateTime.Now;
                            stockTransModel.stockTrans_type = "minus";

                            stock stocksModel = db.stocks.Find((byte)stockIds[ctr]);
                            stocksModel.stock_quantity -= prodArray[ctr] * o.order_quantity;

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
                    stockModel.percentage = (decimal)(db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_quantity).FirstOrDefault() / db.stocks.Where(x => x.stock_id == i).Select(x => x.stock_capacity).FirstOrDefault()) * 100;
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
                var stockQuantity = (decimal)db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_quantity).FirstOrDefault() + model.toAdd;
                var stockCapacity = (decimal)db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_capacity).FirstOrDefault();
                if (stockQuantity > stockCapacity)
                {
                    Session["StatusMsg"] = "Stock cant exceed max capacity";
                    return RedirectToAction("InventoryOverview", "Employee");
                }
                else
                {
                    stock stocksModel = db.stocks.Find(model.stock_id);
                    stocksModel.stock_quantity = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_quantity).FirstOrDefault() + model.toAdd;

                    db.stocks.Attach(stocksModel);
                    db.Entry(stocksModel).Property(x => x.stock_quantity).IsModified = true;

                    stockTran stockTransModel = new stockTran();
                    var itemName = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_name).FirstOrDefault();
                    var itemUnit = db.stocks.Where(x => x.stock_id == model.stock_id).Select(x => x.stock_unit).FirstOrDefault();
                    stockTransModel.stockTrans_stock = model.stock_id;
                    stockTransModel.stockTrans_narration = "Restocked " + model.toAdd + " " + model.stock_unit + " of " + itemName;
                    stockTransModel.stockTrans_timestamp = DateTime.Now;
                    stockTransModel.stockTrans_type = "IN";
                    stockTransModel.stockTrans_value = model.toAdd;
                    db.stockTrans.Add(stockTransModel);

                    db.SaveChanges();
                    Session["StatusMsg"] = "Stock successfully restocked!";
                    return RedirectToAction("InventoryOverview", "Employee");
                }
            }
        }

        [HttpGet]
        public ActionResult AddStock()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStock(stock model)
        {
            if (ModelState.IsValid)
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
            else
            {
                Session["StatusMsg"] = "Saving failed please fillup the required forms!";
            } 
            return RedirectToAction("InventoryOverview", "Employee");
        }

        #endregion
    }
}