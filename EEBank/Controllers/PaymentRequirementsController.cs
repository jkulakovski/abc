using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.IO;
using System.Web.Mvc;
using EEBank.Models;
using EEBank.Methods;
using System.Reflection;

namespace EEBank.Controllers

{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultiButtonAttribute : ActionNameSelectorAttribute
    {
        public string MatchFormKey { get; set; }
        public string MatchFormValue { get; set; }
        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            return controllerContext.HttpContext.Request[MatchFormKey] != null &&
                controllerContext.HttpContext.Request[MatchFormKey] == MatchFormValue;
        }

    }
    public class PaymentRequirementsController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();
        Get_ecp ecp = new Get_ecp();
        Random rn = new Random();

        // GET: PaymentRequirements
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            if (user.RoleId == 5)
            {
                var paymentRequirements = db.PaymentRequirements.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.TypePaymentRequirements).Include(p => p.Users).Where(p => p.Users.Email == User.Identity.Name).Where(p => p.StatusId != 3).OrderByDescending(p => p.DocNumber);
                return View(paymentRequirements.ToList());
            }
            if (user.RoleId == 6)
            {
                var paymentRequirements = db.PaymentRequirements.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.TypePaymentRequirements).Include(p => p.Users).Where(p => p.StatusId != 3).OrderByDescending(p => p.Date);
                return View(paymentRequirements.ToList());
            }
            else
            {
                var paymentRequirements = db.PaymentRequirements.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.TypePaymentRequirements).Include(p => p.Users).Where(p => p.FullInfManagers.Email ==user.Email).Where(p => p.StatusId == 1);
                return View(paymentRequirements.ToList());
            }
        }
              

        // GET: PaymentRequirements/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentRequirements paymentRequirements = db.PaymentRequirements.Find(id);
            if (paymentRequirements == null)
            {
                return HttpNotFound();
            }
            return PartialView(paymentRequirements);
        }



        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "отклонить")]
        public ActionResult Reject(PaymentRequirements paymentRequirements)
        {
            PaymentRequirements doc = db.PaymentRequirements.Find(paymentRequirements.PaymentRequirementsID);
            doc.StatusId = 4;
            doc.Comment = paymentRequirements.Comment;

            if (ModelState.IsValid)
            {
                db.Entry(doc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paymentRequirements);

        }

        [HttpPost, ActionName("Details")]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "принять")]
        public ActionResult Accept(int id)
        {
            PaymentRequirements paymentRequirements = db.PaymentRequirements.Find(id);
            paymentRequirements.StatusId = 3;
            
            db.Entry(paymentRequirements).State = EntityState.Modified;
            db.SaveChanges();
            ArchivePaymentRequirements achive = new ArchivePaymentRequirements();
            achive.PaymentRequirementId = id;
            db.ArchivePaymentRequirements.Add(achive);
            db.SaveChanges();
            return RedirectToAction("Index");

        }
     
        // GET: PaymentRequirements/Create
        public ActionResult Create()
        {
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress");
            ViewBag.BankReceiver = new SelectList(db.Banks, "BankID", "BanckCode");
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress");
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName");
            return PartialView();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PaymentRequirements paymentRequirements, HttpPostedFileBase upload)
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            paymentRequirements.StatusId = ecp.GET_ECP(user, upload);
            
            var managers = db.FullInfManagers.Where(p => p.BankID == paymentRequirements.BankID).ToList();
           
            int index = rn.Next(0, managers.Count);

            int docs_count = db.PaymentRequirements.Count(p => p.UserID == user.UserId);
            
            if (ModelState.IsValid)  {
                int id = Convert.ToInt32(paymentRequirements.Benficiar);
                paymentRequirements.Benficiar = db.Banks.FirstOrDefault(p => p.BankID == id).Adress;
                id = Convert.ToInt32(paymentRequirements.BankReceiver);
                paymentRequirements.BankReceiver = db.Banks.FirstOrDefault(p => p.BankID == id).BanckCode;
                paymentRequirements.DocType = 4;
                paymentRequirements.Date = System.DateTime.Now;
                paymentRequirements.DocNumber = docs_count + 1;
                paymentRequirements.UserID = user.UserId;
                paymentRequirements.AccountNumber = Convert.ToString(user.UserId);
                if (paymentRequirements.StatusId == 1)
                    paymentRequirements.ManagerId = managers.ElementAt(index).ManagerID;
                else
                    paymentRequirements.ManagerId = null;
                db.PaymentRequirements.Add(paymentRequirements);
                db.SaveChanges();
                
                return RedirectToAction("Index");

            }      
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress", paymentRequirements.BankID);
            ViewBag.BankReceiver = new SelectList(db.Banks, "BankID", "BanckCode", paymentRequirements.BankReceiver);
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress", paymentRequirements.Benficiar);
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName", paymentRequirements.TypeOfRequirements);

            return PartialView(paymentRequirements);
        }
        
        public ActionResult Create_by_manager()
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            var manager = db.FullInfManagers.FirstOrDefault(p => p.Email == user.Email);
            var bank = db.Banks.Where(t => t.BankID == manager.BankID);
            ViewBag.BankID = new SelectList(bank, "BankID", "Adress");
            ViewBag.BankReceiver = new SelectList(bank, "BankID", "BanckCode");
            ViewBag.Benficiar = new SelectList(bank, "BankID", "Adress");
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName");
            ViewBag.UserID = new SelectList(db.UserInf, "UserID", "FullName");
            ViewBag.AccountNumber = new SelectList(db.Users, "UserID", "UserID");
            return PartialView();
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_by_manager([Bind(Include = "PaymentRequirementsID,Date,TypeOfRequirements,DocType,СurrencyCode,SummOfremittance,UserID,AccountNumber,BankID,Benficiar,BankReceiver,PaymentPurpose,DocNumber,UserUNP,BankUNP,StatusId,ManagerId")] PaymentRequirements paymentRequirements)
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            var manager = db.FullInfManagers.FirstOrDefault(p => p.Email == user.Email);


            if (ModelState.IsValid)
            {
                int id = Convert.ToInt32(paymentRequirements.Benficiar);
                paymentRequirements.Benficiar = db.Banks.FirstOrDefault(p => p.BankID == id).Adress;
                id = Convert.ToInt32(paymentRequirements.BankReceiver);
                paymentRequirements.BankReceiver = db.Banks.FirstOrDefault(p => p.BankID == id).BanckCode;
                paymentRequirements.ManagerId = manager.ManagerID;
                paymentRequirements.DocType = 4;
                paymentRequirements.Date = System.DateTime.Now;
                paymentRequirements.StatusId = 3;
                db.PaymentRequirements.Add(paymentRequirements);
                db.SaveChanges();

                return RedirectToAction("Index");

            }

            var bank = db.Banks.Where(t => t.BankID == manager.BankID);
            ViewBag.BankID = new SelectList(bank, "BankID", "Adress", paymentRequirements.BankID);
            ViewBag.BankReceiver = new SelectList(bank, "BankID", "BanckCode", paymentRequirements.BankReceiver);
            ViewBag.Benficiar = new SelectList(bank, "BankID", "Adress", paymentRequirements.Benficiar);
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName",paymentRequirements.TypeOfRequirements);
            ViewBag.UserID = new SelectList(db.UserInf, "UserID", "FullName", paymentRequirements.UserID);
            ViewBag.AccountNumber = new SelectList(db.Users, "UserID", "UserID",paymentRequirements.AccountNumber);

            return View(paymentRequirements);
        }
        // GET: PaymentRequirements/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentRequirements paymentRequirements = db.PaymentRequirements.Find(id);
            if (paymentRequirements == null)
            {
                return HttpNotFound();
            }
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress");
            ViewBag.BankReceiver = new SelectList(db.Banks, "BankID", "BanckCode");
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress");
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName");
            return PartialView(paymentRequirements);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PaymentRequirements paymentRequirements, HttpPostedFileBase upload)
        {

            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);

            paymentRequirements.StatusId = ecp.GET_ECP(user, upload);

            var managers = db.FullInfManagers.Where(p => p.BankID == paymentRequirements.BankID).ToList();
           
            int index = rn.Next(0, managers.Count);

            var old_doc = db.PaymentRequirements.Find(paymentRequirements.PaymentRequirementsID);
            
            if (ModelState.IsValid)
            {
                int id = Convert.ToInt32(paymentRequirements.Benficiar);
                paymentRequirements.Benficiar = db.Banks.FirstOrDefault(p => p.BankID == id).Adress;
                id = Convert.ToInt32(paymentRequirements.BankReceiver);
                paymentRequirements.BankReceiver = db.Banks.FirstOrDefault(p => p.BankID == id).BanckCode;
                if (paymentRequirements.StatusId == 1)
                    paymentRequirements.ManagerId = managers.ElementAt(index).ManagerID;
                else
                    paymentRequirements.ManagerId = null;

                paymentRequirements.Date = old_doc.Date;
                paymentRequirements.TypeOfRequirements = old_doc.TypeOfRequirements;
                paymentRequirements.DocType = 4;
                paymentRequirements.UserID = old_doc.UserID;
                paymentRequirements.AccountNumber = old_doc.AccountNumber;
                paymentRequirements.DocNumber = old_doc.DocNumber;

                db.Entry(old_doc).CurrentValues.SetValues(paymentRequirements);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress", paymentRequirements.BankID);
            ViewBag.BankReceiver = new SelectList(db.Banks, "BankID", "BanckCode", paymentRequirements.BankReceiver);
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress", paymentRequirements.Benficiar);
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName", paymentRequirements.TypePaymentRequirements);
            return View(paymentRequirements);
        }

        // GET: PaymentRequirements/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentRequirements paymentRequirements = db.PaymentRequirements.Find(id);
            if (paymentRequirements == null)
            {
                return HttpNotFound();
            }
            return PartialView(paymentRequirements);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PaymentRequirements paymentRequirements = db.PaymentRequirements.Find(id);
            db.PaymentRequirements.Remove(paymentRequirements);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
