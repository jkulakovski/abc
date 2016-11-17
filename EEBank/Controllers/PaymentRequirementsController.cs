using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EEBank.Models;

namespace EEBank.Controllers
{
    public class PaymentRequirementsController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        // GET: PaymentRequirements
        public ActionResult Index()
        {
            var paymentRequirements = db.PaymentRequirements.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.TypePaymentRequirements).Include(p => p.Users).Where(p => p.Users.Email == User.Identity.Name);
            return View(paymentRequirements.ToList());
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
            return View(paymentRequirements);
        }

        // GET: PaymentRequirements/Create
        public ActionResult Create()
        {
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress");
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PaymentRequirementsID,Date,TypeOfRequirements,DocType,СurrencyCode,SummOfremittance,UserID,AccountNumber,BankID,Benficiar,BankReceiver,PaymentPurpose,DocNumber,UserUNP,BankUNP,StatusId")] PaymentRequirements paymentRequirements)
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();

            
            
            if (ModelState.IsValid)  {
                paymentRequirements.DocType = 4;
                paymentRequirements.Date = System.DateTime.Now;
                paymentRequirements.UserID = user.UserId;
                paymentRequirements.AccountNumber = Convert.ToString(user.UserId);
                db.PaymentRequirements.Add(paymentRequirements);
                db.SaveChanges();
                return RedirectToAction("Index");

            }            
            
           
             

            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress", paymentRequirements.BankID);
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName", paymentRequirements.TypeOfRequirements);

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
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress", paymentRequirements.BankID);
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName", paymentRequirements.DocType);
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeNAme", paymentRequirements.TypeOfRequirements);
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email", paymentRequirements.UserID);
            return View(paymentRequirements);
        }

        // POST: PaymentRequirements/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PaymentRequirementsID,Date,TypeOfRequirements,DocType,СurrencyCode,SummOfremittance,UserID,AccountNumber,BankID,Benficiar,BankReceiver,PaymentPurpose,DocNumber,UserUNP,BankUNP")] PaymentRequirements paymentRequirements)
        {
            if (ModelState.IsValid)
            {
                db.Entry(paymentRequirements).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress", paymentRequirements.BankID);
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName", paymentRequirements.DocType);
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeNAme", paymentRequirements.TypeOfRequirements);
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email", paymentRequirements.UserID);
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
            return View(paymentRequirements);
        }

        // POST: PaymentRequirements/Delete/5
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
