using RefundSystem_University.Attributes;
using RefundSystem_University.Models;

namespace RefundSystem_University.Controllers
{
    [CustomAuthorize(Roles = "Admin")]
    //TODO: Add admin permission where needed
    public abstract class AuthenticatedBaseController : BaseController
    {
        protected virtual new User User => Session["User"] as User;
    }
}