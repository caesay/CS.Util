﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CS.Wpf.Controls
{
    public static class PhotoLoader
    {
        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static string GetSource(Image obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }

        public static void SetSource(Image obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(string), typeof(PhotoLoader), new UIPropertyMetadata(string.Empty, OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Manager.Instance.LoadImage(e.NewValue as string, d as Image);
        }


        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetDisplayWaitingAnimationDuringLoading(Image obj)
        {
            return (bool)obj.GetValue(DisplayWaitingAnimationDuringLoadingProperty);
        }

        public static void SetDisplayWaitingAnimationDuringLoading(Image obj, bool value)
        {
            obj.SetValue(DisplayWaitingAnimationDuringLoadingProperty, value);
        }
        public static readonly DependencyProperty DisplayWaitingAnimationDuringLoadingProperty =
            DependencyProperty.RegisterAttached("DisplayWaitingAnimationDuringLoading", typeof(bool), typeof(PhotoLoader), new UIPropertyMetadata(true));


        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetDisplayErrorThumbnailOnError(Image obj)
        {
            return (bool)obj.GetValue(DisplayErrorThumbnailOnErrorProperty);
        }

        public static void SetDisplayErrorThumbnailOnError(Image obj, bool value)
        {
            obj.SetValue(DisplayErrorThumbnailOnErrorProperty, value);
        }
        public static readonly DependencyProperty DisplayErrorThumbnailOnErrorProperty =
            DependencyProperty.RegisterAttached("DisplayErrorThumbnailOnError", typeof(bool), typeof(PhotoLoader), new UIPropertyMetadata(true));


        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static DisplayOptions GetDisplayOption(Image obj)
        {
            return (DisplayOptions)obj.GetValue(DisplayOptionProperty);
        }

        public static void SetDisplayOption(Image obj, DisplayOptions value)
        {
            obj.SetValue(DisplayOptionProperty, value);
        }
        public static readonly DependencyProperty DisplayOptionProperty =
            DependencyProperty.RegisterAttached("DisplayOption", typeof(DisplayOptions), typeof(PhotoLoader), new UIPropertyMetadata(DisplayOptions.FullResolution));


        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetIsLoading(Image obj)
        {
            return (bool)obj.GetValue(IsLoadingProperty);
        }

        internal static void SetIsLoading(Image obj, bool value)
        {
            obj.SetValue(IsLoadingProperty, value);
        }
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.RegisterAttached("IsLoading", typeof(bool), typeof(PhotoLoader), new UIPropertyMetadata(true));


        [AttachedPropertyBrowsableForType(typeof(Image))]
        public static bool GetErrorDetected(Image obj)
        {
            return (bool)obj.GetValue(ErrorDetectedProperty);
        }

        internal static void SetErrorDetected(Image obj, bool value)
        {
            obj.SetValue(ErrorDetectedProperty, value);
        }

        public static readonly DependencyProperty ErrorDetectedProperty =
            DependencyProperty.RegisterAttached("ErrorDetected", typeof(bool), typeof(PhotoLoader), new UIPropertyMetadata(false));

    }
}
