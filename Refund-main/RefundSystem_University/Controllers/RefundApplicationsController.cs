using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RefundSystem_University.Models;
using RefundSystem_University.ViewModels;
using System.Data.Entity.Validation;
using RefundSystem_University.Models.Enums;
using System.Net.Mail;
using System.Configuration;
using RefundSystem_University.Attributes;
using RefundSystem_University.Services;

namespace RefundSystem_University.Controllers
{
    [CustomAuthorize]
    public class RefundApplicationsController : AuthenticatedBaseController
    {
        // GET: RefundApplications
        public ActionResult Index()
        {
            var refundApplications = db.RefundApplications.Include(r => r.Department).Include(r => r.Entity).Include(r => r.Form).Include(r => r.ProcessManager).Include(r => r.User).Include(r => r.ApplicationApprovalStatus);
            return View(refundApplications.ToList());
        }

        // GET: RefundApplications/Details/5
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RefundApplication refundApplication = db.RefundApplications.Find(id);
            if (refundApplication == null)
            {
                return HttpNotFound();
            }

            var authorizedSignatories = GetAuthorizedSignatories(refundApplication);
            ViewBag.AuthorizedSignatories = authorizedSignatories;
            var nextAuthorizedSignatory = GetNextAuthorizedSignatory(refundApplication, authorizedSignatories);
            if (nextAuthorizedSignatory != null && nextAuthorizedSignatory.Id == User?.AuthorizedSignatories?.FirstOrDefault()?.Id)
                ViewBag.AuthorizedSignatory = nextAuthorizedSignatory;

            return View(refundApplication);
        }

        private static readonly EmailService MailService = new EmailService(
            "isupptest22@gmail.com",
            ConfigurationManager.AppSettings["GmailPassword"]?.ToString() ?? "ffdfvvsfawziqvdg",
            "isupptest22@gmail.com",
            "מערכת החזרים"
        );

        private static readonly SMSService SmsService = new SMSService(
            ConfigurationManager.AppSettings["TwilioAccountSid"],
            ConfigurationManager.AppSettings["TwilioAuthToken"],
            ConfigurationManager.AppSettings["TwilioFromNumber"]
        );

        // GET: RefundApplications/Create
        public ActionResult Create()
        {
            var entities = db.EntitiesTable.ToList();
            ViewBag.EntityId = new SelectList(entities, "Id", "Name");
            //var departments = entities.Any() ? db.Departments.Where(x => x.EntityId == entities.First().Id).ToList() : new List<Department>();
            //ViewBag.DepartmentId = new SelectList(departments, "Id", "Name");
            //ViewBag.FormId = new SelectList(db.Forms, "Id", "Name");
            //ViewBag.ProcessManagerId = new SelectList(db.ProcessManagers, "Id", "User.UserName");
            ViewBag.UserId = new SelectList(User.IsAdmin ? db.Users.ToArray() : new User[] { User }, "Id", "UserName", User.Id);
            return View();
        }

        // POST: RefundApplications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,EntityId,FormId,UserId,DepartmentId,ProcessManagerId,CustomerName,CustomerIdNumber,CreditLastDigits,RefundMethod,AccountOwnerName,BankNumber," +
            "BranchNumber,AccountNumber,TransactionDate,TransactionAmount,CancellationReason,FullCancellation,RefundAmount,AdditionalCredit,Details,Remarks,Date")]
            RefundApplication refundApplication, IEnumerable<HttpPostedFileBase> files)
        {
            //בודק את נתוני הטופס ושומר את הקבצים
            var form = db.Forms.Find(refundApplication.FormId);
            if (form == null)
                ModelState.AddModelError("FormId", "זהו שדה חובה");
            else if(form.PaymentMethod == (byte)PaymentMethod.Cheque && !refundApplication.RefundMethod.HasValue)
                ModelState.AddModelError("RefundMethod", "זהו שדה חובה");
            else if (form.PaymentMethod == (byte)PaymentMethod.Credit && refundApplication.CreditLastDigits?.Length != 4)
                ModelState.AddModelError("CreditLastDigits", "זהו שדה חובה באורך 4 תווים");
            if (refundApplication.AdditionalCredit && string.IsNullOrEmpty(refundApplication.Details))
                ModelState.AddModelError("Details", "השדה פירוט הוא חובה כאשר קיים זיכוי נוסף");
            if (refundApplication.RefundAmount > refundApplication.TransactionAmount)
                ModelState.AddModelError("", "סכום העסקה חייב להיות גדול מסכום ההחזר");
            if(!files?.Any() ?? false)
                ModelState.AddModelError("RefundApplicationFiles", "חובה לצרף קבלה");

            if (refundApplication.RefundApplicationFiles == null)
                refundApplication.RefundApplicationFiles = new List<RefundApplicationFile>();
            foreach (var item in files)
                refundApplication.RefundApplicationFiles.Add(new RefundApplicationFile(SaveFile(item, "RefundApplicationFiles", string.Empty, true)));
            //בודק שנתוני הטופס שהוזנו תקינים
            if (ModelState.IsValid)
            {
                try
                {
                    //הוספת הבקשה למסד בנתונים ושמירה 
                    db.RefundApplications.Add(refundApplication);
                    db.SaveChanges();
                    //
                    refundApplication.Form = form;
                    db.Entry(refundApplication).Reference(x => x.Department).Load();
                    // שליחה לחותם הבא בתור
                    var result = NotifyNextAuthorizedSignatory(refundApplication);
                    //הודעה אם קרתה שגיאה בשליחה של הבקשה והדפסה בויו באג
                    if (!result.Success)
                    {
                        ViewBag.Message = result.Message;
                        ViewBag.Exception = result.Exception;
                    }
                    // חזרה לעמוד הבית במקרה של הצלחה
                    return RedirectToAction("Index");
                }
                // בדיקה של שגיאת אימות אם חסרים פרטים
                catch (DbEntityValidationException e)
                {
                    //מחיקת הקבצים שנשמרו קודם (בעיקבת אי שמירת הבקשה
                    foreach (var item in refundApplication.RefundApplicationFiles)
                        DeleteFile(item.FilePath); //TODO: Inform about failure???
                    refundApplication.RefundApplicationFiles = null;
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה עקב שגיאות אימות");
                    ViewBag.Exception = e.Message;
                }
                //שאר השגיאות
                catch (Exception e)
                {
                    //מחיקת הקבצים שנשמרו קודם
                    foreach (var item in refundApplication.RefundApplicationFiles)
                        DeleteFile(item.FilePath); //TODO: Inform about failure???
                    refundApplication.RefundApplicationFiles = null;
                    ViewBag.Exception = e.Message;
                    ModelState.AddModelError("", "שגיאה התרחשה במהלך השמירה");
                }
            }

            //מנקה את רשימת קובצי הבקשבה
            refundApplication.RefundApplicationFiles = null;
            //ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Name", refundApplication.DepartmentId);
            ViewBag.EntityId = new SelectList(db.EntitiesTable, "Id", "Name", refundApplication.EntityId);
            //ViewBag.FormId = new SelectList(db.Forms, "Id", "Name", refundApplication.FormId);
            //ViewBag.ProcessManagerId = new SelectList(db.ProcessManagers, "Id", "Id", refundApplication.ProcessManagerId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", refundApplication.UserId);
            return View(refundApplication);
        }

        private JsonResultData NotifyNextAuthorizedSignatory(RefundApplication refundApplication)
        {
            var url = $"/RefundApplications/Details/{refundApplication.Id}";

            string message = string.Empty;
            string exception = string.Empty;
            string errorMessage;
            AuthorizedSignatory nextAuthorizedSignatory = GetNextAuthorizedSignatory(refundApplication);
            if (nextAuthorizedSignatory != null)
            {
                //Send email
                var isSent = MailService.SendEmail(
                    new string[] { nextAuthorizedSignatory.User.Email },
                    "בקשה להחזר ממתינה לאישורך",
                    NotifyAboutRefundApplicationAwaitingSignatureBody(nextAuthorizedSignatory.Name, url),
                    out errorMessage
                );
                errorMessage = string.Empty;
                if (!string.IsNullOrEmpty(errorMessage) || !isSent)
                {
                    message += "בקשתך התקבלה בהצלחה, אך ארעה שגיאה בעדכון החותם הבא בדוא\"ל שבקשה להחזר ממתינה לאישורו. ";
                    exception += errorMessage;
                }

                //Send sms
                isSent = SmsService.SendSMS(
                    nextAuthorizedSignatory.CellPhone,
                    NotifyAboutRefundApplicationAwaitingSignatureText(nextAuthorizedSignatory.Name, url),
                    out errorMessage
                );
                if (!string.IsNullOrEmpty(errorMessage) || !isSent)
                {
                    message += "בקשתך התקבלה בהצלחה, אך ארעה שגיאה בעדכון החותם הבא במסרון שבקשה להחזר ממתינה לאישורו. ";
                    exception += errorMessage;
                }
            }
            else
            {
                var isSent = MailService.SendEmail(
                    db.ApprovedRefundApplicationEmailCcRecipients.Select(x => x.Email).ToArray(),
                    "בקשה להחזר אושרה ונחתמה סופית",
                    NotifyAboutApprovedRefundApplicationBody(url),
                    out errorMessage
                );
                errorMessage = string.Empty;
                if (!string.IsNullOrEmpty(errorMessage) || !isSent)
                {
                    message += "בקשתך התקבלה בהצלחה, אך ארעה שגיאה בעדכון שהבקשה אושרה ונחתמה סופית";
                    exception += errorMessage;
                }
            }

            return (string.IsNullOrWhiteSpace(message) ? new JsonResultData(true) : new JsonResultData(message) { Exception = exception });
        }

        private JsonResultData NotifyAboutRejectedRefundApplication(RefundApplication refundApplication, string nonApprovalReason)
        {
            var url = $"/RefundApplications/Details/{refundApplication.Id}";
            string errorMessage;
            //Send email
            var isSent = MailService.SendEmail(
             new string[] { refundApplication.User.Email },
             "בקשה להחזר נדחתה",
             NotifyAboutRejectedRefundApplicationBody(refundApplication.User.UserName, nonApprovalReason, url),
             out errorMessage
         );
            errorMessage = string.Empty;
            if (!string.IsNullOrEmpty(errorMessage) || !isSent)
                return new JsonResultData("בקשתך התקבלה בהצלחה, אך ארעה שגיאה בעדכון מגיש הבקשה שהבקשה נדחתה") { Exception = errorMessage };
            return new JsonResultData(true);
        }

        private AuthorizedSignatory GetNextAuthorizedSignatory(RefundApplication refundApplication, List<AuthorizedSignatory> authorizedSignatories = null)
        {
            if (refundApplication.ApplicationApprovalStatus.Any(aas => !aas.Approved))
                return null;
            //Authorized signatories list
            authorizedSignatories = authorizedSignatories ?? GetAuthorizedSignatories(refundApplication);

            var nextAuthorizedSignatory = authorizedSignatories.FirstOrDefault(x => !refundApplication.ApplicationApprovalStatus.Any(aas => aas.AuthorizedSignatoryId == x.Id));
            return nextAuthorizedSignatory;
        }

        private List<AuthorizedSignatory> GetAuthorizedSignatories(RefundApplication refundApplication)
        {
            var authorizedSignatories = new List<AuthorizedSignatory>();
            var departmentManagerAuthorizedSignatory = refundApplication?.Department?.DepartmentManagers?.FirstOrDefault()?.AuthorizedSignatory;
            if (departmentManagerAuthorizedSignatory != null)
                authorizedSignatories.Add(departmentManagerAuthorizedSignatory);
            var authorizedSignatoriesQuery = db.AuthorizedSignatories.Include(x => x.User).Where(x => x.Entities.Any(e => e.Id == refundApplication.EntityId));
            if (refundApplication.Form.CancellationType.OnRegistrationDay)
                authorizedSignatoriesQuery = authorizedSignatoriesQuery.Where(x => x.OrderForCancellationTypeOnRegistrationDay.HasValue)
                    .OrderBy(x => x.OrderForCancellationTypeOnRegistrationDay);
            else
                authorizedSignatoriesQuery = authorizedSignatoriesQuery.Where(x => x.OrderForCancellationTypeNotOnRegistrationDay.HasValue)
                    .OrderBy(x => x.OrderForCancellationTypeNotOnRegistrationDay);
            authorizedSignatories.AddRange(authorizedSignatoriesQuery.ToList());
            return authorizedSignatories;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveApplicationApprovalStatus([Bind(Include = "Id,RefundApplicationId,AuthorizedSignatoryId,Approved,Date,NonApprovalReason")]
            ApplicationApprovalStatu applicationApprovalStatus)
        {
            if (!applicationApprovalStatus.Approved && string.IsNullOrWhiteSpace(applicationApprovalStatus.NonApprovalReason))
                ModelState.AddModelError("RefundMethod", "זהו שדה חובה");
            if (!ModelState.IsValid)
                return Json(new JsonResultData()); //TODO: Add error details

            try
            {
                var refundApplication = db.RefundApplications.Find(applicationApprovalStatus.RefundApplicationId);
                AuthorizedSignatory nextAuthorizedSignatory = GetNextAuthorizedSignatory(refundApplication);
                var authorizedSignatoryId = User.AuthorizedSignatories.FirstOrDefault().Id;
                if (nextAuthorizedSignatory.Id != authorizedSignatoryId)
                    return Json(new JsonResultData("פעולה שגויה - הגישה נדחתה"));
                applicationApprovalStatus.AuthorizedSignatoryId = authorizedSignatoryId;
                applicationApprovalStatus.Date = DateTime.Now;
                db.ApplicationApprovalStatus.Add(applicationApprovalStatus);
                db.SaveChanges();

                if (!applicationApprovalStatus.Approved)
                    return Json(NotifyAboutRejectedRefundApplication(refundApplication, applicationApprovalStatus.NonApprovalReason));

                db.Entry(refundApplication).Collection(x => x.ApplicationApprovalStatus).Load();
                return Json(NotifyNextAuthorizedSignatory(refundApplication));
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

        private string NotifyAboutRefundApplicationAwaitingSignatureBody(string authorizedSignatoryName, string url)
        {
            string body =
                 "<div dir='rtl' style='min-width: 332px;max-width: 600px;border: 1px solid #f0f0f0;border-bottom: 1px solid #c0c0c0;border-top: 0;border-bottom-left-radius: 3px;border-bottom-right-radius: 3px;font-size:11pt;font-family:Arial,sans-serif;color:rgb(31,73,125)padding: 15px;'>" +
                    $"<b>{authorizedSignatoryName.Trim()} שלום רב,</b>" +
                    "<br />" +
                    "<br />" +
                    $"ניתן לצפות ולאשר את הבקשה בקישור {Request.Url.GetLeftPart(UriPartial.Authority)}{url}." +
                    "<br />" +
                    "<br />" +
                    "מערכת החזרים." +
                    "<br />" +
                "</div>";

            return body;
        }     

        private string NotifyAboutRefundApplicationAwaitingSignatureText(string authorizedSignatoryName, string url)
        {
            string text = $"{authorizedSignatoryName.Trim()} שלום רב,\n" +
                "בקשה להחזר ממתינה לאישורך\n" +
                $"ניתן לצפות ולאשר את הבקשה בקישור {Request.Url.GetLeftPart(UriPartial.Authority)}{url}.\n" +
                "מערכת החזרים.";
            return text;
        }

        private string NotifyAboutApprovedRefundApplicationBody(string url)
        {
            string body =
                 "<div dir='rtl' style='min-width: 332px;max-width: 600px;border: 1px solid #f0f0f0;border-bottom: 1px solid #c0c0c0;border-top: 0;border-bottom-left-radius: 3px;border-bottom-right-radius: 3px;font-size:11pt;font-family:Arial,sans-serif;color:rgb(31,73,125)padding: 15px;'>" +
                    $"<b>שלום רב,</b>" +
                    "<br />" +
                    "<br />" +
                    $"ניתן לצפות בבקשה בקישור {Request.Url.GetLeftPart(UriPartial.Authority)}{url}." +
                    "<br />" +
                    "<br />" +
                    "מערכת החזרים." +
                    "<br />" +
                "</div>";

            return body;
        }

        private string NotifyAboutRejectedRefundApplicationBody(string userName, string nonApprovalReason, string url)
        {
            string body =
                 "<div dir='rtl' style='min-width: 332px;max-width: 600px;border: 1px solid #f0f0f0;border-bottom: 1px solid #c0c0c0;border-top: 0;border-bottom-left-radius: 3px;border-bottom-right-radius: 3px;font-size:11pt;font-family:Arial,sans-serif;color:rgb(31,73,125)padding: 15px;'>" +
                     $"<b>{userName.Trim()} שלום רב,</b>" +
                    "<br />" +
                    "<br />" +
                    $"סיבת דחיית הבקשה: {nonApprovalReason}. " +
                    "<br />" +
                    "<br />" +
                    $"ניתן לצפות בבקשה בקישור {Request.Url.GetLeftPart(UriPartial.Authority)}{url}." +
                    "<br />" +
                    "<br />" +
                    "מערכת החזרים." +
                    "<br />" +
                "</div>";

            return body;
        }

        //// GET: RefundApplications/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    RefundApplication refundApplication = db.RefundApplications.Find(id);
        //    if (refundApplication == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    if (refundApplication.ApplicationApprovalStatus?.Any() ?? false)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    //ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Name", refundApplication.DepartmentId);
        //    ViewBag.EntityId = new SelectList(db.EntitiesTable, "Id", "Name", refundApplication.EntityId);
        //    //ViewBag.FormId = new SelectList(db.Forms, "Id", "Name", refundApplication.FormId);
        //    //ViewBag.ProcessManagerId = new SelectList(db.ProcessManagers, "Id", "Id", refundApplication.ProcessManagerId);
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", refundApplication.UserId);
        //    return View(refundApplication);
        //}

        //// POST: RefundApplications/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,EntityId,FormId,UserId,DepartmentId,ProcessManagerId,CustomerName,CustomerIdNumber,CreditLastDigits,RefundMethod,AccountOwnerName,BankNumber,BranchNumber,AccountNumber,TransactionDate,TransactionAmount,CancellationReason,FullCancellation,RefundAmount,AdditionalCredit,Details,Remarks,Date")] RefundApplication refundApplication)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(refundApplication).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    //ViewBag.DepartmentId = new SelectList(db.Departments, "Id", "Name", refundApplication.DepartmentId);
        //    ViewBag.EntityId = new SelectList(db.EntitiesTable, "Id", "Name", refundApplication.EntityId);
        //    //ViewBag.FormId = new SelectList(db.Forms, "Id", "Name", refundApplication.FormId);
        //    //ViewBag.ProcessManagerId = new SelectList(db.ProcessManagers, "Id", "Id", refundApplication.ProcessManagerId);
        //    ViewBag.UserId = new SelectList(db.Users, "Id", "UserName", refundApplication.UserId);
        //    return View(refundApplication);
        //}

        // POST: RefundApplications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                RefundApplication refundApplication = db.RefundApplications.Find(id);
                if (refundApplication == null)
                    return Json(new JsonResultData("פעולה שגויה - הרשומה לא נמצאה"));
                if (refundApplication.ApplicationApprovalStatus?.Any() ?? false)
                    return Json(new JsonResultData("פעולה שגויה - ישנן רשומות המשוייכות לרשומה זו"));
                if (refundApplication.RefundApplicationFiles?.Any() ?? false)
                {
                    foreach (var item in refundApplication.RefundApplicationFiles)
                    {
                        if (!DeleteFile(item.FilePath))
                            return Json(new JsonResultData("התרחשה שגיאה במחיקת אחד מהקבצים המשוייכים"));
                    }
                    db.RefundApplicationFiles.RemoveRange(refundApplication.RefundApplicationFiles);
                }
                db.RefundApplications.Remove(refundApplication);
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
