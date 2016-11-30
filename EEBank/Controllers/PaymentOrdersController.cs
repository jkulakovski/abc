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

        // GET: PaymentOrders
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            if (user.RoleId == 5)
            {
                var paymentOrder = db.PaymentOrder.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.Users).Where(p => p.Users.Email == User.Identity.Name).Where(p => p.StatusID != 3);
                return View(paymentOrder.ToList());
            }
            if (user.RoleId == 6)
            {
                var paymentOrder = db.PaymentOrder.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.Users);
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
            return View(paymentOrder);
        }

        [HttpPost, ActionName("Details")]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "отклонить")]
        public ActionResult Reject([Bind(Include = "Comment")] PaymentOrder paymentOrder)
        {

            var balans = db.UserInf.Where(p => p.UserID == paymentOrder.UserID).FirstOrDefault().Balans;
            if (paymentOrder.Summ > balans)
                paymentOrder.StatusID = db.DocStatus.Where(p => p.StatusName == "Откленен").FirstOrDefault().StatusId;
            else
            {
                paymentOrder.StatusID = db.DocStatus.Where(p => p.StatusName == "Картотека №2").FirstOrDefault().StatusId;
                paymentOrder.Comment = "Недостаточно средств на счету";
            }
                
            db.Entry(paymentOrder).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        [HttpPost, ActionName("Details")]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "принять")]
        public ActionResult Accept(int id)
        {
            PaymentOrder paymentOrder = db.PaymentOrder.Find(id);
            paymentOrder.StatusID = 3;
            db.Entry(paymentOrder).State = EntityState.Modified;
            db.SaveChanges();
            ArchivePaymentRequirements achive = new ArchivePaymentRequirements();
            achive.PaymentRequirementId = id;
            db.ArchivePaymentRequirements.Add(achive);
            db.SaveChanges();
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
            return View();
        }

        // POST: PaymentOrders/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PaymentOrderID,Date,TypeOfPaymatOrder,DocType,ExchangeRates,UserID,Summ,BankReceiver,BankCodeID,Benficiar,BankAccount,PaymentPurpose,UserUNP,ReceiverUNP,PaymentCode, PaymentTypeID, ManagerID,StatusID,Comment")] PaymentOrder paymentOrder, HttpPostedFileBase upload)
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            string s = "";

            if (upload != null)
            {
                // получаем имя файла
                string fileName = System.IO.Path.GetFileName(upload.FileName);
                // сохраняем файл в папку Files в проекте
                upload.SaveAs(Server.MapPath("~/Files/" + fileName));

                StreamReader ReadFile = System.IO.File.OpenText(@"C:/Users/Elizaveta/Documents/visual studio 2013/Projects/EEBank/EEBank/Files/" + fileName);
                string Input = null;
                while ((Input = ReadFile.ReadLine()) != null)
                {
                    s += Input;
                }
            }

            string login = "";
            for (int i = 0; i < user.Email.Length; i++)
            {
                if (Char.IsLetter(user.Email[i]))
                    login += user.Email[i];
                if (user.Email[i] == ('@'))
                    break;

            }

            string keys = user.UserInf1.Where(m => m.Email == User.Identity.Name).FirstOrDefault().OpenKey;

            int[] key = new int[2];

            int iter = 0;
            for (int i = 0; i < 2; i++)
            {
                string num = "";
                for (int j = 0 + iter; j < keys.Length; j++)
                {
                    if (!keys[j].Equals(' '))
                    {
                        num += keys[j];

                    }
                    else
                    {
                        iter = j + 1;
                        break;
                    }
                }
                key[i] = Convert.ToInt32(num);
            }
            double[] res;
            ECP ecp = new ECP();
            int[] hash = ECP.Hash(login);
            res = ecp.Deshifrov_ecp(s, key[0], key[1]);
            int it = 0;

            if (hash.Length == res.Length)
            {
                for (int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] == res[i])
                        it++;
                }
            }

            var managers = db.FullInfManagers.Where(p => p.BankID == paymentOrder.BankCodeID).ToList();
            Random rn = new Random();
            int index = rn.Next(0, managers.Count);

            if (ModelState.IsValid)
            {
                paymentOrder.DocType = 5;
                paymentOrder.Date = System.DateTime.Now;
                paymentOrder.UserID = user.UserId;
                paymentOrder.BankAccount = user.UserId;
                if (it == res.Length)
                    paymentOrder.StatusID = 1;
                if (it != res.Length || it == 0)
                    paymentOrder.StatusID = 2;
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
            ViewBag.BankCodeID = new SelectList(db.Banks, "BankID", "Adress", paymentOrder.BankCodeID);
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName", paymentOrder.DocType);
            ViewBag.TypeOfPaymatOrder = new SelectList(db.TypeOfPaymentOrder, "TYpeId", "Name", paymentOrder.TypeOfPaymatOrder);
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email", paymentOrder.UserID);
            return View(paymentOrder);
        }

        // POST: PaymentOrders/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PaymentOrderID,Date,TypeOfPaymatOrder,DocType,ExchangeRates,UserID,UserCountry,BankReceiver,BankCodeID,Benficiar,BankAccount,PaymentPurpose,UserUNP,ReceiverUNP,PaymentCode")] PaymentOrder paymentOrder)
        {
            if (ModelState.IsValid)
            {
                db.Entry(paymentOrder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BankCodeID = new SelectList(db.Banks, "BankID", "Adress", paymentOrder.BankCodeID);
            ViewBag.DocType = new SelectList(db.DocType, "DocTypeId", "DocName", paymentOrder.DocType);
            ViewBag.TypeOfPaymatOrder = new SelectList(db.TypeOfPaymentOrder, "TYpeId", "Name", paymentOrder.TypeOfPaymatOrder);
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email", paymentOrder.UserID);
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
            return View(paymentOrder);
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
