using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using CS.Wpf.Annotations;

namespace CS.Wpf.Brushes
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public class CheckeredBrush : UpdatableMarkupExtension, INotifyPropertyChanged
    {
        public double BlockSize
        {
            get { return _blockSize; }
            set
            {
                if (value.Equals(_blockSize)) return;
                _blockSize = value;
                OnPropertyChanged(nameof(BlockSize));
            }
        }
        public Brush Background
        {
            get { return _background; }
            set
            {
                if (Equals(value, _background)) return;
                _background = value;
                OnPropertyChanged(nameof(Background));
            }
        }
        public Brush Foreground
        {
            get { return _foreground; }
            set
            {
                if (Equals(value, _foreground)) return;
                _foreground = value;
                OnPropertyChanged(nameof(Foreground));
            }
        }

        private double _blockSize = 25d;
        private Brush _background = System.Windows.Media.Brushes.Black;
        private Brush _foreground = System.Windows.Media.Brushes.White;

        protected override object ProvideInitialValue(IServiceProvider serviceProvider)
        {
            return CreateBrush();
        }

        private DrawingBrush CreateBrush()
        {
            var half = BlockSize / 2;
            var brush = new DrawingBrush();
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.Tile;
            brush.ViewportUnits = BrushMappingMode.Absolute;
            brush.Viewport = new Rect(0, 0, BlockSize, BlockSize);
            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing(Background, null, new RectangleGeometry(new Rect(0, 0, BlockSize, BlockSize))));
            var geometryGroup = new GeometryGroup();
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, half, half)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(half, half, half, half)));
            drawingGroup.Children.Add(new GeometryDrawing(Foreground, null, geometryGroup));
            brush.Drawing = drawingGroup;
            brush.Freeze();
            return brush;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.UpdateValue(CreateBrush());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
