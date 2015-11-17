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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BuildFeedApp
{
   public sealed partial class DetailsRow : UserControl
   {
      public DetailsRow()
      {
         this.InitializeComponent();
      }

      public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(DetailsRow), new PropertyMetadata("TEST:"));
      public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(DetailsRow), new PropertyMetadata(""));

      public string Label
      {
         get { return (string)GetValue(LabelProperty); }
         set { SetValue(LabelProperty, value); }
      }

      public string Value
      {
         get { return (string)GetValue(ValueProperty); }
         set { SetValue(ValueProperty, value); }
      }
   }
}
