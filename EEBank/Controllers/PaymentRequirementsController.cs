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

        // GET: PaymentRequirements
        public ActionResult Index()
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            if (user.RoleId == 5)
            {
                var paymentRequirements = db.PaymentRequirements.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.TypePaymentRequirements).Include(p => p.Users).Where(p => p.Users.Email == User.Identity.Name).Where(p => p.StatusId != 3);
                return View(paymentRequirements.ToList());
            }
            if (user.RoleId == 6)
            {
                var paymentRequirements = db.PaymentRequirements.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.TypePaymentRequirements).Include(p => p.Users);
                return View(paymentRequirements.ToList());
            }
            else
            {
                var paymentRequirements = db.PaymentRequirements.Include(p => p.Banks).Include(p => p.DocType1).Include(p => p.TypePaymentRequirements).Include(p => p.Users).Where(p => p.FullInfManagers.Email ==user.Email).Where(p => p.StatusId == 1);
                return View(paymentRequirements.ToList());
            }
        }
              

        // GET: PaymentRequirements/Details/5
        public ActionResult Details(int? id, string action)
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



        [HttpPost, ActionName("Details")]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "Reject")]
        public ActionResult Reject([Bind(Include = "Comment")] PaymentRequirements paymentRequirements)
        {
            
            var balans = db.UserInf.Where(p => p.UserID == paymentRequirements.UserID).FirstOrDefault().Balans;
            if (paymentRequirements.SummOfremittance > balans)
                paymentRequirements.StatusId = db.DocStatus.Where(p => p.StatusName == "Откленен").FirstOrDefault().StatusId;
            else
                paymentRequirements.StatusId = db.DocStatus.Where(p => p.StatusName == "Картотека №2").FirstOrDefault().StatusId;
            db.Entry(paymentRequirements).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");

        }

        [HttpPost, ActionName("Details")]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "Accept")]
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
            return View();
        }

        


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PaymentRequirementsID,Date,TypeOfRequirements,DocType,СurrencyCode,SummOfremittance,UserID,AccountNumber,BankID,Benficiar,BankReceiver,PaymentPurpose,DocNumber,UserUNP,BankUNP,StatusId,ManagerId")] PaymentRequirements paymentRequirements, HttpPostedFileBase upload)
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

            int [] key = new int[2];
            
            int iter = 0;
            for(int i = 0; i < 2; i++){
                string num = "";
                for (int j = 0 + iter; j < keys.Length; j++){
                    if ( !keys[j].Equals(' '))
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
            res =  ecp.Deshifrov_ecp(s, key[0], key[1]);
            int it = 0;
            
            if (hash.Length == res.Length)
            {
                for (int i = 0; i < hash.Length; i++)
                {
                    if (hash[i] == res[i])
                        it++;
                }
            }

            var managers = db.FullInfManagers.Where(p => p.BankID == paymentRequirements.BankID).ToList();
            Random rn = new Random();
            int index = rn.Next(0, managers.Count);
            
            
            if (ModelState.IsValid)  {
                paymentRequirements.DocType = 4;
                paymentRequirements.Date = System.DateTime.Now;
                paymentRequirements.UserID = user.UserId;
                paymentRequirements.AccountNumber = Convert.ToString(user.UserId);
                if (it == res.Length)
                    paymentRequirements.StatusId = 1;
                if (it != res.Length || it == 0)
                    paymentRequirements.StatusId = 2;
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

            return View(paymentRequirements);
        }

        public ActionResult Create_by_manager()
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            var manager = db.FullInfManagers.Where(p => p.Email == user.Email).FirstOrDefault();
            var bank = db.Banks.Where(t => t.BankID == manager.BankID);
            ViewBag.BankID = new SelectList(bank, "BankID", "Adress");
            ViewBag.BankReceiver = new SelectList(bank, "BankID", "BanckCode");
            ViewBag.Benficiar = new SelectList(bank, "BankID", "Adress");
            ViewBag.TypeOfRequirements = new SelectList(db.TypePaymentRequirements, "TypeId", "TypeName");
            ViewBag.UserID = new SelectList(db.UserInf, "UserID", "FullName");
            ViewBag.AccountNumber = new SelectList(db.Users, "UserID", "UserID");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_by_manager([Bind(Include = "PaymentRequirementsID,Date,TypeOfRequirements,DocType,СurrencyCode,SummOfremittance,UserID,AccountNumber,BankID,Benficiar,BankReceiver,PaymentPurpose,DocNumber,UserUNP,BankUNP,StatusId,ManagerId")] PaymentRequirements paymentRequirements)
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            var manager = db.FullInfManagers.Where(p => p.Email == user.Email).FirstOrDefault();


            if (ModelState.IsValid)
            {
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
            return View(paymentRequirements);
        }

        // POST: PaymentRequirements/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PaymentRequirementsID,Date,TypeOfRequirements,DocType,СurrencyCode,SummOfremittance,UserID,AccountNumber,BankID,Benficiar,BankReceiver,PaymentPurpose,DocNumber,UserUNP,BankUNP, ManagerId")] PaymentRequirements paymentRequirements, HttpPostedFileBase upload)
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

            var managers = db.FullInfManagers.Where(p => p.BankID == paymentRequirements.BankID).ToList();
            Random rn = new Random();
            int index = rn.Next(0, managers.Count);

            if (ModelState.IsValid)
            {
                if (it == res.Length)
                    paymentRequirements.StatusId = 1;
                if (it != res.Length || it == 0)
                    paymentRequirements.StatusId = 2;
                if (paymentRequirements.StatusId == 1)
                    paymentRequirements.ManagerId = managers.ElementAt(index).ManagerID;
                else
                    paymentRequirements.ManagerId = null;
                db.Entry(paymentRequirements).State = EntityState.Modified;
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
