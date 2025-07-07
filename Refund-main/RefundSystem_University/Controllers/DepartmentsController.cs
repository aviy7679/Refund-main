using RefundSystem_University.Attributes;
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
    public class DepartmentsController : AuthenticatedBaseController
    {
        // GET: Departments
        public ActionResult Index()
        {
            var departments = db.Departments.Include(d => d.Entity);

            //For creating entity from modal
            ViewBag.EntityId = new SelectList(db.EntitiesTable, "Id", "Name");

            return View(departments.ToList());
        }

        // POST: Departments/Data
        [HttpPost]
        [OverrideAuthorization]
        [CustomAuthorize]
        public ActionResult Data()
        {
            return Json(db.Departments.ToList().Select(x => new { x.Id, x.Name, Data = new { x.EntityId } }));
        }

        // GET: Departments/Edit/5
        public ActionResult _Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            ViewBag.EntityId = new SelectList(db.EntitiesTable, "Id", "Name", department.EntityId);
            return PartialView("_Form", department);
        }

        // POST: Departments/Save
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "Id,Name,EntityId")] Department department)
        {
            if (!ModelState.IsValid)
                return Json(new JsonResultData());

            try
            {
                if (department.Id > 0)
                    db.Entry(department).State = EntityState.Modified;
                else
                    db.Departments.Add(department);
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

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Department department = db.Departments.Find(id);
                if (department == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                if ((department.DepartmentManagers?.Any() ?? false) || (department.ProcessManagers?.Any() ?? false) || (department.RefundApplications?.Any() ?? false))
                    return Json(new JsonResultData("פעולה שגויה - ישנן רשומות המשוייכות לרשומה זו"));
                db.Departments.Remove(department);
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
