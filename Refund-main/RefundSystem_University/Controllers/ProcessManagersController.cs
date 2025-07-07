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
    public class ProcessManagersController : AuthenticatedBaseController
    {
        // GET: ProcessManagers
        public ActionResult Index()
        {
            var processManagers = db.ProcessManagers.Include(p => p.Department).Include(p => p.User);

            //For creating entity from modal
            ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Name");
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName");

            return View(processManagers.ToList());
        }

        // POST: ProcessManagers/Data
        [HttpPost]
        [OverrideAuthorization]
        [CustomAuthorize]
        public ActionResult Data()
        {
            return Json(db.ProcessManagers.Include(p => p.User).ToList().Select(x => new { x.Id, Name = x.User.UserName, Data = new { x.DepartmentId } }));
        }

        // GET: ProcessManagers/Edit/5
        public ActionResult _Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProcessManager processManager = db.ProcessManagers.Find(id);
            if (processManager == null)
            {
                return HttpNotFound();
            }
            ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Name", processManager.DepartmentId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", processManager.UserId);
            return PartialView("_Form", processManager);
        }

        // POST: ProcessManagers/Save
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "Id,DepartmentId,UserId")] ProcessManager processManager)
        {
            if (!ModelState.IsValid)
                return Json(new JsonResultData());

            try
            {
                if (processManager.Id > 0)
                    db.Entry(processManager).State = EntityState.Modified;
                else
                    db.ProcessManagers.Add(processManager);
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

        // POST: ProcessManagers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                ProcessManager processManager = db.ProcessManagers.Find(id);
                if (processManager == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                if (processManager.RefundApplications?.Any() ?? false)
                    return Json(new JsonResultData("פעולה שגויה - ישנן רשומות המשוייכות לרשומה זו"));
                db.ProcessManagers.Remove(processManager);
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
