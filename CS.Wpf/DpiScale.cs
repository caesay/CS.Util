using System;
using System.Windows;
using System.Windows.Media;

namespace CS.Wpf
{
    public static class DpiScale
    {
        public static double DpiX { get; private set; }
        public static double DpiY { get; private set; }

        private static Point _scalePoint;
        private static bool _initialized;

        public static Transform DownScaleTransform
        {
            get
            {
                EnsureInitialized();
                return new ScaleTransform(1 / _scalePoint.X, 1 / _scalePoint.Y);
            }
        }
        public static Transform UpScaleTransform
        {
            get
            {
                EnsureInitialized();
                return new ScaleTransform(_scalePoint.X, _scalePoint.Y);
            }
        }

        public static void ScaleUISetup(double dpiX, double dpiY)
        {
            DpiX = dpiX;
            DpiY = dpiY;
            _scalePoint.X = dpiX / 96;
            _scalePoint.Y = dpiY / 96;
            _initialized = true;
        }

        public static System.Drawing.Rectangle TranslateDownScaleRectangle(System.Drawing.Rectangle rect)
        {
            EnsureInitialized();
            var rectangle = new System.Drawing.Rectangle((int)Math.Round((double)rect.X / _scalePoint.X), (int)Math.Round((double)rect.Y / _scalePoint.Y), (int)Math.Round((double)rect.Width / _scalePoint.X), (int)Math.Round((double)rect.Height / _scalePoint.Y));
            return rectangle;
        }
        public static System.Drawing.Rectangle DownScaleRectangle(System.Drawing.Rectangle rect)
        {
            EnsureInitialized();
            var w = (int)Math.Round((double)rect.Width / _scalePoint.X);
            var h = (int)Math.Round((double)rect.Height / _scalePoint.Y);

            return new System.Drawing.Rectangle(rect.X, rect.Y, w, h);
        }
        public static Rect TranslateDownScaleRect(Rect rect)
        {
            EnsureInitialized();
            Rect rect1 = new Rect(rect.X / _scalePoint.X, rect.Y / _scalePoint.Y, rect.Width / _scalePoint.X, rect.Height / _scalePoint.Y);
            return rect1;
        }
        public static Point DownScalePoint(Point point)
        {
            EnsureInitialized();
            return new Point(point.X / _scalePoint.X, point.Y / _scalePoint.Y);
        }
        public static Rect DownScaleRect(Rect rect)
        {
            EnsureInitialized();
            Rect rect1 = new Rect(rect.Left, rect.Top, rect.Width / _scalePoint.X, rect.Height / _scalePoint.Y);
            return rect1;
        }
        public static Size DownScaleSize(Size size)
        {
            EnsureInitialized();
            Size size1 = new Size(size.Width / _scalePoint.X, size.Height / _scalePoint.Y);
            return size1;
        }
        public static double DownScaleX(double x)
        {
            EnsureInitialized();
            return x / _scalePoint.X;
        }
        public static double DownScaleY(double y)
        {
            EnsureInitialized();
            return y / _scalePoint.Y;
        }

        public static System.Drawing.Rectangle TranslateUpScaleRectangle(System.Drawing.Rectangle rect)
        {
            EnsureInitialized();
            var rectangle = new System.Drawing.Rectangle((int)Math.Round((double)rect.X * _scalePoint.X), (int)Math.Round((double)rect.Y * _scalePoint.Y), (int)Math.Round((double)rect.Width * _scalePoint.X), (int)Math.Round((double)rect.Height * _scalePoint.Y));
            return rectangle;
        }
        public static System.Drawing.Rectangle UpScaleRectangle(System.Drawing.Rectangle rect)
        {
            EnsureInitialized();
            var rectangle = new System.Drawing.Rectangle(rect.X, rect.Y, (int)Math.Round((double)rect.Width * _scalePoint.X), (int)Math.Round((double)rect.Height * _scalePoint.Y));
            return rectangle;
        }
        public static Rect TranslateUpScaleRect(Rect rect)
        {
            EnsureInitialized();
            Rect rect1 = new Rect(
                rect.Left * _scalePoint.X,
                rect.Top * _scalePoint.Y,
                rect.Width * _scalePoint.X,
                rect.Height * _scalePoint.Y);
            return rect1;
        }
        public static Point UpScalePoint(Point point)
        {
            EnsureInitialized();
            Point point1 = new Point(point.X * _scalePoint.X, point.Y * _scalePoint.Y);
            return point1;
        }
        public static Rect UpScaleRect(Rect rect)
        {
            EnsureInitialized();
            Rect rect1 = new Rect(rect.Left, rect.Top, rect.Width * _scalePoint.X, rect.Height * _scalePoint.Y);
            return rect1;
        }
        public static Size UpScaleSize(Size size)
        {
            EnsureInitialized();
            Size size1 = new Size(size.Width * _scalePoint.X, size.Height * _scalePoint.Y);
            return size1;
        }
        public static double UpScaleX(double x)
        {
            EnsureInitialized();
            return x * _scalePoint.X;
        }
        public static double UpScaleY(double y)
        {
            EnsureInitialized();
            return y * _scalePoint.Y;
        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException("DpiScale must be initialized with a call to 'ScaleUISetup' " +
                                                    "before using transformation methods.");
        }
    }
}