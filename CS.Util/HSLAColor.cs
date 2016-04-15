using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CS.Util.Extensions;

namespace CS.Util
{
    /// <summary>
    /// Represents a color in HSLA format (Hue, Saturation, Lightness, Alpha). 
    /// This class can be interchanged implicitly with the built-in Color classes.
    /// </summary>
    public class HSLAColor : ICloneable, IEquatable<HSLAColor>
    {
        /// <summary>
        /// This is a value from 0 to 360 inclusive that represents the current color.
        /// </summary>
        public double Hue
        {
            get
            {
                var h = _hue * 60d;
                if (h < 0) h += 360;
                return h;
            }
            set
            {
                if (value < 0 || value > 360)
                    throw new ArgumentOutOfRangeException("value", "Hue must be >= 0 and <= to 360");
                _hue = value / 60d;
            }
        }
        /// <summary>
        /// This is a value from 0 to 100 inclusive that represents the intensity of the current Hue. 
        /// 0 is solid gray with no color at all, 100 is full color saturation (default 100)
        /// </summary>
        public double Saturation
        {
            get { return _sat * 100; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("value", "Saturation must be >= 0 and <= to 100");
                _sat = value / 100;
            }
        }
        /// <summary>
        /// This is a value from 0 to 100 inclusive that represents the light or darkness of the current Hue. 
        /// 0 is solid black, and 100 is solid white irrespective of the Hue or Saturation. (default 50)
        /// </summary>
        public double Lightness
        {
            get { return _lum * 100; }
            set
            {
                if (value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("value", "Lightness must be >= 0 and <= to 100");
                _lum = value / 100;
            }
        }
        /// <summary>
        /// This is a value from 0 to 100 inclusive that represents the transparency of the current color.
        /// 0 is completely transparent, and 100 is fully opaque. (default 100)
        /// </summary>
        public double Alpha
        {
            get { return _alp * 100; }
            set
            {
                if(value < 0 || value > 100)
                    throw new ArgumentOutOfRangeException("value", "Alpha must be >= 0 and <= to 100");
                _alp = value / 100;
            }
        }

        private double _hue;
        private double _sat;
        private double _lum;
        private double _alp;

        private HSLAColor(double H, double S, double L, double A)
        {
            _hue = H;
            _sat = S;
            _lum = L;
            _alp = A;
        }

        public static HSLAColor FromARGB(Color color)
        {
            return FromARGB(color.A, color.R, color.G, color.B);
        }
        public static HSLAColor FromARGB(byte A, byte R, byte G, byte B)
        {
            double _A = (A / 255d);
            double _R = (R / 255d);
            double _G = (G / 255d);
            double _B = (B / 255d);

            double _Min = Math.Min(Math.Min(_R, _G), _B);
            double _Max = Math.Max(Math.Max(_R, _G), _B);
            double _Delta = _Max - _Min;

            double H = 0;
            double S = 0;
            double L = (double)((_Max + _Min) / 2.0d);

            if (_Delta != 0)
            {
                if (L < 0.5d)
                {
                    S = (double)(_Delta / (_Max + _Min));
                }
                else
                {
                    S = (double)(_Delta / (2.0d - _Max - _Min));
                }


                if (_R == _Max)
                {
                    H = (_G - _B) / _Delta;
                }
                else if (_G == _Max)
                {
                    H = 2d + (_B - _R) / _Delta;
                }
                else if (_B == _Max)
                {
                    H = 4d + (_R - _G) / _Delta;
                }
            }

            return new HSLAColor(H, S, L, _A);
        }
        public static HSLAColor FromARGB(System.Drawing.Color color)
        {
            return FromARGB(color.A, color.R, color.G, color.B);
        }

        private static double HueToColor(double c, double t1, double t2)
        {
            if (c < 0) c += 1d;
            if (c > 1) c -= 1d;
            if (6.0d * c < 1.0d) return t1 + (t2 - t1) * 6.0d * c;
            if (2.0d * c < 1.0d) return t2;
            if (3.0d * c < 2.0d) return t1 + (t2 - t1) * (2.0d / 3.0d - c) * 6.0d;
            return t1;
        }

        public Color ToARGB()
        {
            byte a, r, g, b;
            a = (byte)Math.Round(_alp * 255d);
            if (_sat == 0)
            {
                r = (byte)Math.Round(_lum * 255d);
                g = (byte)Math.Round(_lum * 255d);
                b = (byte)Math.Round(_lum * 255d);
            }
            else
            {
                double t1, t2;
                double th = _hue / 6.0d;

                if (_lum < 0.5d)
                {
                    t2 = _lum * (1d + _sat);
                }
                else
                {
                    t2 = (_lum + _sat) - (_lum * _sat);
                }
                t1 = 2d * _lum - t2;

                double tr, tg, tb;
                tr = th + (1.0d / 3.0d);
                tg = th;
                tb = th - (1.0d / 3.0d);

                tr = HueToColor(tr, t1, t2);
                tg = HueToColor(tg, t1, t2);
                tb = HueToColor(tb, t1, t2);
                r = (byte)Math.Round(tr * 255d);
                g = (byte)Math.Round(tg * 255d);
                b = (byte)Math.Round(tb * 255d);
            }
            return Color.FromArgb(a, r, g, b);
        }

        public HSLAColor Clone()
        {
            return new HSLAColor(_hue, _sat, _lum, _alp);
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public override bool Equals(object obj)
        {
            var cast = obj as HSLAColor;
            if (cast == null)
                return false;
            return Equals(cast);
        }
        public bool Equals(HSLAColor other)
        {
            if (other == null) return false;
            double tolerance = 2;
            return Math.Abs(other._hue - _hue) < tolerance
                && Math.Abs(other._lum - _lum) < tolerance
                && Math.Abs(other._sat - _sat) < tolerance
                && Math.Abs(other._alp - _alp) < tolerance;
        }
        public override int GetHashCode()
        {
            return this.GetAutoHashCode(_hue, _sat, _lum, _alp);
        }
        public override string ToString()
        {
            return $"HSLColor:{{H:{Hue}, S:{Saturation}, L:{Lightness} A:{Alpha}}}";
        }

        public static implicit operator HSLAColor(System.Drawing.Color rgb)
        {
            return FromARGB(rgb);
        }
        public static implicit operator HSLAColor(Color rgb)
        {
            return FromARGB(rgb);
        }
        public static implicit operator Color(HSLAColor hsla)
        {
            return hsla.ToARGB();
        }
        public static implicit operator System.Drawing.Color(HSLAColor hsla)
        {
            var wcolor = hsla.ToARGB();
            return System.Drawing.Color.FromArgb(wcolor.A, wcolor.R, wcolor.G, wcolor.B);
        }
    }
}
