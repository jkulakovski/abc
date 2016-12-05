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
    public class FreeFormatDocsController : Controller
    {
        private EEBankEntitie db = new EEBankEntitie();

        // GET: FreeFormatDocs
        public ActionResult Index()
        {

            var user = db.Users.FirstOrDefault(p => p.Email == User.Identity.Name);
            if (user.RoleId == 5)
            {
                var freeFormatDoc = db.FreeFormatDoc.Where(p => p.Users.Email == User.Identity.Name).Where(p => p.StatusID != 3);
                return View(freeFormatDoc.ToList());
            }
            if (user.RoleId == 6)
            {
                var freeFormatDoc = db.FreeFormatDoc.Include(p => p.Users);
                return View(freeFormatDoc.ToList());
            }
            else
            {
                var freeFormatDoc = db.FreeFormatDoc.Include(p => p.DocType).Where(p => p.FullInfManagers.Email == user.Email).Where(p => p.StatusID == 1);
                return View(freeFormatDoc.ToList());
            }
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
            return View(freeFormatDoc);
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
            return RedirectToAction("Index");

        }
        
        // GET: FreeFormatDocs/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "FreeFormatDocID,DocTypeID,Document,ManagerID,UserID,StatusID,Comment,Date")] FreeFormatDoc freeFormatDoc, HttpPostedFileBase upload)
        {
            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            freeFormatDoc.StatusID = GET_ECP(user, upload);

            var managers = db.FullInfManagers.ToList();
            Random rn = new Random();
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

            return View(freeFormatDoc);
        }

        public ActionResult Create_by_manager()
        {
            ViewBag.UserID = new SelectList(db.UserInf, "UserID", "FullName");
            return View();
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
            return View(freeFormatDoc);
        }

        public int GET_ECP(Users user, HttpPostedFileBase upload)
        {
            
            string s = "";

            if (upload != null)
            {
                // получаем имя файла
                string fileName = System.IO.Path.GetFileName(upload.FileName);
                Random rn = new Random();
                // сохраняем файл в папку Files в проекте
                fileName = fileName + Convert.ToString(rn.Next(0x0061, 0x007A));
                upload.SaveAs(Server.MapPath("~/Files/" + fileName ));


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
            if (it == res.Length)
                return 1;
            if (it != res.Length || it == 0)
                return 2;
            else
                return 0;
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
            return View(freeFormatDoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(FreeFormatDoc freeFormatDoc, HttpPostedFileBase upload){

            var user = db.Users.Where(p => p.Email == User.Identity.Name).FirstOrDefault();
            freeFormatDoc.StatusID = GET_ECP(user, upload);
            

            var managers = db.FullInfManagers.ToList();
            Random rn = new Random();
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
            return View(freeFormatDoc);
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
            return View(freeFormatDoc);
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
