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
    public class ArchiveFreeFormatDocsController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        // GET: ArchiveFreeFormatDocs
        [Authorize]
        public ActionResult Index()
        {
            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            if (user.RoleId == 5)
            {
                var archiveFreeFormatDoc = db.ArchiveFreeFormatDoc.Include(a => a.FreeFormatDoc).Where(p => p.FreeFormatDoc.UserID == user.UserId);
                return View(archiveFreeFormatDoc.ToList());
            }
            if (user.RoleId == 6)
            {
                var archiveFreeFormatDoc = db.ArchiveFreeFormatDoc.Include(a => a.FreeFormatDoc);
                return View(archiveFreeFormatDoc.ToList());
            }
            else
            {
                var archiveFreeFormatDoc = db.ArchiveFreeFormatDoc.Include(a => a.FreeFormatDoc).Where(p => p.FreeFormatDoc.FullInfManagers.Email == User.Identity.Name);
                return View(archiveFreeFormatDoc.ToList());
            }
        }

        // GET: ArchiveFreeFormatDocs/Details/5
        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchiveFreeFormatDoc archiveFreeFormatDoc = db.ArchiveFreeFormatDoc.Find(id);
            if (archiveFreeFormatDoc == null)
            {
                return HttpNotFound();
            }
            return PartialView(archiveFreeFormatDoc);
        }

        // GET: ArchiveFreeFormatDocs/Create
        public ActionResult Create()
        {
            ViewBag.FreeFormatDocID = new SelectList(db.FreeFormatDoc, "FreeFormatDocID", "Document");
            return View();
        }

        // POST: ArchiveFreeFormatDocs/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ArchiveFreeFormatDocID,FreeFormatDocID")] ArchiveFreeFormatDoc archiveFreeFormatDoc)
        {
            if (ModelState.IsValid)
            {
                db.ArchiveFreeFormatDoc.Add(archiveFreeFormatDoc);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FreeFormatDocID = new SelectList(db.FreeFormatDoc, "FreeFormatDocID", "Document", archiveFreeFormatDoc.FreeFormatDocID);
            return View(archiveFreeFormatDoc);
        }

        // GET: ArchiveFreeFormatDocs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchiveFreeFormatDoc archiveFreeFormatDoc = db.ArchiveFreeFormatDoc.Find(id);
            if (archiveFreeFormatDoc == null)
            {
                return HttpNotFound();
            }
            ViewBag.FreeFormatDocID = new SelectList(db.FreeFormatDoc, "FreeFormatDocID", "Document", archiveFreeFormatDoc.FreeFormatDocID);
            return View(archiveFreeFormatDoc);
        }

        // POST: ArchiveFreeFormatDocs/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ArchiveFreeFormatDocID,FreeFormatDocID")] ArchiveFreeFormatDoc archiveFreeFormatDoc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(archiveFreeFormatDoc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.FreeFormatDocID = new SelectList(db.FreeFormatDoc, "FreeFormatDocID", "Document", archiveFreeFormatDoc.FreeFormatDocID);
            return View(archiveFreeFormatDoc);
        }

        // GET: ArchiveFreeFormatDocs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchiveFreeFormatDoc archiveFreeFormatDoc = db.ArchiveFreeFormatDoc.Find(id);
            if (archiveFreeFormatDoc == null)
            {
                return HttpNotFound();
            }
            return View(archiveFreeFormatDoc);
        }

        // POST: ArchiveFreeFormatDocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ArchiveFreeFormatDoc archiveFreeFormatDoc = db.ArchiveFreeFormatDoc.Find(id);
            db.ArchiveFreeFormatDoc.Remove(archiveFreeFormatDoc);
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
