using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace HybridWebApp.Toolkit.WP8.Views
{
    public partial class LoadingOverlay : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Brush _BackgroundBrush;

        public Brush BackgroundBrush
        {
            get { return _BackgroundBrush; }
            set
            {
                if (_BackgroundBrush == value)
                {
                    return;
                }

                _BackgroundBrush = value;
                _OnPropertyChanged("BackgroundBrush");
            }
        }

        private Brush _ForegroundBrush;

        public Brush ForegroundBrush
        {
            get { return _ForegroundBrush; }
            set
            {
                if (_ForegroundBrush == value)
                {
                    return;
                }

                _ForegroundBrush = value;
                _OnPropertyChanged("ForegroundBrush");
            }
        }

        private ImageSource _BackgroundImage;

        public ImageSource BackgroundImage
        {
            get { return _BackgroundImage; }
            set
            {
                if (_BackgroundImage == value)
                {
                    return;
                }

                _BackgroundImage = value;
                _OnPropertyChanged("BackgroundImage");
            }
        }

        private double _BackgroundImageWidth;

        public double BackgroundImageWidth
        {
            get { return _BackgroundImageWidth; }
            set
            {
                if(_BackgroundImageWidth == value)
                {
                    return;
                }

                _BackgroundImageWidth = value;
                _OnPropertyChanged("BackgroundImageWidth");
            }
        }

        public LoadingOverlay()
        {
            InitializeComponent();

            this.ForegroundBrush = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            this.BackgroundBrush = Application.Current.Resources["PhoneBackgroundBrush"] as SolidColorBrush;
            this.BackgroundImageWidth = 336;
            
            this.DataContext = this;
        }

        private void _OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
