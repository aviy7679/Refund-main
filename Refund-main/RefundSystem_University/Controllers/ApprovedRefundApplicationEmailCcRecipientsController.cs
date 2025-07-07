using RefundSystem_University.Models;
using RefundSystem_University.ViewModels;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace RefundSystem_University.Controllers
{
    public class ApprovedRefundApplicationEmailCcRecipientsController : AuthenticatedBaseController
    {
        // GET: ApprovedRefundApplicationEmailCcRecipients
        public ActionResult Index()
        {
            return View(db.ApprovedRefundApplicationEmailCcRecipients.ToList());
        }

        // POST: ApprovedRefundApplicationEmailCcRecipients/Save
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "Id,Email")] ApprovedRefundApplicationEmailCcRecipient approvedRefundApplicationEmailCcRecipient)
        {
            if (!ModelState.IsValid)
                return Json(new JsonResultData());

            try
            {
                db.ApprovedRefundApplicationEmailCcRecipients.Add(approvedRefundApplicationEmailCcRecipient);
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

        // POST: ApprovedRefundApplicationEmailCcRecipients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                ApprovedRefundApplicationEmailCcRecipient approvedRefundApplicationEmailCcRecipient = db.ApprovedRefundApplicationEmailCcRecipients.Find(id);
                if (approvedRefundApplicationEmailCcRecipient == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                db.ApprovedRefundApplicationEmailCcRecipients.Remove(approvedRefundApplicationEmailCcRecipient);
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
