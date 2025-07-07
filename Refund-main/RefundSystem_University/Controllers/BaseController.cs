using RefundSystem_University.Models;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WebMarkupMin.AspNet4.Mvc;

namespace RefundSystem_University.Controllers
{
    [CompressContent]
    [MinifyHtml]
    public abstract class BaseController : Controller
    {
        protected Entities db = new Entities();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public string SaveFile(HttpPostedFileBase file, string subFolder = "General", string field = "", bool required = false)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var fullPath = Path.Combine(Server.MapPath("~/Uploads/"), subFolder);
                    if (!string.IsNullOrEmpty(subFolder))
                        if (!Directory.Exists(fullPath))
                            Directory.CreateDirectory(fullPath);

                    if (file != null && file.ContentLength > 0)
                    {
                        var timestamp = DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
                        var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{timestamp}{Path.GetExtension(file.FileName)}";
                        var path = Path.Combine(fullPath, fileName);
                        file.SaveAs(path);
                        return Path.Combine(subFolder, fileName);
                    }
                    else if (required)
                        ModelState.AddModelError(field, "שדה חובה");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(field, "שגיאה התרחשה במהלך שמירת התמונה");
                }
            }

            return string.Empty;
        }

        public string SaveSignature(string imageBase64String, string subFolder = "General", string field = "", bool required = false)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var fullPath = Path.Combine(Server.MapPath("~/Uploads/"), subFolder);
                    if (!string.IsNullOrEmpty(subFolder))
                        if (!Directory.Exists(fullPath))
                            Directory.CreateDirectory(fullPath);

                    if (!string.IsNullOrEmpty(imageBase64String))
                    {
                        var timestamp = DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
                        var fileName = $"Signature_{timestamp}.png";
                        var path = Path.Combine(fullPath, fileName);
                        byte[] imageBytes = Convert.FromBase64String(imageBase64String);
                        System.IO.File.WriteAllBytes(path, imageBytes);
                        return Path.Combine(subFolder, fileName);
                    }
                    else if (required)
                        ModelState.AddModelError(field, "שדה חובה");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(field, "שגיאה התרחשה במהלך שמירת החתימה");
                }
            }

            return string.Empty;
        }

        public bool DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return true;

            try
            {
                System.IO.File.Delete(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                return true;
            }
            catch (Exception ex)
            {
                //Logger.Error(ex, $"Failed to delete file {fileName}"); //TODO: Return error
                return false;
            }
        }
    }
}