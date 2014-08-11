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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HybridWebApp.Toolkit.WP8.Views
{
    public partial class OfflineOverlay : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _LargeFontSize;

        public int LargeFontSize
        {
            get { return _LargeFontSize; }
            set
            {
                if (_LargeFontSize == value)
                {
                    return;
                }

                _LargeFontSize = value;
                _OnPropertyChanged();
            }
        }

        private int _SmallFontSize;

        public int SmallFontSize
        {
            get { return _SmallFontSize; }
            set
            {
                if (_SmallFontSize == value)
                {
                    return;
                }

                _SmallFontSize = value;
                _OnPropertyChanged();
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
                _OnPropertyChanged();
            }
        }

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
                _OnPropertyChanged();
            }
        }

        private string _BackgroundImage;

        public string BackgroundImage
        {
            get { return _BackgroundImage; }
            set
            {
                if (_BackgroundImage == value)
                {
                    return;
                }

                _BackgroundImage = value;
                _OnPropertyChanged();
            }
        }

        private double _BackgroundImageWidth;

        public double BackgroundImageWidth
        {
            get { return _BackgroundImageWidth; }
            set
            {
                if (_BackgroundImageWidth == value)
                {
                    return;
                }

                _BackgroundImageWidth = value;
                _OnPropertyChanged();
            }
        }

        public Action RetryAction { get; set; }

        public OfflineOverlay()
        {
            InitializeComponent();

            this.ForegroundBrush = new SolidColorBrush(Colors.Black);
            this.BackgroundBrush = new SolidColorBrush(Colors.White);
            this.BackgroundImageWidth = 336;
            this.LargeFontSize = 16;
            this.SmallFontSize = 12;

            this.DataContext = this;
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            this.RetryAction();
        }

        private void _OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
