using RefundSystem_University.Models;
using RefundSystem_University.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RefundSystem_University.Controllers
{
    public class LoginController : BaseController
    {
        // GET: Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            string returnUrl = "";
            if (Request.QueryString["ReturnUrl"] != null && Url.IsLocalUrl(Request.QueryString["ReturnUrl"]))
                returnUrl = Request.QueryString["ReturnUrl"].ToString();
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public ActionResult Login(string userName, string password, string returnUrl)
        {
            ViewError result;
            try
            {
                User user = db.Users.Include("AuthorizedSignatories").SingleOrDefault(u => u.UserName.Equals(userName) && u.Password.Equals(password));
                if (user != null)
                {
                    Session["User"] = user;

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    var controller = "RefundApplications";
                    return RedirectToAction("Index", controller);
                }
                result = new ViewError("שם משתמש או סיסמה אינם נכונים", null);
            }
            catch (Exception ex)
            {
                ViewBag.Exception = ex.ToString();
                result = new ViewError("ניסיון ההתחברות נכשל", ex);
            }
            return View(result);
        }

        public ActionResult Logout()
        {
            Session.RemoveAll();
            return RedirectToAction("Login");
        }
    }
}