using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Numerics;

namespace EEBank.Methods
{
    public class ECP
    {
        public static int[] Hash(string str)
        {
            int n, H0 = 0;
            n = str.Length * 10;
            int[] num_for_el = new int[n / 10];

            for (int i = 0; i < str.Length; i++)
            {
                num_for_el[i] = Convert.ToByte(str[i]);
            }
            int[] hash_strin = new int[num_for_el.Length];
            for (int i = 0; i < hash_strin.Length; i++)
            {
                int Hi;
                Hi = Convert.ToInt32(Math.Pow(H0 + num_for_el[i], 2) % n);
                hash_strin[i] = Hi;
                H0 = Hi;
            }

            return hash_strin;

        }


        public static bool prime(long num)
        {
            for (long i = 2; i <= Math.Sqrt(num); i++)
            {
                if (num % i == 0)
                    return false;
            }

            return true;
        }


        public static int NOD(int m, int n)
        {
            int nod = 0;
            for (int i = 1; i < (n * m + 1); i++)
            {
                if (m % i == 0 && n % i == 0)
                {
                    nod = i;
                }
            }
            return nod;
        }



        public static bool Two_Prime(int n, int m)
        {
            if (NOD(n, m) == 1)
                return true;
            else
                return false;

        }

        public int[] Key()
        {
            Random rn = new Random();
            ECP pr = new ECP();
            int q = rn.Next(3, 29);
            int p = rn.Next(3, 29);
            while (true)
            {
                if (prime(q))
                    break;
                else
                    q++;
            }

            while (true)
            {
                if (prime(p) && (p > q))
                    break;
                else
                    p++;
            }

            int n = q * p;
            int fi = (p - 1) * (q - 1);
            int e = rn.Next(0, 29);

            while (true)
            {
                if (prime(e) && Two_Prime(e, fi))
                    break;
                else
                    e++;
            }

            int d = rn.Next(3, 29);
            while (true)
            {
                if (prime(d) && ((d * e) % fi == 1))
                    break;
                else
                    d++;
            }

            int[] result = new int[4];
            result[0] = e;
            result[1] = n;
            result[2] = d;
            result[3] = n;

            return result;



        }

        public string Create_ECP(string str, int[] keys)
        {
            Random rn = new Random();
            int[] hash_str = Hash(str);
            int[] ecp = new int[str.Length];
            string r = "";
            for (int i = 0; i < ecp.Length; i++)
            {

                BigInteger k = BigInteger.Pow(hash_str[i], keys[2]) % keys[1];

                r += k;
                r += Convert.ToChar(rn.Next(0x0061, 0x007A));
            }

            return r;

        }
        public double[] Deshifrov_ecp(string str, int open_key_p1, int open_key_p2)
        {
            int len = 0;
            string number1 = "";
            for (int j = 0; j < str.Length; j++)
            {
                if (Char.IsLetter(str[j]) == false)
                {
                    number1 += str[j];
                }
                else
                {
                    len++;
                    number1 = "";
                }

            }
            int iter = 0;
            string res = "";
            double[] arr = new double[len];


            for (int i = 0; i < len; i++)
            {
                string number = "";
                for (int j = iter; j < str.Length; j++)
                {
                    if (Char.IsLetter(str[j]) == false)
                    {
                        number += str[j];
                    }
                    else
                    {
                        iter = j + 1;
                        break;
                    }
                }
                BigInteger jl = Convert.ToInt64(number);
                BigInteger b = BigInteger.Pow(jl, open_key_p1) % open_key_p2;

                res += b;
                res += " ";
            }
            Char separ = ' ';
            String[] st = res.Split(separ);
            for (int i = 0; i < st.Length - 1; i++)
                arr[i] = Convert.ToInt32(st[i]);


            return arr;
        }
    }
}

        




    

