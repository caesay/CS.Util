using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;

namespace CS.Util
{
    internal static class DynMath<T>
    {
        //this uses the dynamic keyword, so the first time executing a method for a certain type
        //will be expensive, but after that it's cached by the runtime and just as fast.

        public static T Add(T one, T two)
        {
            try
            {
                return (dynamic)one + (dynamic)two;
            }
            catch (RuntimeBinderException)
            {
                throw new ArgumentException("The specified type does not have a addition operator.");
            }
        }
        public static T Multiply(T one, T two)
        {
            try
            {
                return (dynamic)one * (dynamic)two;
            }
            catch (RuntimeBinderException)
            {
                throw new ArgumentException("The specified type does not have a multiplication operator.");
            }
        }
        public static T Subtract(T one, T two)
        {
            try
            {
                return (dynamic)one - (dynamic)two;
            }
            catch (RuntimeBinderException)
            {
                throw new ArgumentException("The specified type does not have a subtraction operator.");
            }
        }
        public static T Divide(T one, T two)
        {
            try
            {
                return (dynamic)one / (dynamic)two;
            }
            catch (RuntimeBinderException)
            {
                throw new ArgumentException("The specified type does not have a division operator.");
            }
        }
        public static T Negate(T one)
        {
            try
            {
                return -(dynamic)one;
            }
            catch (RuntimeBinderException)
            {
                throw new ArgumentException("The specified type does not have a negate operator.");
            }
        }
    }

    public class Fraction<T>
    {
        public T Numerator { get; private set; }
        public T Denominator { get; private set; }

        private const double _equalityTolerance = 0.001;

        public Fraction(T numerator, T denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public static Fraction<T> operator +(Fraction<T> a, Fraction<T> b)
        {
            var res = new Fraction<T>(
                DynMath<T>.Add(DynMath<T>.Multiply(a.Numerator, b.Denominator), DynMath<T>.Multiply(b.Numerator, a.Denominator)),
                DynMath<T>.Multiply(a.Denominator, b.Denominator));
            res.Simplify();
            return res;
        }
        public static Fraction<T> operator -(Fraction<T> a, Fraction<T> b)
        {
            var res = new Fraction<T>(
                DynMath<T>.Subtract(DynMath<T>.Multiply(a.Numerator, b.Denominator), DynMath<T>.Multiply(b.Numerator, a.Denominator)),
                DynMath<T>.Multiply(a.Denominator, b.Denominator));
            res.Simplify();
            return res;
        }
        public static Fraction<T> operator /(Fraction<T> a, Fraction<T> b)
        {
            var res = new Fraction<T>(
                DynMath<T>.Multiply(a.Numerator, b.Denominator),
                DynMath<T>.Multiply(a.Denominator, b.Numerator));
            res.Simplify();
            return res;
        }
        public static Fraction<T> operator *(Fraction<T> a, Fraction<T> b)
        {
            var res = new Fraction<T>(
                DynMath<T>.Multiply(a.Numerator, b.Numerator),
                DynMath<T>.Multiply(a.Denominator, b.Denominator));
            res.Simplify();
            return res;
        }

        public static bool operator <(Fraction<T> a, Fraction<T> b)
        {
            return (dynamic)a.Numerator / a.Denominator < (dynamic)b.Numerator / b.Denominator;
        }
        public static bool operator >(Fraction<T> a, Fraction<T> b)
        {
            return (dynamic)a.Numerator / a.Denominator > (dynamic)b.Numerator / b.Denominator;
        }
        public static bool operator <=(Fraction<T> a, Fraction<T> b)
        {
            return (dynamic)a.Numerator / a.Denominator <= (dynamic)b.Numerator / b.Denominator;
        }
        public static bool operator >=(Fraction<T> a, Fraction<T> b)
        {
            return (dynamic)a.Numerator / a.Denominator >= (dynamic)b.Numerator / b.Denominator;
        }
        public static bool operator ==(Fraction<T> a, Fraction<T> b)
        {
            if (a == null && (b == null)) return true;
            if (a == null != (b == null)) return false;

            try
            {
                dynamic one = (dynamic)a.Numerator / a.Denominator;
                dynamic two = (dynamic)b.Numerator / b.Denominator;
                dynamic result = Math.Abs(one - two);
                return result < _equalityTolerance;
            }
            catch (RuntimeBinderException)
            {
            }
            return a.Numerator.Equals(b.Numerator) && a.Denominator.Equals(b.Denominator);
        }
        public static bool operator !=(Fraction<T> a, Fraction<T> b)
        {
            if (a == null && (b == null)) return false;
            if (a == null != (b == null)) return true;

            try
            {
                dynamic one = (dynamic)a.Numerator / a.Denominator;
                dynamic two = (dynamic)b.Numerator / b.Denominator;
                dynamic result = Math.Abs(one - two);
                return result >= _equalityTolerance;
            }
            catch (RuntimeBinderException)
            {
            }
            return !(a.Numerator.Equals(b.Numerator) && a.Denominator.Equals(b.Denominator));
        }

        public static Fraction<T> NumberToFraction(dynamic value)
        {
            dynamic error = _equalityTolerance;
            int sign = Math.Sign(value);
            if (sign == -1)
                value = Math.Abs(value);
            if (sign != 0)
                error *= value;

            dynamic n = Math.Floor((double)value);
            value -= n;
            if (value < error)
            {
                return new Fraction<T>((T)(sign * n), (T)(dynamic)1);
            }
            if (1 - error < value)
            {
                return new Fraction<T>((T)(sign * (n + 1)), (T)(dynamic)1);
            }

            // The lower fraction is 0/1
            dynamic lower_n = 0;
            dynamic lower_d = 1;

            // The upper fraction is 1/1
            dynamic upper_n = 1;
            dynamic upper_d = 1;

            while (true)
            {
                // The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
                dynamic middle_n = lower_n + upper_n;
                dynamic middle_d = lower_d + upper_d;

                if (middle_d * (value + error) < middle_n)
                {
                    // real + error < middle : middle is our new upper
                    upper_n = middle_n;
                    upper_d = middle_d;
                }
                else if (middle_n < (value - error) * middle_d)
                {
                    // middle < real - error : middle is our new lower
                    lower_n = middle_n;
                    lower_d = middle_d;
                }
                else
                {
                    // Middle is our best fraction
                    return new Fraction<T>((T)((n * middle_d + middle_n) * sign), (T)middle_d);
                }
            }
        }

        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }

        private void Simplify()
        {
            dynamic gcd = GCD(Numerator, Denominator);
            Numerator = Numerator / gcd;
            Denominator = Denominator / gcd;
        }
        dynamic GCD(params dynamic[] args)
        {
            return args.Aggregate((a, b) =>
            {
                while (b > 0)
                {
                    int rem = a % b;
                    a = b;
                    b = rem;
                }
                return a;
            });
        }
    }
}
