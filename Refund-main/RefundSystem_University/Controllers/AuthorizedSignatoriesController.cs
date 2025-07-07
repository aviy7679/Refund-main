using RefundSystem_University.Models;
using RefundSystem_University.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace RefundSystem_University.Controllers
{
    public class AuthorizedSignatoriesController : AuthenticatedBaseController
    {
        // GET: AuthorizedSignatories
        public ActionResult Index()
        {
            var authorizedSignatories = db.AuthorizedSignatories.Include(a => a.User);
            return View(authorizedSignatories.ToList());
        }

        // GET: AuthorizedSignatories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuthorizedSignatory authorizedSignatory = db.AuthorizedSignatories.Find(id);
            if (authorizedSignatory == null)
            {
                return HttpNotFound();
            }
            return View(authorizedSignatory);
        }

        // GET: AuthorizedSignatories/Create
        public ActionResult Create()
        {
            var authorizedSignatory = new AuthorizedSignatory();
            SetOrderForCancellationTypeNotOnRegistrationDay(authorizedSignatory, true);
            SetOrderForCancellationTypeOnRegistrationDay(authorizedSignatory, true);
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName");
            ViewBag.Entities = new MultiSelectList(db.EntitiesTable, "Id", "Name");
            return View(authorizedSignatory);
        }

        // POST: AuthorizedSignatories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,JobType,UserId,CellPhone,IsForCancellationTypeNotOnRegistrationDay,OrderForCancellationTypeNotOnRegistrationDay," +
            "IsForCancellationTypeOnRegistrationDay,OrderForCancellationTypeOnRegistrationDay,SignaturePath")] AuthorizedSignatory authorizedSignatory, List<int> entitiesIds)
        {
            authorizedSignatory.SignaturePath = SaveSignature(authorizedSignatory.SignaturePath, "AuthorizedSignatoriesSignatures", string.Empty, true);

            if (ModelState.IsValid)
            {
                try
                {
                    SetOrderForCancellationTypeNotOnRegistrationDay(authorizedSignatory);
                    SetOrderForCancellationTypeOnRegistrationDay(authorizedSignatory);

                    if (entitiesIds != null)
                        foreach (var entityId in entitiesIds)
                        {
                            var entity = db.EntitiesTable.Find(entityId);
                            if (entity != null)
                                authorizedSignatory.Entities.Add(entity);
                        }

                    db.AuthorizedSignatories.Add(authorizedSignatory);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException e)
                {
                    DeleteFile(authorizedSignatory.SignaturePath); //TODO: Inform about failure???
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה עקב שגיאות אימות");
                    ViewBag.Exception = e.Message;
                }
                catch (Exception e)
                {
                    DeleteFile(authorizedSignatory.SignaturePath); //TODO: Inform about failure???
                    ViewBag.Exception = e.Message;
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה");
                }
            }

            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", authorizedSignatory.UserId);
            ViewBag.Entities = new SelectList(db.EntitiesTable, "Id", "Name");
            return View(authorizedSignatory);
        }

        private void SetOrderForCancellationTypeOnRegistrationDay(AuthorizedSignatory authorizedSignatory, bool forView = false)
        {
            byte? order = null;
            if (authorizedSignatory.IsForCancellationTypeOnRegistrationDay || forView)
            {
                order = (byte)(db.AuthorizedSignatories.Count(x => x.OrderForCancellationTypeOnRegistrationDay.HasValue) + 1);
                while (db.AuthorizedSignatories.Any(x => x.OrderForCancellationTypeOnRegistrationDay.HasValue && x.OrderForCancellationTypeOnRegistrationDay == order))
                {
                    order++;
                }
            }

            authorizedSignatory.OrderForCancellationTypeOnRegistrationDay = order;
        }

        private void SetOrderForCancellationTypeNotOnRegistrationDay(AuthorizedSignatory authorizedSignatory, bool forView = false)
        {
            byte? order = null;
            if (authorizedSignatory.IsForCancellationTypeNotOnRegistrationDay || forView)
            {
                order = (byte)(db.AuthorizedSignatories.Count(x => x.OrderForCancellationTypeNotOnRegistrationDay.HasValue) + 1);
                while (db.AuthorizedSignatories.Any(x => x.OrderForCancellationTypeNotOnRegistrationDay.HasValue && x.OrderForCancellationTypeNotOnRegistrationDay == order))
                {
                    order++;
                }
            }
            authorizedSignatory.OrderForCancellationTypeNotOnRegistrationDay = order;
        }

        // GET: AuthorizedSignatories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AuthorizedSignatory authorizedSignatory = db.AuthorizedSignatories.Find(id);
            if (authorizedSignatory == null)
            {
                return HttpNotFound();
            }

            if (!authorizedSignatory.OrderForCancellationTypeNotOnRegistrationDay.HasValue)
                SetOrderForCancellationTypeNotOnRegistrationDay(authorizedSignatory, true);
            else
                authorizedSignatory.IsForCancellationTypeNotOnRegistrationDay = true;

            if (!authorizedSignatory.OrderForCancellationTypeOnRegistrationDay.HasValue)
                SetOrderForCancellationTypeOnRegistrationDay(authorizedSignatory, true);
            else
                authorizedSignatory.IsForCancellationTypeOnRegistrationDay = true;

            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", authorizedSignatory.UserId);
            ViewBag.Entities = new MultiSelectList(db.EntitiesTable, "Id", "Name", authorizedSignatory.Entities.Select(x => x.Id));
            return View(authorizedSignatory);
        }

        // POST: AuthorizedSignatories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,JobType,UserId,CellPhone,IsForCancellationTypeNotOnRegistrationDay,OrderForCancellationTypeNotOnRegistrationDay," +
            "IsForCancellationTypeOnRegistrationDay,OrderForCancellationTypeOnRegistrationDay,SignaturePath,Entities")] AuthorizedSignatory authorizedSignatory, List<int> entitiesIds)
        {
            var oldAuthorizedSignatory = db.AuthorizedSignatories.AsNoTracking().FirstOrDefault(x => x.Id == authorizedSignatory.Id);

            var signaturePath = authorizedSignatory.SignaturePath != oldAuthorizedSignatory.SignaturePath ? 
                SaveSignature(authorizedSignatory.SignaturePath, "AuthorizedSignatoriesSignatures", string.Empty, false) : null;
            string oldSignaturePath = null;
            if (!string.IsNullOrEmpty(signaturePath))
            {
                oldSignaturePath = oldAuthorizedSignatory.SignaturePath;
                authorizedSignatory.SignaturePath = signaturePath;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.AuthorizedSignatories.Attach(authorizedSignatory);
                    db.Entry(authorizedSignatory).Collection(x => x.Entities).Load();
                    //var memberEntry = db.Entry(authorizedSignatory).Member("Entities");
                    //var collectionMember = memberEntry as DbCollectionEntry;
                    //collectionMember.Load();

                    foreach (var entity in authorizedSignatory.Entities.ToList())
                        if (!entitiesIds?.Contains(entity.Id) ?? true)
                            authorizedSignatory.Entities.Remove(entity);

                    if (entitiesIds != null)
                        foreach (var entityId in entitiesIds.Where(x => !authorizedSignatory.Entities.Any(e => e.Id == x)).ToList())
                        {
                            var entity = db.EntitiesTable.Find(entityId);
                            if (entity != null)
                                authorizedSignatory.Entities.Add(entity);
                        }

                    db.Entry(authorizedSignatory).State = EntityState.Modified;

                    if (oldAuthorizedSignatory.OrderForCancellationTypeNotOnRegistrationDay.HasValue == authorizedSignatory.IsForCancellationTypeNotOnRegistrationDay)
                        db.Entry(authorizedSignatory).Property(x => x.OrderForCancellationTypeNotOnRegistrationDay).IsModified = false;
                    else
                        SetOrderForCancellationTypeNotOnRegistrationDay(authorizedSignatory);

                    if (oldAuthorizedSignatory.OrderForCancellationTypeOnRegistrationDay.HasValue == authorizedSignatory.IsForCancellationTypeOnRegistrationDay)
                        db.Entry(authorizedSignatory).Property(x => x.OrderForCancellationTypeOnRegistrationDay).IsModified = false;
                    else
                        SetOrderForCancellationTypeOnRegistrationDay(authorizedSignatory);

                    db.SaveChanges();
                    DeleteFile(oldSignaturePath); //TODO: Inform about failure???
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException e)
                {
                    DeleteFile(signaturePath); //TODO: Inform about failure???
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה עקב שגיאות אימות");
                    ViewBag.Exception = e.Message;
                }
                catch (Exception e)
                {
                    DeleteFile(signaturePath); //TODO: Inform about failure???
                    ViewBag.Exception = e.Message;
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה");
                }
            }
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", authorizedSignatory.UserId);
            ViewBag.Entities = new MultiSelectList(db.EntitiesTable, "Id", "Name", authorizedSignatory.Entities.Select(x => x.Id));
            return View(authorizedSignatory);
        }

        // POST: AuthorizedSignatories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                AuthorizedSignatory authorizedSignatory = db.AuthorizedSignatories.Find(id);
                if (authorizedSignatory == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                if (authorizedSignatory.ApplicationApprovalStatus?.Any() ?? false)
                    return Json(new JsonResultData("פעולה שגויה - ישנן רשומות המשוייכות לרשומה זו"));
                if (!DeleteFile(authorizedSignatory.SignaturePath))
                    return Json(new JsonResultData("התרחשה שגיאה במחיקת החתימה"));

                foreach (var item in authorizedSignatory.Entities.ToList())
                    authorizedSignatory.Entities.Remove(item);
                
                db.AuthorizedSignatories.Remove(authorizedSignatory);
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
