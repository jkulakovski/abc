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
    public class ArchivePaymentRequirementsController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        // GET: ArchivePaymentRequirements
        public ActionResult Index()
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            if (user.RoleId == 5)
            {
                var archivePaymentRequirements = db.ArchivePaymentRequirements.Include(a => a.PaymentRequirements).Where(p => p.PaymentRequirements.UserID == user.UserId); ;
                return View(archivePaymentRequirements.ToList());
            }
            if (user.RoleId == 6)
            {
                var archivePaymentRequirements = db.ArchivePaymentRequirements.Include(a => a.PaymentRequirements).Where(p => p.PaymentRequirementId != null);
                return View(archivePaymentRequirements.ToList());
            }
            else
            {
                var archivePaymentRequirements = db.ArchivePaymentRequirements.Include(a => a.PaymentRequirements).Where(p => p.PaymentRequirements.FullInfManagers.Email == User.Identity.Name);
                return View(archivePaymentRequirements.ToList());
            }
                
        }

        // GET: ArchivePaymentRequirements/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchivePaymentRequirements archivePaymentRequirements = db.ArchivePaymentRequirements.Find(id);
            if (archivePaymentRequirements == null)
            {
                return HttpNotFound();
            }
            return  PartialView(archivePaymentRequirements);
        }

        // GET: ArchivePaymentRequirements/Create
        public ActionResult Create()
        {
            ViewBag.PaymentRequirementId = new SelectList(db.PaymentRequirements, "PaymentRequirementsID", "СurrencyCode");
            return View();
        }

        // POST: ArchivePaymentRequirements/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ArchivePaymentRequirementsId,PaymentRequirementId")] ArchivePaymentRequirements archivePaymentRequirements)
        {
            if (ModelState.IsValid)
            {
                db.ArchivePaymentRequirements.Add(archivePaymentRequirements);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PaymentRequirementId = new SelectList(db.PaymentRequirements, "PaymentRequirementsID", "СurrencyCode", archivePaymentRequirements.PaymentRequirementId);
            return View(archivePaymentRequirements);
        }

        // GET: ArchivePaymentRequirements/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchivePaymentRequirements archivePaymentRequirements = db.ArchivePaymentRequirements.Find(id);
            if (archivePaymentRequirements == null)
            {
                return HttpNotFound();
            }
            ViewBag.PaymentRequirementId = new SelectList(db.PaymentRequirements, "PaymentRequirementsID", "СurrencyCode", archivePaymentRequirements.PaymentRequirementId);
            return View(archivePaymentRequirements);
        }

        // POST: ArchivePaymentRequirements/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ArchivePaymentRequirementsId,PaymentRequirementId")] ArchivePaymentRequirements archivePaymentRequirements)
        {
            if (ModelState.IsValid)
            {
                db.Entry(archivePaymentRequirements).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PaymentRequirementId = new SelectList(db.PaymentRequirements, "PaymentRequirementsID", "СurrencyCode", archivePaymentRequirements.PaymentRequirementId);
            return View(archivePaymentRequirements);
        }

        // GET: ArchivePaymentRequirements/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchivePaymentRequirements archivePaymentRequirements = db.ArchivePaymentRequirements.Find(id);
            if (archivePaymentRequirements == null)
            {
                return HttpNotFound();
            }
            return View(archivePaymentRequirements);
        }

        // POST: ArchivePaymentRequirements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ArchivePaymentRequirements archivePaymentRequirements = db.ArchivePaymentRequirements.Find(id);
            db.ArchivePaymentRequirements.Remove(archivePaymentRequirements);
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
