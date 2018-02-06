using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using BuildFeed.Controllers;
using MongoAuth;

namespace BuildFeed.Admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    [RouteArea("admin")]
    [RoutePrefix("users")]
    public class UsersController : BaseController
    {
        [Route]
        public ActionResult Index() => View(Membership.GetAllUsers().Cast<MembershipUser>());

        [Route("admins")]
        public ActionResult Admins()
        {
            var admins = Roles.GetUsersInRole("Administrators").Select(Membership.GetUser).ToList();

            return View(admins.OrderByDescending(m => m.UserName));
        }

        [Route("approve/{id:guid}")]
        public ActionResult Approve(Guid id)
        {
            var provider = Membership.Provider as MongoMembershipProvider;
            provider?.ChangeApproval(id, true);
            return RedirectToAction(nameof(Index));
        }

        [Route("unapprove/{id:guid}")]
        public ActionResult Unapprove(Guid id)
        {
            var provider = Membership.Provider as MongoMembershipProvider;
            provider?.ChangeApproval(id, false);
            return RedirectToAction(nameof(Index));
        }

        [Route("lock/{id:guid}")]
        public ActionResult Lock(Guid id)
        {
            var provider = Membership.Provider as MongoMembershipProvider;
            provider?.ChangeLockStatus(id, true);
            return RedirectToAction(nameof(Index));
        }

        [Route("unlock/{id:guid}")]
        public ActionResult Unlock(Guid id)
        {
            var provider = Membership.Provider as MongoMembershipProvider;
            provider?.ChangeLockStatus(id, false);
            return RedirectToAction(nameof(Index));
        }

        [Route("delete/{id:guid}")]
        public ActionResult Delete(Guid id)
        {
            var provider = Membership.Provider as MongoMembershipProvider;
            provider?.DeleteUser(id);
            return RedirectToAction(nameof(Index));
        }

        [Route("cleanup")]
        public ActionResult Cleanup()
        {
            MembershipUserCollection users = Membership.GetAllUsers();

            foreach (MembershipUser user in users)
            {
                if (!user.IsApproved
                    && user.CreationDate.AddDays(30) < DateTime.Now)
                {
                    Membership.DeleteUser(user.UserName);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}