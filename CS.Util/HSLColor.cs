using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CS.Util.Extensions;

namespace CS.Util
{
    public class HSLColor : ICloneable, IEquatable<HSLColor>
    {
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
                _hue = value / 60d;
            }
        }
        public double Saturation
        {
            get { return _sat * 100; }
            set { _sat = value / 100; }
        }
        public double Lightness
        {
            get { return _lum * 100; }
            set { _lum = value / 100; }
        }

        private double _hue;
        private double _sat;
        private double _lum;

        private HSLColor(double H, double S, double L)
        {
            _hue = H;
            _sat = S;
            _lum = L;
        }

        public static HSLColor FromRGB(Color color)
        {
            return FromRGB(color.R, color.G, color.B);
        }
        public static HSLColor FromRGB(byte R, byte G, byte B)
        {
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

            return new HSLColor(H, S, L);
        }
        public static HSLColor FromRGB(System.Drawing.Color color)
        {
            return FromRGB(color.R, color.G, color.B);
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

        public Color ToRGB()
        {
            byte r, g, b;
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
            return Color.FromRgb(r, g, b);
        }
        public bool IsBoring()
        {
            //these are arbitrary, i just tweaked them for what i thought looked good.
            if (Lightness < 25 || Lightness > 70 || Saturation < 35)
            {
                return true;
            }
            return false;
        }
        public void Excite()
        {
            //these are arbitrary, i just tweaked them for what i thought looked good.
            Saturation = Math.Max(Saturation, 45);
            Lightness = Math.Max(Lightness, 25);
            Lightness = Math.Min(Lightness, 70);
        }

        public HSLColor Clone()
        {
            return new HSLColor(_hue, _sat, _lum);
        }
        object ICloneable.Clone()
        {
            return new HSLColor(_hue, _sat, _lum);
        }
        public override bool Equals(object obj)
        {
            var cast = obj as HSLColor;
            if (cast == null)
                return false;
            return Equals(cast);
        }
        public bool Equals(HSLColor other)
        {
            double tolerance = 2;
            return Math.Abs(other._hue - _hue) < tolerance
                && Math.Abs(other._lum - _lum) < tolerance
                && Math.Abs(other._sat - _sat) < tolerance;
        }
        public override int GetHashCode()
        {
            return this.GetAutoHashCode(MemberSelector.PrivateFields);
        }
        public override string ToString()
        {
            return $"{{Hue:{Hue}, Saturation:{Saturation}, Lightness:{Lightness}}}";
        }
    }
}
