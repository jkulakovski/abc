using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EEBank.Models;
using System.IO;
using EEBank.Methods;

namespace EEBank.Controllers
{

    public class PaymentOrdersController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();
        Get_ecp ecp = new Get_ecp();
        Random rn = new Random();

        // GET: PaymentOrders
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            if (user.RoleId == 5)
            {
                var paymentOrder = db.PaymentOrder.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.Users).Where(p => p.Users.Email == User.Identity.Name).Where(p => p.StatusID != 3).OrderByDescending(p => p.Date);
                return View(paymentOrder.ToList());
            }
            if (user.RoleId == 6)
            {
                var paymentOrder = db.PaymentOrder.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.Users).Where(p => p.StatusID != 3).OrderByDescending(p => p.Date);
                return View(paymentOrder.ToList());
            }
            else
            {
                var paymentOrder = db.PaymentOrder.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.Users).Where(p => p.FullInfManagers.Email == user.Email).Where(p => p.StatusID == 1);
                return View(paymentOrder.ToList());
            }
        }

        // GET: PaymentOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentOrder paymentOrder = db.PaymentOrder.Find(id);
            if (paymentOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView(paymentOrder);
        }

        [HttpPost, ActionName("Details")]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "отклонить")]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(PaymentOrder paymentOrder)
        {
            PaymentOrder doc = db.PaymentOrder.Find(paymentOrder.PaymentOrderID);
            var balans = db.UserInf.Where(p =>p.UserID == doc.UserID).FirstOrDefault().Balans;
            if (paymentOrder.Summ > balans)
                doc.StatusID = db.DocStatus.FirstOrDefault(p => p.StatusName == "Откленен").StatusId;
            else
            {
                doc.StatusID = db.DocStatus.FirstOrDefault(p => p.StatusName == "Картотека №2").StatusId;
                doc.Comment = "Недостаточно средств на счету";
            }
                
            db.Entry(doc).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        [HttpPost, ActionName("Details")]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "принять")]
        public ActionResult Accept(int id)
        {
            PaymentOrder paymentOrder = db.PaymentOrder.Find(id);
            ArchivePaymentOrders archive = new ArchivePaymentOrders();
           
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name).UserInf1.FirstOrDefault(p => p.UserID == paymentOrder.UserID);
            if (user.Balans >= paymentOrder.Summ)
            {
                paymentOrder.StatusID = 3;
                user.Balans = user.Balans - paymentOrder.Summ;
                archive.PaymentOrderID = paymentOrder.PaymentOrderID;
                db.ArchivePaymentOrders.Add(archive);
                db.SaveChanges();

            }
            db.Entry(paymentOrder).State = EntityState.Modified;
            db.SaveChanges();
            ArchivePaymentRequirements achive = new ArchivePaymentRequirements();
            /*achive.PaymentRequirementId = id;
            db.ArchivePaymentRequirements.Add(achive);
            db.SaveChanges();*/
            return RedirectToAction("Index");

        }

        // GET: PaymentOrders/Create
        public ActionResult Create()
        {
            ViewBag.BankReceiver = new SelectList(db.Banks, "BankID", "Adress");
            ViewBag.BankCodeID = new SelectList(db.Banks, "BankID", "BanckCode");
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName");
            ViewBag.TypeOfPaymatOrder = new SelectList(db.TypeOfPaymentOrder, "TYpeId", "Name");
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress");
            ViewBag.PaymentTypeID = new SelectList(db.PaymentType, "PaymentTypeID", "PaymentName");
            return PartialView();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PaymentOrder paymentOrder, HttpPostedFileBase upload)
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            paymentOrder.StatusID = ecp.GET_ECP(user, upload);

            var managers = db.FullInfManagers.Where(p => p.BankID == paymentOrder.BankCodeID).ToList();
            Random rn = new Random();
            int index = rn.Next(0, managers.Count);

            if (ModelState.IsValid)
            {
                paymentOrder.DocType = 5;
                paymentOrder.Date = System.DateTime.Now;
                paymentOrder.UserID = user.UserId;
                paymentOrder.BankAccount = user.UserId;
                if (paymentOrder.StatusID == 1)
                    paymentOrder.ManagerID = managers.ElementAt(index).ManagerID;
                else
                    paymentOrder.ManagerID = null;
                db.PaymentOrder.Add(paymentOrder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BankCodeID = new SelectList(db.Banks, "BankID", "BanckCode", paymentOrder.BankCodeID);
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName",paymentOrder.DocType);
            ViewBag.TypeOfPaymatOrder = new SelectList(db.TypeOfPaymentOrder, "TYpeId", "Name", paymentOrder.TypeOfPaymatOrder);
            ViewBag.BankReceiver = new SelectList(db.Banks, "BankID", "Adress", paymentOrder.BankReceiver);
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress", paymentOrder.Benficiar);
            ViewBag.PaymentTypeID = new SelectList(db.PaymentType, "PaymentTypeID", "PaymentName", paymentOrder.PaymentOrderID);
            return PartialView(paymentOrder);
        }

        public ActionResult Create_by_manager()
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            var manager = db.FullInfManagers.Where(p => p.Email == user.Email).FirstOrDefault();
            var bank = db.Banks.Where(t => t.BankID == manager.BankID);
            ViewBag.BankReceiver = new SelectList(bank, "BankID", "Adress");
            ViewBag.BankCodeID = new SelectList(bank, "BankID", "BanckCode");
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName");
            ViewBag.TypeOfPaymatOrder = new SelectList(db.TypeOfPaymentOrder, "TYpeId", "Name");
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress");
            ViewBag.PaymentTypeID = new SelectList(db.PaymentType, "PaymentTypeID", "PaymentName");
            ViewBag.UserID = new SelectList(db.UserInf, "UserID", "FullName");
            ViewBag.BankAccount = new SelectList(db.Users, "UserID", "UserID");
            return PartialView();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_by_manager([Bind(Include = "PaymentOrderID,Date,TypeOfPaymatOrder,DocType,ExchangeRates,UserID,Summ,BankReceiver,BankCodeID,Benficiar,BankAccount,PaymentPurpose,UserUNP,ReceiverUNP,PaymentCode, PaymentTypeID, ManagerID,StatusID,Comment")] PaymentOrder paymentOrder)
        {
            var manager = db.FullInfManagers.FirstOrDefault(p => p.Email == User.Identity.Name) ;


            if (ModelState.IsValid)
            {
                paymentOrder.ManagerID = manager.ManagerID;
                paymentOrder.DocType = 5;
                paymentOrder.Date = System.DateTime.Now;
                paymentOrder.StatusID = 3;
                db.PaymentOrder.Add(paymentOrder);
                db.SaveChanges();

                return RedirectToAction("Index");

            }

            var bank = db.Banks.Where(t => t.BankID == manager.BankID);
            var account = db.Users.Where(p =>p.Email == User.Identity.Name);
            ViewBag.BankReceiver = new SelectList(bank, "BankID", "Adress", paymentOrder.BankReceiver);
            ViewBag.BankCodeID = new SelectList(bank, "BankID", "BanckCode", paymentOrder.BankCodeID);
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName",paymentOrder.DocType);
            ViewBag.TypeOfPaymatOrder = new SelectList(db.TypeOfPaymentOrder, "TYpeId", "Name", paymentOrder.TypeOfPaymatOrder);
            ViewBag.Benficiar = new SelectList(bank, "BankID", "Adress",paymentOrder.Benficiar);
            ViewBag.PaymentTypeID = new SelectList(db.PaymentType, "PaymentTypeID", "PaymentName",paymentOrder.PaymentOrderID);
            ViewBag.UserID = new SelectList(db.UserInf, "UserID", "FullName",paymentOrder.UserID);
            ViewBag.BankAccount = new SelectList( db.Users, "UserID", "UserID", paymentOrder.BankAccount);
            return View(paymentOrder);
        }

        // GET: PaymentOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentOrder paymentOrder = db.PaymentOrder.Find(id);
            if (paymentOrder == null)
            {
                return HttpNotFound();
            }
            ViewBag.BankCodeID = new SelectList(db.Banks, "BankID", "BanckCode", paymentOrder.BankCodeID);
            ViewBag.BankReceiver = new SelectList(db.Banks, "BankID", "Adress", paymentOrder.BankReceiver);
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName", paymentOrder.DocType);
            ViewBag.TypeOfPaymatOrder = new SelectList(db.TypeOfPaymentOrder, "TYpeId", "Name", paymentOrder.TypeOfPaymatOrder);
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email", paymentOrder.UserID);
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress", paymentOrder.Benficiar);
            return PartialView(paymentOrder);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PaymentOrder paymentOrder, HttpPostedFileBase upload)
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);

            paymentOrder.StatusID = ecp.GET_ECP(user, upload);

            var managers = db.FullInfManagers.Where(p => p.BankID == paymentOrder.BankCodeID).ToList();

            int index = rn.Next(0, managers.Count);

            var old_doc = db.PaymentOrder.Find(paymentOrder.PaymentOrderID);

            if (ModelState.IsValid)
            {
                int id = Convert.ToInt32(paymentOrder.Benficiar);
                paymentOrder.Benficiar = db.Banks.FirstOrDefault(p => p.BankID == id).Adress;
                id = Convert.ToInt32(paymentOrder.BankReceiver);
                paymentOrder.BankReceiver = db.Banks.FirstOrDefault(p => p.BankID == id).BanckCode;
                if (paymentOrder.StatusID == 1){
                    paymentOrder.ManagerID = managers.ElementAt(index).ManagerID;
                    paymentOrder.Comment = null;
                }
                    
                else
                    paymentOrder.ManagerID = null;

                paymentOrder.Date = old_doc.Date;
                paymentOrder.PaymentTypeID = old_doc.PaymentTypeID;
                paymentOrder.DocType = 5;
                paymentOrder.UserID = old_doc.UserID;
                paymentOrder.BankAccount = old_doc.BankAccount;

                db.Entry(old_doc).CurrentValues.SetValues(paymentOrder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BankCodeID = new SelectList(db.Banks, "BankID", "BanckCode", paymentOrder.BankCodeID);
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName", paymentOrder.DocType);
            ViewBag.TypeOfPaymatOrder = new SelectList(db.TypeOfPaymentOrder, "TYpeId", "Name", paymentOrder.TypeOfPaymatOrder);
            ViewBag.BankReceiver = new SelectList(db.Banks, "BankID", "Adress", paymentOrder.BankReceiver);
            ViewBag.Benficiar = new SelectList(db.Banks, "BankID", "Adress", paymentOrder.Benficiar);
            ViewBag.PaymentTypeID = new SelectList(db.PaymentType, "PaymentTypeID", "PaymentName", paymentOrder.PaymentOrderID);
           
            return View(paymentOrder);
        }

        // GET: PaymentOrders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PaymentOrder paymentOrder = db.PaymentOrder.Find(id);
            if (paymentOrder == null)
            {
                return HttpNotFound();
            }
            return PartialView(paymentOrder);
        }

        // POST: PaymentOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PaymentOrder paymentOrder = db.PaymentOrder.Find(id);
            db.PaymentOrder.Remove(paymentOrder);
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
