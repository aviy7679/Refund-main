using RefundSystem_University.Models;
using RefundSystem_University.ViewModels;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RefundSystem_University.Controllers
{
    public class EntitiesController : AuthenticatedBaseController
    {
        // GET: Entities
        public ActionResult Index()
        {
            return View(db.EntitiesTable.ToList());
        }

        // GET: Entities/Edit/5
        public ActionResult _Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Entity entity = db.EntitiesTable.Find(id);
            if (entity == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Form", entity);
        }

        // POST: Entities/Save
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save([Bind(Include = "Id,Name,LogoPath")] Entity entity, HttpPostedFileBase file)
        {
            var logoPath = SaveFile(file, "EntitiesLogos", string.Empty, string.IsNullOrEmpty(entity.LogoPath));
            string oldLogoPath = null;
            if (!string.IsNullOrEmpty(logoPath))
            {
                oldLogoPath = entity.LogoPath;
                entity.LogoPath = logoPath;
            }
            if (!ModelState.IsValid)
                return Json(new JsonResultData()); //TODO: Add error details

            try
            {
                if (entity.Id > 0)
                    db.Entry(entity).State = EntityState.Modified;
                else
                    db.EntitiesTable.Add(entity);
                db.SaveChanges();
                DeleteFile(oldLogoPath); //TODO: Inform about failure???
                return Json(new JsonResultData(true));
            }
            catch (DbEntityValidationException e)
            {
                DeleteFile(logoPath); //TODO: Inform about failure???
                return Json(new JsonResultData("שגיאה התרחשה במהלך השמירה עקב שגיאות אימות", e));
            }
            catch (Exception e)
            {
                DeleteFile(logoPath); //TODO: Inform about failure???
                return Json(new JsonResultData("שגיאה התרחשה במהלך השמירה", e));
            }
        }

        // POST: Entities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Entity entity = db.EntitiesTable.Find(id);
                if (entity == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                if ((entity.Departments?.Any() ?? false) || (entity.Forms?.Any() ?? false))
                    return Json(new JsonResultData("פעולה שגויה - ישנן רשומות המשוייכות לרשומה זו"));
                if (!DeleteFile(entity.LogoPath))
                    return Json(new JsonResultData("התרחשה שגיאה במחיקת הלוגו"));
                db.EntitiesTable.Remove(entity);
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
