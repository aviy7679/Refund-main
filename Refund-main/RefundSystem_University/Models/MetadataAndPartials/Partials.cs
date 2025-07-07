using RefundSystem_University.Models.MetadataAndPartials;
using System.ComponentModel.DataAnnotations;

namespace RefundSystem_University.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {

    }

    [MetadataType(typeof(EntityMetadata))]
    public partial class Entity
    {

    }

    [MetadataType(typeof(DepartmentMetadata))]
    public partial class Department
    {

    }

    [MetadataType(typeof(DepartmentManagerMetadata))]
    public partial class DepartmentManager
    {

    }

    [MetadataType(typeof(ManagerMetadata))]
    public partial class ProcessManager
    {

    }

    [MetadataType(typeof(CancellationTypeMetadata))]
    public partial class CancellationType
    {

    }

    [MetadataType(typeof(FormMetadata))]
    public partial class Form
    {

    }

    [MetadataType(typeof(AuthorizedSignatoryMetadata))]
    public partial class AuthorizedSignatory
    {
        public bool IsForCancellationTypeNotOnRegistrationDay { get; set; }
        public bool IsForCancellationTypeOnRegistrationDay { get; set; }
    }

    [MetadataType(typeof(RefundApplicationMetadata))]
    public partial class RefundApplication
    {

    }

    public partial class RefundApplicationFile
    {
        public RefundApplicationFile()
        {

        }

        public RefundApplicationFile(string filePath)
        {
            FilePath = filePath;
        }
    }

    [MetadataType(typeof(ApplicationApprovalStatusMetadata))]
    public partial class ApplicationApprovalStatu
    {
        public ApplicationApprovalStatu()
        {

        }

        public ApplicationApprovalStatu(int refundApplicationId, int authorizedSignatoryId)
        {
            RefundApplicationId = refundApplicationId;
            AuthorizedSignatoryId = authorizedSignatoryId;
        }
    }

    [MetadataType(typeof(ApprovedRefundApplicationEmailCcRecipientMetadata))]
    public partial class ApprovedRefundApplicationEmailCcRecipient
    {

    }
}