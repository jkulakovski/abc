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
using Exel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace EEBank.Controllers
{
    public class FreeFormatDocsController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();
        Get_ecp ecp = new Get_ecp();
        Random rn = new Random();

        // GET: FreeFormatDocs
        public ActionResult Index()
        {

            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            if (user.RoleId == 5)
            {
                var freeFormatDoc = db.FreeFormatDoc.Where(p => p.Users.Email == User.Identity.Name).Where(p => p.StatusID != 3).OrderByDescending(p => p.Date);
                return View(freeFormatDoc.ToList());
            }
            if (user.RoleId == 6)
            {
                var freeFormatDoc = db.FreeFormatDoc.Include(p => p.Users).Where(p => p.FreeFormatDocID != null).Where(p => p.Date != null).OrderByDescending(p => p.Date);
                return View(freeFormatDoc.ToList());
            }
            else
            {
                var freeFormatDoc = db.FreeFormatDoc.Include(p => p.DocType).Where(p => p.FullInfManagers.Email == user.Email).Where(p => p.StatusID == 1);
                return View(freeFormatDoc.ToList());
            }
        }


        public ActionResult Export()
        {
                Exel.Application application = new Exel.Application();
                Exel.Workbook workbook = application.Workbooks.Add(System.Reflection.Missing.Value);
                Exel.Worksheet worksheet = workbook.ActiveSheet;
                var docs = db.FreeFormatDoc.Where(p => p.Users.Email == User.Identity.Name).Where(p =>p.StatusID == 3).Where(p => p.Date.Month >= p.Date.Month - 1 ).ToList();
                worksheet.Cells[1, 1] = "Дата создания";
                worksheet.Cells[1, 2] = "Документ";
                worksheet.Cells[1, 3] = "Менеджер";
                int row = 2;
                foreach (FreeFormatDoc doc in docs)
                {
                    worksheet.Cells[row, 1] = doc.Date.ToString("MM/dd/yyyy");
                    worksheet.Cells[row, 2] = doc.Document;
                    worksheet.Cells[row, 3] = doc.FullInfManagers.FullName;
                    row ++;
                }
                worksheet.get_Range("A1", "C1").EntireColumn.AutoFit();

                var head = worksheet.get_Range("A1", "C1");
                head.Font.Bold = true;
                head.Font.Color = System.Drawing.Color.Blue;
                head.Font.Size = 13;

                string date = Convert.ToString(rn.Next(0x0061, 0x007A));
                String path = String.Format("D:\\extract_freeformat{0}.xls", date);


                workbook.SaveAs(path);
                workbook.Close();
                Marshal.ReleaseComObject(workbook);

                application.Quit();
                Marshal.FinalReleaseComObject(application);

                Response.AddHeader("Content-Disposition", String.Format("attachment;filename={0}", path));
                Response.WriteFile(path);
                Response.End();
                return null;
        }
        // GET: FreeFormatDocs/Details/5
      
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FreeFormatDoc freeFormatDoc = db.FreeFormatDoc.Find(id);
            if (freeFormatDoc == null)
            {
                return HttpNotFound();
            }
            return PartialView(freeFormatDoc);
        }


        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "отклонить")]
        public ActionResult Reject(FreeFormatDoc freeFormatDoc)
        {
            FreeFormatDoc doc = db.FreeFormatDoc.Find(freeFormatDoc.FreeFormatDocID);
            doc.StatusID = 4;
            doc.Comment = freeFormatDoc.Comment;

           // doc.Comment = Request.Form.GetValues("s");
            if (ModelState.IsValid){
                db.Entry(doc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(freeFormatDoc);

        }

      [HttpPost, ActionName("Details")]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "принять")]
        public ActionResult Accept(int id)
        {
            FreeFormatDoc freeFormatDoc = db.FreeFormatDoc.Find(id);
            freeFormatDoc.StatusID = 3;
            freeFormatDoc.Comment = null;
            db.Entry(freeFormatDoc).State = EntityState.Modified;
            db.SaveChanges();
            ArchiveFreeFormatDoc archive = new ArchiveFreeFormatDoc();
            archive.FreeFormatDocID = freeFormatDoc.FreeFormatDocID;
            db.ArchiveFreeFormatDoc.Add(archive);
            db.SaveChanges();
            return RedirectToAction("Index");

        }
        
        // GET: FreeFormatDocs/Create
        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FreeFormatDocID,DocTypeID,Document,ManagerID,UserID,StatusID,Comment,Date")] FreeFormatDoc freeFormatDoc, HttpPostedFileBase upload)
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            
            freeFormatDoc.StatusID =ecp.GET_ECP(user, upload);

            var managers = db.FullInfManagers.ToList();
            
            int index = rn.Next(0, managers.Count);

            if (ModelState.IsValid)
            {
                freeFormatDoc.DocTypeID = 6;
                freeFormatDoc.Date = System.DateTime.Now;
                freeFormatDoc.UserID = user.UserId;
                if (freeFormatDoc.StatusID == 1)
                    freeFormatDoc.ManagerID = managers.ElementAt(index).ManagerID;
                else
                    freeFormatDoc.ManagerID = null;
                db.FreeFormatDoc.Add(freeFormatDoc);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return PartialView(freeFormatDoc);
        }

        public ActionResult Create_by_manager()
        {
            ViewBag.UserID = new SelectList(db.UserInf, "UserID", "FullName");
            return PartialView();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_by_manager(FreeFormatDoc freeFormatDoc)
        {
            var manager = db.FullInfManagers.FirstOrDefault(p => p.Email == User.Identity.Name);


            if (ModelState.IsValid)
            {
                freeFormatDoc.ManagerID = manager.ManagerID;
                freeFormatDoc.DocTypeID = 6;
                freeFormatDoc.Date = System.DateTime.Now;
                freeFormatDoc.StatusID = 3;
                db.FreeFormatDoc.Add(freeFormatDoc);
                db.SaveChanges();

                return RedirectToAction("Index");

            }

            ViewBag.UserID = new SelectList(db.UserInf, "UserID", "FullName", freeFormatDoc.UserID);
            return PartialView(freeFormatDoc);
        }

        

        // GET: FreeFormatDocs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FreeFormatDoc freeFormatDoc = db.FreeFormatDoc.Find(id);
            if (freeFormatDoc == null)
            {
                return HttpNotFound();
            }
            return PartialView(freeFormatDoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FreeFormatDoc freeFormatDoc, HttpPostedFileBase upload){

            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            freeFormatDoc.StatusID = ecp.GET_ECP(user, upload);
            

            var managers = db.FullInfManagers.ToList();
            int index = rn.Next(0, managers.Count);
            FreeFormatDoc old_doc = db.FreeFormatDoc.Find(freeFormatDoc.FreeFormatDocID);
            if (ModelState.IsValid)
            {
                if (freeFormatDoc.StatusID == 1)
                    freeFormatDoc.ManagerID = managers.ElementAt(index).ManagerID;
                else
                    freeFormatDoc.ManagerID = null;
                freeFormatDoc.Comment = old_doc.Comment;
                freeFormatDoc.Date = old_doc.Date;
                freeFormatDoc.DocTypeID = old_doc.DocTypeID;
                freeFormatDoc.UserID = old_doc.UserID;
                db.Entry(old_doc).CurrentValues.SetValues(freeFormatDoc);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return PartialView(freeFormatDoc);
        }

        // GET: FreeFormatDocs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FreeFormatDoc freeFormatDoc = db.FreeFormatDoc.Find(id);
            if (freeFormatDoc == null)
            {
                return HttpNotFound();
            }
            return PartialView(freeFormatDoc);
        }

        // POST: FreeFormatDocs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            FreeFormatDoc freeFormatDoc = db.FreeFormatDoc.Find(id);
            db.FreeFormatDoc.Remove(freeFormatDoc);
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
