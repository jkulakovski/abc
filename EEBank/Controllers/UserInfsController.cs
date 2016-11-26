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

namespace EEBank.Controllers
{
    public class UserInfsController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        // GET: UserInfs
        public ActionResult Index()
        {
            var userInf = db.UserInf.Include(u => u.Users1);
            return View(userInf.ToList());
        }

        // GET: UserInfs/Details/5
        public ActionResult Details(int? id)
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

        // GET: UserInfs/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email");
            return View();
        }

        // POST: UserInfs/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
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
                    if (user.Email[i] == ('@'))
                        break;
                

            }


            string sign = ecp.Create_ECP(login, keys);

            string open_key = "";
            open_key += keys[0].ToString();
            open_key += ' ';
            open_key += keys[1].ToString();

            

            var new_userinf = new UserInf { FullName = userInf.FullName, Email = user.Email, Password = user.Password, Phone = userInf.Phone, AccountNumber = user.UserId.ToString(), ECP = sign, OpenKey = open_key, UserID = user.UserId, Adress = userInf.Adress };
            new_userinf.Balans = 0;
            db.SaveChanges();
            if (ModelState.IsValid)
            {               
              
                db.UserInf.Add(new_userinf);
                db.SaveChanges();
                return RedirectToAction("Index","Home");
            }

            return View(userInf);
        }


        

        // POST: UserInfs/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
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
