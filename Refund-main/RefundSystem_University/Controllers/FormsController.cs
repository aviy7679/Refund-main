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
    public class FormsController : AuthenticatedBaseController
    {
        // GET: Forms
        public ActionResult Index()
        {
            var forms = db.Forms.Include(f => f.CancellationType).Include(f => f.Entity);

            //For creating entity from modal
            ViewBag.CancellationTypeId = new SelectList(db.CancellationTypes, "Id", "Name");
            ViewBag.EntityId = new SelectList(db.EntitiesTable, "Id", "Name");

            return View(forms.ToList());
        }

        // POST: Forms/Data
        [HttpPost]
        [OverrideAuthorization]
        [CustomAuthorize]
        public ActionResult Data()
        {
            return Json(db.Forms.ToList().Select(x => new { x.Id, x.Name, Data = new { x.EntityId, x.PaymentMethod } }));
        }

        // GET: Forms/Edit/5
        public ActionResult _Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Form form = db.Forms.Find(id);
            if (form == null)
            {
                return HttpNotFound();
            }
            ViewBag.CancellationTypeId = new SelectList(db.CancellationTypes, "Id", "Name", form.CancellationTypeId);
            ViewBag.EntityId = new SelectList(db.EntitiesTable, "Id", "Name", form.EntityId);
            return PartialView("_Form", form);
        }

        // POST: Forms/Save
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "Id,Name,CancellationTypeId,EntityId,PaymentMethod")] Form form)
        {
            if (!ModelState.IsValid)
                return Json(new JsonResultData());

            try
            {
                if (form.Id > 0)
                    db.Entry(form).State = EntityState.Modified;
                else
                    db.Forms.Add(form);
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

        // POST: Forms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Form form = db.Forms.Find(id);
                if (form == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                db.Forms.Remove(form);
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
