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
    public class CancellationTypesController : AuthenticatedBaseController
    {
        // GET: CancellationTypes
        public ActionResult Index()
        {
            return View(db.CancellationTypes.ToList());
        }

        // GET: CancellationTypes/Edit/5
        public ActionResult _Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CancellationType cancellationType = db.CancellationTypes.Find(id);
            if (cancellationType == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Form", cancellationType);
        }

        // POST: CancellationTypes/Save
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "Id,Name,OnRegistrationDay")] CancellationType cancellationType)
        {
            if (!ModelState.IsValid)
                return Json(new JsonResultData());

            try
            {
                if (cancellationType.Id > 0)
                    db.Entry(cancellationType).State = EntityState.Modified;
                else
                    db.CancellationTypes.Add(cancellationType);
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

        // POST: CancellationTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                CancellationType cancellationType = db.CancellationTypes.Find(id);
                if (cancellationType == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                db.CancellationTypes.Remove(cancellationType);
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
