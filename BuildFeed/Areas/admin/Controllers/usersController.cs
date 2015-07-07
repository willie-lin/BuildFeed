using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using RedisAuth;

namespace BuildFeed.Areas.admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class usersController : Controller
    {
        // GET: admin/users
        public ActionResult index()
        {
            return View(Membership.GetAllUsers()
                        .Cast<MembershipUser>()
                        .OrderByDescending(m => m.IsApproved)
                        .ThenBy(m => m.UserName));
        }

        public ActionResult admins()
        {
            var admins = Roles.GetUsersInRole("Administrators")
                        .Select(Membership.GetUser)
                        .ToList();

            return View(admins.OrderByDescending(m => m.UserName));
        }

        public ActionResult promote(string id)
        {
            Roles.AddUserToRole(id, "Administrators");
            return RedirectToAction("Index");
        }

        public ActionResult demote(string id)
        {
            Roles.RemoveUserFromRole(id, "Administrators");
            return RedirectToAction("Index");
        }

        public ActionResult approve(Guid id)
        {
            RedisMembershipProvider provider = (Membership.Provider as RedisMembershipProvider);
            provider?.ChangeApproval(id, true);
            return RedirectToAction("Index");
        }

        public ActionResult unapprove(Guid id)
        {
            RedisMembershipProvider provider = (Membership.Provider as RedisMembershipProvider);
            provider?.ChangeApproval(id, false);
            return RedirectToAction("Index");
        }

        public ActionResult @lock(Guid id)
        {
            RedisMembershipProvider provider = (Membership.Provider as RedisMembershipProvider);
            provider?.ChangeLockStatus(id, true);
            return RedirectToAction("Index");
        }

        public ActionResult unlock(Guid id)
        {
            RedisMembershipProvider provider = (Membership.Provider as RedisMembershipProvider);
            provider?.ChangeLockStatus(id, false);
            return RedirectToAction("Index");
        }

        public ActionResult cleanup()
        {
            MembershipUserCollection users = Membership.GetAllUsers();

            foreach (MembershipUser user in users)
            {
                if (!user.IsApproved && (user.CreationDate.AddDays(30) < DateTime.Now))
                {
                    Membership.DeleteUser(user.UserName);
                }
            }

            return RedirectToAction("index");
        }
    }
}