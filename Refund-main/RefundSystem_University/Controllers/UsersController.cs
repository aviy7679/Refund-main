using RefundSystem_University.Models;
using RefundSystem_University.ViewModels;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace RefundSystem_University.Controllers
{
    public class UsersController : AuthenticatedBaseController
    {
        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserName,Password,IsAdmin,Email")] User user)
        {
            if (db.Users.Any(x => x.UserName == user.UserName))
                ModelState.AddModelError("", "שם משתמש קיים, הזן שם יחודי");

            if (ModelState.IsValid)
            {
                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException e)
                {
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה עקב שגיאות אימות");
                    ViewBag.Exception = e.Message;
                }
                catch (Exception e)
                {
                    ViewBag.Exception = e.Message;
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה");
                }
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,Password,IsAdmin,Email")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Users.Attach(user);
                    db.Entry(user).State = EntityState.Modified;
                    db.Entry(user).Property(x => x.UserName).IsModified = false;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException e)
                {
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה עקב שגיאות אימות");
                    ViewBag.Exception = e.Message;
                }
                catch (Exception e)
                {
                    ViewBag.Exception = e.Message;
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה");
                }
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                User user = db.Users.Find(id);
                if (user == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                if ((user.AuthorizedSignatories?.Any() ?? false) || (user.RefundApplications?.Any() ?? false) || (user.DepartmentManagers?.Any() ?? false) || (user.ProcessManagers?.Any() ?? false))
                    return Json(new JsonResultData("פעולה שגויה - ישנן רשומות המשוייכות לרשומה זו"));
                db.Users.Remove(user);
                db.SaveChanges();
                return Json(new JsonResultData(true));
            }
            catch (Exception e)
            {
                return Json(new JsonResultData("התרחשה שגיאה במהלך המחיקה", e));
            }            
        }
    }
}
