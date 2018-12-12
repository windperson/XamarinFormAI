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
using Xam.Plugins.OnDeviceCustomVision;

namespace UseOfflineAI.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new UseOfflineAI.App());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await WindowsImageClassifier.Init("my_cat_dog", new[] { "cat", "dog", "Negative" });

            base.OnNavigatedTo(e);
        }
    }
}
