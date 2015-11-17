using BuildFeedApp.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BuildFeedApp
{
   /// <summary>
   /// An empty page that can be used on its own or navigated to within a Frame.
   /// </summary>
   public sealed partial class MainPage : Page
   {
      public int BuildModel { get; private set; }

      public MainPage()
      {
         this.InitializeComponent();
      }

      private void Page_Loaded(object sender, RoutedEventArgs e)
      {
         var item = new IncrementalBuildGroups();
         lvGroups.ItemsSource = item;
      }

      private async void lvGroups_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         FrontBuildGroup fbg = lvGroups.SelectedValue as FrontBuildGroup;
         lvBuilds.ItemsSource = await ApiCache.GetApi<Build[]>($"https://buildfeed.net/api/GetBuildsForBuildGroup?major={fbg.Key.Major}&minor={fbg.Key.Minor}&number={fbg.Key.Build}&revision={fbg.Key.Revision}");
      }

      private void lvBuilds_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         Build b = lvBuilds.SelectedValue as Build;
         spDetails.DataContext = b;
      }
   }
}
