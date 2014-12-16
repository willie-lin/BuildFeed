using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BuildFeed.Auth;

namespace BuildFeed.Areas.admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class usersController : Controller
    {
        // GET: admin/users
        public ActionResult index()
        {
            return View(Membership.GetAllUsers().Cast<MembershipUser>().OrderByDescending(m => m.IsApproved).ThenBy(m => m.UserName));
        }

        public ActionResult admins()
        {
            List<MembershipUser> admins = new List<MembershipUser>();
            foreach(var m in Roles.GetUsersInRole("Administrators"))
            {
                admins.Add(Membership.GetUser(m));
            }

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
            var provider = (Membership.Provider as RedisMembershipProvider);
            provider.ChangeApproval(id, true);
            return RedirectToAction("Index");
        }

        public ActionResult unapprove(Guid id)
        {
            var provider = (Membership.Provider as RedisMembershipProvider);
            provider.ChangeApproval(id, false);
            return RedirectToAction("Index");
        }

        public ActionResult @lock(Guid id)
        {
            var provider = (Membership.Provider as RedisMembershipProvider);
            provider.ChangeLockStatus(id, true);
            return RedirectToAction("Index");
        }

        public ActionResult unlock(Guid id)
        {
            var provider = (Membership.Provider as RedisMembershipProvider);
            provider.ChangeLockStatus(id, false);
            return RedirectToAction("Index");
        }
    }
}