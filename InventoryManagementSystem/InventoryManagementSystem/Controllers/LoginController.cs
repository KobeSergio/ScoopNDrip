using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Controllers;

namespace InventoryManagementSystem.Controllers
{

    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Autherize(user u)
        {

            using (IMSDatabaseEntities db = new IMSDatabaseEntities())
            {
                var userDetails = db.users.Where(x => x.user_username == u.user_username && x.user_pass == u.user_pass).FirstOrDefault();
                if (userDetails == null)
                {


                    u.LoginErrorMessage = "Wrong username or password.";
                    return View("Index", u);

                }
                else
                {
                    //////////////////////////////////////////////////////
                    //this part is rogi's code, implement on final system        
 
                    Session["userNickname"] = userDetails.user_nickname;
                    Session["userID"] = userDetails.user_id.ToString();
                    
                    if(userDetails.user_img != null)
                    {
                        var base64 = Convert.ToBase64String(userDetails.user_img);
                        Session["userImage"] = string.Format("data:image/gif; base64, {0}", base64);
                    }
                     
                    if (userDetails.user_role.ToLower() == "admin")
                    //up to here
                    ///////////////////////////////////////////////////
                    {
                        Session["userRole"] = "Admin";
                        return RedirectToAction("InventoryDashboard", "Admin");
                    }
                    else
                        Session["userRole"] = "Employee";
                    return RedirectToAction("PointOfSales", "Employee");

                }
            }



        }


        public ActionResult SignOut(user u)
        { 
            Session.Abandon();
            return RedirectToAction("Index", "Login");

        }


    }
}
