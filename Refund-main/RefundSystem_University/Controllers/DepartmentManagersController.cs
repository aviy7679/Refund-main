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
    public class DepartmentManagersController : AuthenticatedBaseController
    {
        // GET: DepartmentManagers
        public ActionResult Index()
        {
            var departmentManagers = db.DepartmentManagers.Include(d => d.Department).Include(d => d.User).Include(d => d.AuthorizedSignatory);

            //For creating entity from modal
            ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Name");
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName");
            ViewBag.AuthorizedSignatoryId = new SelectList(db.AuthorizedSignatories, "Id", "User.UserName");

            return View(departmentManagers.ToList());
        }

        // GET: DepartmentManagers/Edit/5
        public ActionResult _Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentManager departmentManager = db.DepartmentManagers.Find(id);
            if (departmentManager == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Name", departmentManager.DepartmentId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", departmentManager.UserId);
            ViewBag.AuthorizedSignatoryId = new SelectList(db.AuthorizedSignatories, "Id", "User.UserName", departmentManager.AuthorizedSignatoryId);

            return PartialView("_Form", departmentManager);
        }

        // POST: DepartmentManagers/Save
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "Id,DepartmentId,UserId,AuthorizedSignatoryId")] DepartmentManager departmentManager)
        {
            if (!ModelState.IsValid)
                return Json(new JsonResultData());

            try
            {
                if (departmentManager.Id > 0)
                    db.Entry(departmentManager).State = EntityState.Modified;
                else
                    db.DepartmentManagers.Add(departmentManager);
                db.SaveChanges();
                return Json(new JsonResultData(true));
            }
            catch (DbEntityValidationException e)
            {
                return Json(new JsonResultData("שגיאה התרחשה במהלך השמירה עקב שגיאות אימות", e));
            }
            catch (Exception e)
            {
                return Json(new JsonResultData("שגיאה התרחשה במהלך השמירה", e));
            }
        }

        // POST: DepartmentManagers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                DepartmentManager departmentManager = db.DepartmentManagers.Find(id);
                if (departmentManager == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                db.DepartmentManagers.Remove(departmentManager);
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
