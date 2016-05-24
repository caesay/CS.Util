using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CS.Wpf.Controls
{
    public class ExpandingStackPanel : StackPanel
    {
        public static bool GetExpandable(UIElement obj)
        {
            return (bool)obj.GetValue(ExpandableProperty);
        }

        public static void SetExpandable(UIElement obj, bool value)
        {
            obj.SetValue(ExpandableProperty, value);
        }

        public static readonly DependencyProperty ExpandableProperty =
            DependencyProperty.RegisterAttached("Expandable", typeof(bool), typeof(ExpandingStackPanel), new PropertyMetadata(false, Expandable_Changed));

        private static void Expandable_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var el = obj as FrameworkElement;

            if ((bool)e.NewValue)
            {
                // the object is marked expandable, wrap it in a ExpandableWrapper.
                var parent = el?.Parent as ExpandingStackPanel;
                if (parent == null)
                    return;

                var index = parent.Children.IndexOf(el);
                if (index < 0)
                    return;
                parent.Children.RemoveAt(index);
                var wrapper = new ExpandableWrapper(el);
                parent.Children.Insert(index, wrapper);

                if (DesignerProperties.GetIsInDesignMode(parent))
                {
                    wrapper.Opacity = 0.4;
                }
                else if (!parent.Expanded)
                {
                    wrapper.Opacity = 0;
                    if (parent.Orientation == Orientation.Vertical)
                        wrapper.Scale.ScaleY = 0;
                    else if (parent.Orientation == Orientation.Horizontal)
                        wrapper.Scale.ScaleX = 0;
                    el.Focusable = false;
                }
            }
            else
            {
                // the object is marked not expandable, remove a wrapper if it exists
                var wrapper = el?.Parent as ExpandableWrapper;
                var parent = wrapper?.Parent as ExpandingStackPanel;
                if (parent == null)
                    return;

                var index = parent.Children.IndexOf(wrapper);
                if (index < 0)
                    return;
                parent.Children.RemoveAt(index);
                parent.Children.Insert(index, el);
                el.Focusable = wrapper.ChildFocusableDefault;
            }
        }

        public bool Expanded { get; private set; }

        private readonly PropertyChangedCallback _defaultCallback;

        public ExpandingStackPanel()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _defaultCallback = OrientationProperty?.GetMetadata(typeof(StackPanel))?.PropertyChangedCallback;

                var metadata = new FrameworkPropertyMetadata(
                    Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnOrientationChanged));

                OrientationProperty?.OverrideMetadata(typeof(ExpandingStackPanel), metadata);
            }
        }

        private void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _defaultCallback?.Invoke(d, e);
            if (Expanded)
                return;

            foreach (var c in Children.OfType<ExpandableWrapper>())
            {
                if (((System.Windows.Controls.Orientation)e.NewValue) == Orientation.Horizontal)
                {
                    c.Scale.ScaleY = 1;
                    c.Scale.ScaleX = 0;
                }
                else
                {
                    c.Scale.ScaleY = 0;
                    c.Scale.ScaleX = 1;
                }
            }
        }

        public void Expand()
        {
            Expanded = true;
            BeginStoryboard(CreateAnimation(Children.OfType<ExpandableWrapper>(), true), HandoffBehavior.SnapshotAndReplace);
        }

        public void Collapse()
        {
            Expanded = false;
            BeginStoryboard(CreateAnimation(Children.OfType<ExpandableWrapper>(), false), HandoffBehavior.SnapshotAndReplace);
        }

        private Storyboard CreateAnimation(IEnumerable<ExpandableWrapper> controls, bool visible)
        {
            Storyboard sb = new Storyboard();
            sb.FillBehavior = FillBehavior.HoldEnd;

            var ease = new PowerEase() { Power = 5 };
            var duration = TimeSpan.FromMilliseconds(700);

            foreach (var c in controls)
            {
                DoubleAnimation opacity = new DoubleAnimation(visible ? 1 : 0, duration);
                opacity.EasingFunction = ease;
                opacity.SetValue(Storyboard.TargetProperty, c);
                opacity.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(ExpandableWrapper.OpacityProperty));
                sb.Children.Add(opacity);

                // replace the current transform with one that's not currently locked by an old animation.
                ScaleTransform st = new ScaleTransform(c.Scale.ScaleX, c.Scale.ScaleY);
                if (Orientation == Orientation.Horizontal)
                    st.ScaleY = 1;
                else
                    st.ScaleX = 1;
                c.Scale = st;

                DoubleAnimation size = new DoubleAnimation(visible ? 1 : 0, duration);
                size.EasingFunction = ease;
                size.SetValue(Storyboard.TargetProperty, c);
                size.SetValue(Storyboard.TargetPropertyProperty,
                    new PropertyPath(Orientation == Orientation.Horizontal ? "LayoutTransform.ScaleX" : "LayoutTransform.ScaleY"));
                sb.Children.Add(size);

                c.Child.Focusable = visible && c.ChildFocusableDefault;
            }
            return sb;
        }

        private class ExpandableWrapper : ContentControl
        {
            public bool ChildFocusableDefault { get; }

            public FrameworkElement Child { get; }

            public ScaleTransform Scale
            {
                get { return (ScaleTransform)LayoutTransform; }
                set { LayoutTransform = value; }
            }

            public ExpandableWrapper(FrameworkElement child)
            {
                ChildFocusableDefault = child.Focusable;
                LayoutTransform = new ScaleTransform(1, 1);
                Focusable = false;
                Child = child;
                this.Content = child;
                ClipToBounds = true;
            }
        }
    }
}
