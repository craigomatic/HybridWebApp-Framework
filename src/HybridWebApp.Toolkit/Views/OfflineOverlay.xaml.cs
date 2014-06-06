using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HybridWebApp.Toolkit.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OfflineOverlay : Page
    {
        public int LargeFontSize
        {
            get { return (int)GetValue(LargeFontSizeProperty); }
            set { SetValue(LargeFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeFontSizeProperty =
            DependencyProperty.Register("LargeFontSize", typeof(int), typeof(OfflineOverlay), new PropertyMetadata(16));

        public int SmallFontSize
        {
            get { return (int)GetValue(SmallFontSizeProperty); }
            set { SetValue(SmallFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmallFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmallFontSizeProperty =
            DependencyProperty.Register("SmallFontSize", typeof(int), typeof(OfflineOverlay), new PropertyMetadata(12));

        public Brush ForegroundBrush
        {
            get { return (Brush)GetValue(ForegroundBrushProperty); }
            set { SetValue(ForegroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForegroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForegroundBrushProperty =
            DependencyProperty.Register("ForegroundBrush", typeof(Brush), typeof(OfflineOverlay), new PropertyMetadata(new SolidColorBrush(Windows.UI.Colors.Black)));

        public Brush BackgroundBrush
        {
            get { return (Brush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LoadingBackgroundBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(Brush), typeof(OfflineOverlay), new PropertyMetadata(new SolidColorBrush(Windows.UI.Colors.White)));

        public ImageSource BackgroundImage
        {
            get { return (ImageSource)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LoadingImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundImageProperty =
            DependencyProperty.Register("BackgroundImage", typeof(ImageSource), typeof(OfflineOverlay), new PropertyMetadata(null));

        public double BackgroundImageWidth
        {
            get { return (double)GetValue(BackgroundImageWidthProperty); }
            set { SetValue(BackgroundImageWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundImageWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundImageWidthProperty =
            DependencyProperty.Register("BackgroundImageWidth", typeof(double), typeof(OfflineOverlay), new PropertyMetadata(336));
        public Action RetryAction { get; set; }

        public OfflineOverlay()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            this.RetryAction();
        }
    }
}
