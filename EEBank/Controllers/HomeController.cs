using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Calabonga.Xml.Exports;
using EEBank.Methods;
using EEBank.Models;

namespace EEBank.Controllers
{
    

    public class HomeController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();
        public ActionResult Index()
        {
            return View();
        }
        [Authorize]
        public ActionResult Archive_Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Requaest_Acoount()
        {
            var role = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name).RoleId;
            if (role == 5)
            {
                var accounts = db.UserInf.Where(p => p.Email == User.Identity.Name);
                return PartialView(accounts.ToList());
            }
            
            return View();
        }

        public ActionResult Requaest_Currency()
        {
            var role = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name).RoleId;
            
             var currency = db.Currency;
             return PartialView(currency.ToList());
        }

        public ActionResult Requaest_DocStatus_PaymentReq()
        {
            var role = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name).RoleId;
            if (role == 5)
            {
                var doc = db.PaymentRequirements.Where(p => p.Users.Email == User.Identity.Name);
                return PartialView(doc.ToList());
            }

            return View();
        }
        public ActionResult Requaest_DocStatus_PaymentOrder()
        {
            var role = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name).RoleId;
            if (role == 5)
            {
                var doc = db.PaymentOrder.Where(p => p.Users.Email == User.Identity.Name);
                return PartialView(doc.ToList());
            }

            return View();
        }
        public ActionResult Requaest_DocStatus_FreeFormat()
        {
            var role = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name).RoleId;
            if (role == 5)
            {
                var doc = db.FreeFormatDoc.Where(p => p.Users.Email == User.Identity.Name);
                return PartialView(doc.ToList());
            }

            return View();
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}