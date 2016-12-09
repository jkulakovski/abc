﻿using System;
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
    public class ArchivePaymentOrdersController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        // GET: ArchivePaymentOrders
        public ActionResult Index()
        {
            var archivePaymentOrders = db.ArchivePaymentOrders.Include(a => a.PaymentOrder);
            return View(archivePaymentOrders.ToList());
        }

        // GET: ArchivePaymentOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchivePaymentOrders archivePaymentOrders = db.ArchivePaymentOrders.Find(id);
            if (archivePaymentOrders == null)
            {
                return HttpNotFound();
            }
            return PartialView(archivePaymentOrders);
        }

        // GET: ArchivePaymentOrders/Create
        public ActionResult Create()
        {
            ViewBag.PaymentOrderID = new SelectList(db.PaymentOrder, "PaymentOrderID", "BankReceiver");
            return View();
        }

        // POST: ArchivePaymentOrders/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ArchivePaymentOrderID,PaymentOrderID")] ArchivePaymentOrders archivePaymentOrders)
        {
            if (ModelState.IsValid)
            {
                db.ArchivePaymentOrders.Add(archivePaymentOrders);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PaymentOrderID = new SelectList(db.PaymentOrder, "PaymentOrderID", "BankReceiver", archivePaymentOrders.PaymentOrderID);
            return View(archivePaymentOrders);
        }

        // GET: ArchivePaymentOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchivePaymentOrders archivePaymentOrders = db.ArchivePaymentOrders.Find(id);
            if (archivePaymentOrders == null)
            {
                return HttpNotFound();
            }
            ViewBag.PaymentOrderID = new SelectList(db.PaymentOrder, "PaymentOrderID", "BankReceiver", archivePaymentOrders.PaymentOrderID);
            return View(archivePaymentOrders);
        }

        // POST: ArchivePaymentOrders/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ArchivePaymentOrderID,PaymentOrderID")] ArchivePaymentOrders archivePaymentOrders)
        {
            if (ModelState.IsValid)
            {
                db.Entry(archivePaymentOrders).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PaymentOrderID = new SelectList(db.PaymentOrder, "PaymentOrderID", "BankReceiver", archivePaymentOrders.PaymentOrderID);
            return View(archivePaymentOrders);
        }

        // GET: ArchivePaymentOrders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ArchivePaymentOrders archivePaymentOrders = db.ArchivePaymentOrders.Find(id);
            if (archivePaymentOrders == null)
            {
                return HttpNotFound();
            }
            return View(archivePaymentOrders);
        }

        // POST: ArchivePaymentOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ArchivePaymentOrders archivePaymentOrders = db.ArchivePaymentOrders.Find(id);
            db.ArchivePaymentOrders.Remove(archivePaymentOrders);
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
