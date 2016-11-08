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

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserInfID,FullName,Email,Password,Phone,AccountNumber,ECP,UserID")] UserInf userInf)
        {
            var usr = db.Users.Where(p => p.Email == User.Identity.Name);
            if (ModelState.IsValid)
            {
                userInf.Email = usr.FirstOrDefault().Email;
                userInf.Password = usr.FirstOrDefault().Password;
                userInf.AccountNumber = userInf.UserInfID.ToString();
                db.UserInf.Add(userInf);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email", userInf.UserID);
            return View(userInf);
        }

        // GET: UserInfs/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.UserID = new SelectList(db.Users, "UserId", "Email", userInf.UserID);
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
