using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class NumericExtensions
    {
        /// <summary>
        /// Converts the integer to a textual representation using English words. 
        /// For example, 142.ToWords() is "one hundred and forty-two".
        /// </summary>
        public static string ToWords(this int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + ToWords(-number);

            string words = "";

            if ((number / 1000000000) > 0)
            {
                words += " " + ToWords(number / 1000000000) + " billion";
                number %= 1000000000;
            }

            if ((number / 1000000) > 0)
            {
                words += " " + ToWords(number / 1000000) + " million";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += " " + ToWords(number / 1000) + " thousand";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += " " + ToWords(number / 100) + " hundred";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += " and";

                var unitsMap = new[] { null, "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { null, "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += " " + unitsMap[number];
                else
                {
                    words += " " + tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words.Substring(1);
        }
    }
}
