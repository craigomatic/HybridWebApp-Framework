﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace HybridWebApp.Toolkit.WP8.Views
{
    public partial class OfflineOverlay : UserControl
    {
        public Action RetryAction { get; set; }

        public OfflineOverlay()
        {
            InitializeComponent();
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            this.RetryAction();
        }
    }
}