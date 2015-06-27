using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using MathNet.Numerics;
using TestMaths.Models;

namespace TestMaths.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Pass new empty model to the View
            var model = new ValueModel();
            return View(model);
        }

        //Calculate PI to arbitrary digits, return as string
        public string Calculate(int numberOfDigits)
        {
            var a = new ArcTanWithBigRational();
            var b = new ArcTanWithBigRational();
            var task1 = Task<BigRational>.Factory.StartNew(
                                    () => a.Calculate((BigRational.One/BigRational.FromInt(5)), 1000, numberOfDigits));
            var task2 = Task<BigRational>.Factory.StartNew(
                                    () => b.Calculate((BigRational.One/BigRational.FromInt(239)), 1000, numberOfDigits));

            var pi = (BigRational.FromInt(16) * task1.Result) - (BigRational.FromInt(4) * task2.Result);

            return BigRationalPiFormatter.Format(pi, numberOfDigits);
        }
        //Calculates Pi to arbitrary significant figure, return as BigRational
        public BigRational BigPi(int numberOfDigits)
        {
            var a = new ArcTanWithBigRational();
            var b = new ArcTanWithBigRational();
            var task1 = Task<BigRational>.Factory.StartNew(
                                    () => a.Calculate((BigRational.One / BigRational.FromInt(5)), 1000, numberOfDigits));
            var task2 = Task<BigRational>.Factory.StartNew(
                                    () => b.Calculate((BigRational.One / BigRational.FromInt(239)), 1000, numberOfDigits));

            var pi = (BigRational.FromInt(16) * task1.Result) - (BigRational.FromInt(4) * task2.Result);
            return pi;
        }
        //Calculate a multiple of Pi to arbitrary significant figures
        [HttpPost]
        public ActionResult CalculateMultPi()
        {
            int nodigits = Convert.ToInt32(Request["txtNoDigits"].ToString());
            BigRational value = BigRational.Parse(Request["txtValue"].ToString());
            BigRational calculation = BigPi(nodigits)*value;
            //let's decorate and send it off
            StringBuilder builder = new StringBuilder();
            builder.Append("<p style=\"word-wrap: break-word;\">");
            //builder.Append(BigRationalPiFormatter.Format(calculation, nodigits));
            var bigstring = BigRationalPiFormatter.Format(calculation, nodigits);
            var bigstringlen = bigstring.Length;
            var i = 0;
            //var leftover = bigstringlen%30;
            while (i<bigstringlen)
            {
                
                    builder.Append(bigstring[i]);
                    i++;
                if (i%90 == 0)
                {
                    builder.Append("<br />");
                }
            }
            builder.Append("</p>");
            return Content(builder.ToString());
        }
        public class BigRationalPiFormatter
        {
            //Let's format any BigRational (e.g. Pi) to a string with specified significant figures
            public static string Format(BigRational pi, int numberOfDigits)
            {
                var numeratorShiftedToEnoughDigits =
                               (pi.Numerator * BigInteger.Pow(new BigInteger(10), numberOfDigits));
                var test = pi.Numerator%pi.Denominator;
                var bigInteger = numeratorShiftedToEnoughDigits / pi.Denominator;
                string piToBeFormatted = bigInteger.ToString();
                int len = piToBeFormatted.Length-numberOfDigits;

                var builder = new StringBuilder();
                builder.Append(piToBeFormatted.Substring(0,len));
                builder.Append(".");
                builder.Append(piToBeFormatted.Substring(len, numberOfDigits - len));
                return builder.ToString();
            }
        }
        //Class to calculate ArcTan using BigRational
        public class ArcTanWithBigRational
        {
            public ArcTanWithBigRational()
            {
                Iterations = 0;
            }

            public int Iterations;

            public BigRational Calculate(BigRational x, int maxNumberOfIterations, int precision)
            {
                bool doASubtract = true;
                var runningTotal = x;
                int count = 0;
                var divisor = 3;
                while (count < maxNumberOfIterations)
                {
                    var current = BigRational.Pow(x, divisor);
                    current = current/BigRational.FromInt(divisor);
                    if (doASubtract)
                    {
                        runningTotal = runningTotal - current;
                    }
                    else
                    {
                        runningTotal = runningTotal + current;
                    }
                    doASubtract = !doASubtract;
                    count++;
                    divisor = divisor + 2;
                    if (!WeHaveEnoughPrecision(current, precision)) continue;
                    Iterations = count;
                    break;
                }
                return runningTotal;
            }

            private static bool WeHaveEnoughPrecision(BigRational current, int precision)
            {
                var fractionPart = (current.Numerator%current.Denominator);
                var thing = BigRational.FromBigInt(fractionPart)/BigRational.FromBigInt(current.Denominator);
                return thing.ToString().Length > precision + 2; //extra 2 digits to ensure enough precision
            }
        }

    }
}