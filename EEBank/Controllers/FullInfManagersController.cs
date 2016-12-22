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
    public class FullInfManagersController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        [Authorize]
        // GET: FullInfManagers
        public ActionResult Index()
        {
            var fullInfManagers = db.FullInfManagers.Include(f => f.Banks).Include(f => f.Roles);
            return View(fullInfManagers.ToList());
        }


        // GET: FullInfManagers/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            FullInfManagers fullInfManagers = db.FullInfManagers.Find(id);
            if (fullInfManagers == null)
            {
                return HttpNotFound();
            }
            return PartialView(fullInfManagers);
        }

        [Authorize]
        // GET: FullInfManagers/Create
        public ActionResult Create()
        {
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress");
            return PartialView();
        }

        // POST: FullInfManagers/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ManagerID,FullName,Email,ECP,OpenKey,RoleID,BankID")] FullInfManagers fullInfManagers)
        {
            var user = db.Users.ToList().LastOrDefault();
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

            var new_userinf = new FullInfManagers { FullName = fullInfManagers.FullName, Email = user.Email, ECP = sign, OpenKey = open_key, RoleID = 7, BankID = fullInfManagers.BankID};
            
           
            if (ModelState.IsValid)
            {
                db.FullInfManagers.Add(new_userinf);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress", fullInfManagers.BankID);
            return PartialView(new_userinf);
        }

        // GET: FullInfManagers/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FullInfManagers fullInfManagers = db.FullInfManagers.Find(id);
            if (fullInfManagers == null)
            {
                return HttpNotFound();
            }
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress", fullInfManagers.BankID);
            ViewBag.RoleID = new SelectList(db.Roles, "RoleId", "RoleName", fullInfManagers.RoleID);
            return PartialView(fullInfManagers);
        }

        // POST: FullInfManagers/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ManagerID,FullName,Email,ECP,OpenKey,RoleID,BankID")] FullInfManagers fullInfManagers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fullInfManagers).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BankID = new SelectList(db.Banks, "BankID", "Adress", fullInfManagers.BankID);
            ViewBag.RoleID = new SelectList(db.Roles, "RoleId", "RoleName", fullInfManagers.RoleID);
            return View(fullInfManagers);
        }

        // GET: FullInfManagers/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FullInfManagers fullInfManagers = db.FullInfManagers.Find(id);
            if (fullInfManagers == null)
            {
                return HttpNotFound();
            }
            return PartialView(fullInfManagers);
        }

        // POST: FullInfManagers/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FullInfManagers fullInfManagers = db.FullInfManagers.Find(id);
            db.FullInfManagers.Remove(fullInfManagers);
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
