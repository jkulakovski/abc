using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EEBank.Models;
using EEBank.Methods;
using System.IO;

namespace EEBank.Controllers
{
    public class UserInfsController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        // GET: UserInfs
        [Authorize]
        public ActionResult Index()
        {
                var userInf = db.UserInf.Include(u => u.Users1);
                return View(userInf.ToList());
            
        }


        [Authorize]
        public ActionResult Account_Index()
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            if (user.RoleId == 5)
            {
                var userInf = db.UserInf.Include(u => u.Users1).Where(p => p.Email == User.Identity.Name);
                return View(userInf.ToList());
            }
            else
            {
                var userInf = db.UserInf.Include(u => u.Users1);
                return View(userInf.ToList());
            }

        }

        // GET: UserInfs/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            
            UserInf user = db.UserInf.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return PartialView(user);
        }

        // GET: UserInfs/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email");
            return View();
        }

        [Authorize]
        public ActionResult ECP()
        {
            return View();
        }

        // POST: UserInfs/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserInfID,FullName,Email,Password,Phone,AccountNumber,ECP,UserID,Adress,Balans")] UserInf userInf)
        {

            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            ECP ecp = new ECP();
            int[] keys = ecp.Key();
            string login = "";
            for (int i = 0; i < user.Email.Length; i++)
            {
                    if (Char.IsLetter(user.Email[i]))
                        login += user.Email[i];
                    if (Char.IsDigit(user.Email[i]))
                        login += user.Email[i];
                    if (user.Email[i] == ('@'))
                        break;
                

            }


            string sign = ecp.Create_ECP(login, keys);


            string open_key = "";
            open_key += keys[0].ToString();
            open_key += ' ';
            open_key += keys[1].ToString();

            string path = String.Format("D:\\ecp{0}.txt",login);
            StreamWriter file = new StreamWriter(path);

            file.Write(sign);
            file.Close();
            

            

            var new_userinf = new UserInf { FullName = userInf.FullName, Email = user.Email, Password = user.Password, Phone = userInf.Phone, AccountNumber = user.UserId.ToString(), ECP = sign, OpenKey = open_key, UserID = user.UserId, Adress = userInf.Adress, CurrencyCodeId = 4 };
            new_userinf.Balans = 0;
            db.SaveChanges();
            if (ModelState.IsValid)
            {               
              
                db.UserInf.Add(new_userinf);
                db.SaveChanges();
                
                return RedirectToAction("ECP");
                
            }

            return View(userInf);
        }

        [Authorize]
        public ActionResult Add_new_account()
        {
            ViewBag.CurrencyCodeId = new SelectList(db.Currency, "CurrencyId", "CodeISO");
            return PartialView();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Add_new_account(UserInf userInf)
        {

            var user = db.UserInf.Where(p => p.Email == User.Identity.Name).FirstOrDefault(p => p.CurrencyCodeId == 4);
            userInf.UserID = user.UserID;
            userInf.FullName = user.FullName;
            userInf.Password = user.Password;
            userInf.Phone = user.Phone;
            userInf.AccountNumber = ( db.UserInf.ToList().LastOrDefault().UserInfID + 1).ToString();
            userInf.ECP = user.ECP;
            userInf.Adress = user.Adress;
            userInf.OpenKey = user.OpenKey;
            userInf.Email = user.Email;
            userInf.Balans = 0;
            var exist_acc = db.UserInf.Where(p => p.Email == User.Identity.Name).FirstOrDefault(p => p.CurrencyCodeId == userInf.CurrencyCodeId);
            if (exist_acc == null)
            {
                if (ModelState.IsValid)
                {

                    db.UserInf.Add(userInf);
                    db.SaveChanges();
                    return RedirectToAction("Account_Index");
                }
            }
            else
            {
                return View("Null");
            }

            ViewBag.CurrencyCodeId = new SelectList(db.Currency, "CurrencyId", "CodeISO", userInf.CurrencyCodeId);
            return PartialView(userInf);
        }

        

        // POST: UserInfs/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserInfID,FullName,Email,Password,Phone,AccountNumber,ECP,UserID")] UserInf userInf)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userInf).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email", userInf.UserID);
            return View(userInf);
        }

        // GET: UserInfs/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserInf userInf = db.UserInf.Find(id);
            if (userInf == null)
            {
                return HttpNotFound();
            }
            return View(userInf);
        }

        // POST: UserInfs/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserInf userInf = db.UserInf.Find(id);
            db.UserInf.Remove(userInf);
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
