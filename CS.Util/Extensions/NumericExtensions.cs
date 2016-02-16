using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class NumericExtensions
    {
        public static string AsPrettyString(this bool value)
        {
            return (value) ? "True" : "False";
        }
        public static string AsPrettyString(this ushort number)
        {
            return ((long)number).AsPrettyString();
        }
        public static string AsPrettyString(this short number)
        {
            return ((long)number).AsPrettyString();
        }
        public static string AsPrettyString(this int number)
        {
            return ((long)number).AsPrettyString();
        }
        public static string AsPrettyString(this uint number)
        {
            return ((long)number).AsPrettyString();
        }
        public static string AsPrettyString(this long number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + AsPrettyString(-number);

            string words = "";

            if ((number / 1000000000000000000) > 0)
            {
                words += " " + AsPrettyString(number / 1000000000000000000) + " quintillion";
                number %= 1000000000000000000;
            }

            if ((number / 1000000000000000) > 0)
            {
                words += " " + AsPrettyString(number / 1000000000000000) + " quadrillon";
                number %= 1000000000000000;
            }

            if ((number / 1000000000000) > 0)
            {
                words += " " + AsPrettyString(number / 1000000000000) + " trillion";
                number %= 1000000000000;
            }

            if ((number / 1000000000) > 0)
            {
                words += " " + AsPrettyString(number / 1000000000) + " billion";
                number %= 1000000000;
            }

            if ((number / 1000000) > 0)
            {
                words += " " + AsPrettyString(number / 1000000) + " million";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += " " + AsPrettyString(number / 1000) + " thousand";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += " " + AsPrettyString(number / 100) + " hundred";
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
        public static string AsPrettyString(this DateTime date)
        {
            return PrettyTime.Format(date);
        }
        public static string AsPrettyString(this TimeSpan time)
        {
            return PrettyTime.Format(time);
        }

        public static byte[] GetBytes(this bool value)
        {
            return BitConverter.GetBytes(value);
        }
        public static byte[] GetBytes(this char value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }
        public static byte[] GetBytes(this short value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }
        public static byte[] GetBytes(this int value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }
        public static byte[] GetBytes(this long value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }
        public static byte[] GetBytes(this ushort value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }
        public static byte[] GetBytes(this uint value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }
        public static byte[] GetBytes(this ulong value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }
        public static byte[] GetBytes(this float value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }
        public static byte[] GetBytes(this double value, bool inLittleEndian = true)
        {
            var converter = inLittleEndian ? EndianBitConverter.Little : EndianBitConverter.Big;
            return converter.GetBytes(value);
        }

        public static bool ApproxEquals(this float d1, double d2)
        {
            return ApproxEquals((double)d1, d2);
        }
        public static bool ApproxEquals(this double d1, double d2)
        {
            const double epsilon = 2.2204460492503131E-16;

            if (d1 == d2)
                return true;

            double tolerance = ((Math.Abs(d1) + Math.Abs(d2)) + 10.0) * epsilon;
            double difference = d1 - d2;

            return (-tolerance < difference && tolerance > difference);
        }

        private const string _base36Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string ToRadix(this ushort number, int toRadix)
        {
            return ((long)number).ToRadix(toRadix);
        }
        public static string ToRadix(this short number, int toRadix)
        {
            return ((long)number).ToRadix(toRadix);
        }
        public static string ToRadix(this uint number, int toRadix)
        {
            return ((long)number).ToRadix(toRadix);
        }
        public static string ToRadix(this int number, int toRadix)
        {
            return ((long)number).ToRadix(toRadix);
        }
        public static string ToRadix(this long number, int toRadix)
        {
            if (toRadix < 2 || toRadix > 36)
                throw new ArgumentException("The radix must be >= 2 and <= 36");
            StringBuilder result = new StringBuilder();
            number = Math.Abs(number);
            while (number > 0)
            {
                result.Insert(0, _base36Characters[(int)(number % toRadix)]);
                number /= toRadix;
            }
            return result.ToString().ToLower();
        }
        public static long FromRadix(this string number, int fromRadix)
        {
            if (fromRadix < 2 || fromRadix > 36)
                throw new ArgumentException("The radix must be >= 2 and <= 36");
            number = number.ToUpper();
            long result = 0, multiplier = 1;
            foreach (var ch in number.ToCharArray().Reverse())
            {
                int position = _base36Characters.IndexOf(ch);
                if (position == -1 || position > fromRadix)
                    throw new ArgumentException("Invalid character in number input string");
                result += position * multiplier;
                multiplier *= fromRadix;
            }
            return result;
        }
    }
}
