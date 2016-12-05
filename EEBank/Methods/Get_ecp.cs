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


namespace EEBank.Methods
{
    public class Get_ecp
    {
        private EEBankEntitie db = new EEBankEntitie();
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
                upload.SaveAs(System.Web.HttpContext.Current.Server.MapPath("~/Files/" + fileName));


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

            string keys = user.UserInf1.Where(m => m.Email == System.Web.HttpContext.Current.User.Identity.Name).FirstOrDefault().OpenKey;

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
    }
}