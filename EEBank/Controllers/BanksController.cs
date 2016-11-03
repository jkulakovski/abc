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
    public class BanksController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        // GET: Banks
        public ActionResult Index()
        {
            return View(db.Banks.ToList());
        }

        // GET: Banks/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Banks banks = db.Banks.Find(id);
            if (banks == null)
            {
                return HttpNotFound();
            }
            return View(banks);
        }

        // GET: Banks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Banks/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BankID,Adress,BanckCode")] Banks banks)
        {
            if (ModelState.IsValid)
            {
                db.Banks.Add(banks);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(banks);
        }

        // GET: Banks/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Banks banks = db.Banks.Find(id);
            if (banks == null)
            {
                return HttpNotFound();
            }
            return View(banks);
        }

        // POST: Banks/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BankID,Adress,BanckCode")] Banks banks)
        {
            if (ModelState.IsValid)
            {
                db.Entry(banks).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(banks);
        }

        // GET: Banks/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Banks banks = db.Banks.Find(id);
            if (banks == null)
            {
                return HttpNotFound();
            }
            return View(banks);
        }

        // POST: Banks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Banks banks = db.Banks.Find(id);
            db.Banks.Remove(banks);
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
