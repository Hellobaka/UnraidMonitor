﻿using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace me.cqp.luohuaming.UnraidMonitor.UI.Controls
{
    /// <summary>
    /// NonClientAreaContent.xaml 的交互逻辑
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class NonClientAreaContent : UserControl
    {
        public NonClientAreaContent()
        {
            InitializeComponent();
        }
    }
}
