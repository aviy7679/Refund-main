using RefundSystem_University.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace RefundSystem_University.Models.MetadataAndPartials
{
    public class UserMetadata
    {
        [Display(Name = "שם משתמש"), Required(ErrorMessage = "{0} הוא שדה חובה"), StringLength(35, MinimumLength = 4)]
        public string UserName { get; set; }
        [Display(Name = "סיסמה"), Required(ErrorMessage = "{0} היא שדה חובה"), StringLength(35, MinimumLength = 4), PasswordPropertyText]
        public string Password { get; set; }
        [Display(Name = "מנהל")]
        public bool IsAdmin { get; set; }
        [Display(Name = "דוא\"ל"), Required(ErrorMessage = "{0} הוא שדה חובה"), EmailAddress, MaxLength(50)]
        public string Email { get; set; }
    }

    public class EntityMetadata
    {
        [Display(Name = "שם"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(60)]
        public string Name { get; set; }
        [Display(Name = "לוגו"), AdditionalMetadata("Type", FileType.Image), UIHint("File")]
        public string LogoPath { get; set; }
    }

    public class DepartmentMetadata
    {
        [Display(Name = "שם"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(35)]
        public string Name { get; set; }
        [Display(Name = "ישות"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int EntityId { get; set; }
    }

    public class ManagerMetadata
    {
        [Display(Name = "מחלקה"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int DepartmentId { get; set; }
        [Display(Name = "משתמש"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int UserId { get; set; }
    }

    public class DepartmentManagerMetadata : ManagerMetadata
    {
        [Display(Name = "מורשה חתימה"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int? AuthorizedSignatoryId { get; set; }
    }

    public class CancellationTypeMetadata
    {
        [Display(Name = "שם"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(35)]
        public string Name { get; set; }
        [Display(Name = "ביטול ביום ההרשמה")]
        public bool OnRegistrationDay { get; set; }
    }

    public class FormMetadata
    {
        [Display(Name = "שם"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(35)]
        public string Name { get; set; }
        [Display(Name = "סוג ביטול"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int CancellationTypeId { get; set; }      
        [Display(Name = "ישות"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int EntityId { get; set; }
        [Display(Name = "אמצעי תשלום"), AdditionalMetadata("Type", typeof(PaymentMethod)), Required(ErrorMessage = "{0} הוא שדה חובה"), EnumDataType(typeof(PaymentMethod))]
        public byte PaymentMethod { get; set; }
    }

    public class AuthorizedSignatoryMetadata
    {
        [Display(Name = "שם"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(35)]
        public string Name { get; set; }
        [Display(Name = "הגדרת תפקיד"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(35)]
        public string JobType { get; set; }
        [Display(Name = "משתמש"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int UserId { get; set; }
        [Display(Name = "ישויות"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public ICollection<Entity> Entities { get; set; }
        [Display(Name = "נייד"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(10), RegularExpression("^[0-9]*$", ErrorMessage = "{0} לא תקין")]
        public string CellPhone { get; set; }
        [Display(Name = "האם מורשה חתימה עבור סבב ביטול לא ביום העסקה?")]
        public bool IsForCancellationTypeNotOnRegistrationDay { get; set; }
        [Display(Name = "סדר סבב ביטול לא ביום העסקה")]
        public Nullable<byte> OrderForCancellationTypeNotOnRegistrationDay { get; set; }
        [Display(Name = "האם מורשה חתימה עבור סבב ביטול ביום העסקה?")]
        public bool IsForCancellationTypeOnRegistrationDay { get; set; }
        [Display(Name = "סדר סבב ביטול ביום העסקה")]
        public Nullable<byte> OrderForCancellationTypeOnRegistrationDay { get; set; }
        [Display(Name = "חתימה"), UIHint("Signature"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public string SignaturePath { get; set; }
    }

    public class RefundApplicationMetadata
    {
        [Display(Name = "ישות"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int EntityId { get; set; }
        [Display(Name = "טופס"), Required(ErrorMessage = "{0} הוא שדה חובה"), UIHint("FilterableSelect")]
        public int FormId { get; set; }
        [Display(Name = "מגיש הבקשה"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public int UserId { get; set; }
        [Display(Name = "מחלקה"), Required(ErrorMessage = "{0} הוא שדה חובה"), UIHint("FilterableSelect")]
        public int DepartmentId { get; set; }
        [Display(Name = "יוזם הזיכוי"), Required(ErrorMessage = "{0} הוא שדה חובה"), UIHint("FilterableSelect")]
        public int ProcessManagerId { get; set; }
        [Display(Name = "שם מלא של הלקוח"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(60)]
        public string CustomerName { get; set; }
        [Display(Name = "ת.ז הלקוח"), MaxLength(9)]
        public string CustomerIdNumber { get; set; }

        [Display(Name = "4 ספרות אחרונות של כרטיס אשראי"), MaxLength(4)]
        public string CreditLastDigits { get; set; }

        [Display(Name = "אופן החזר"), AdditionalMetadata("Type", typeof(RefundMethod)),/* Required(ErrorMessage = "{0} הוא שדה חובה"),*/ EnumDataType(typeof(RefundMethod))]
        public Nullable<byte> RefundMethod { get; set; }
        [Display(Name = "שם הלקוח"), MaxLength(60)]
        public string AccountOwnerName { get; set; }
        [Display(Name = "מספר בנק"), MaxLength(35)]
        public string BankNumber { get; set; }
        [Display(Name = "מספר סניף"), MaxLength(30)]
        public string BranchNumber { get; set; }
        [Display(Name = "מספר חשבון"), MaxLength(35)]
        public string AccountNumber { get; set; }

        [Display(Name = "תאריך ביצוע העסקה"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public System.DateTime TransactionDate { get; set; }
        [Display(Name = "סכום העסקה המלא"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public decimal TransactionAmount { get; set; }
        [Display(Name = "סיבת הביטול"), Required(ErrorMessage = "{0} הוא שדה חובה"), MaxLength(500)]
        public string CancellationReason { get; set; }

        [Display(Name = "סוג העסקה"), UIHint("BooleanRadioButtons"), AdditionalMetadata("FalseLabel", "ביטול חלקי"), AdditionalMetadata("TrueLabel", "ביטול מלא")]
        public bool FullCancellation { get; set; }
        [Display(Name = "סכום ההחזר")]
        public decimal RefundAmount { get; set; }
        [Display(Name = "האם קיים זיכוי נוסף"), UIHint("BooleanRadioButtons")]
        public bool AdditionalCredit { get; set; }

        [Display(Name = "פירוט"), MaxLength(500)]
        public string Details { get; set; }
        [Display(Name = "הערות"), MaxLength(500)]
        public string Remarks { get; set; }
        [Display(Name = "תאריך"), Required(ErrorMessage = "{0} הוא שדה חובה")]
        public System.DateTime Date { get; set; }
        [Display(Name = "העלאת קבצים"), UIHint("File"), Required(ErrorMessage = "{0} הוא שדה חובה"), AdditionalMetadata("Type", FileType.File), AdditionalMetadata("Multiple", true), AdditionalMetadata("Required", true)]
        public ICollection<RefundApplicationFile> RefundApplicationFiles { get; set; }
    }

    public class ApplicationApprovalStatusMetadata
    {
        [Display(Name = "סטטוס"), UIHint("BooleanRadioButtons"), AdditionalMetadata("FalseLabel", "דחיית הבקשה"), AdditionalMetadata("TrueLabel", "אישור הבקשה")]
        public bool Approved { get; set; }
        [Display(Name = "סיבת דחיית הבקשה"), MaxLength(500)]
        public string NonApprovalReason { get; set; }
    }

    public class ApprovedRefundApplicationEmailCcRecipientMetadata
    {
        [Display(Name = "דוא\"ל"), Required(ErrorMessage = "{0} הוא שדה חובה"), EmailAddress, MaxLength(50)]
        public string Email { get; set; }
    }
}