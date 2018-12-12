using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xam.Plugins.OnDeviceCustomVision;
using Xamarin.Forms;

namespace UseOfflineAI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

            if (cameraStatus != PermissionStatus.Granted || storageStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera, Permission.Storage });
                cameraStatus = results[Permission.Camera];
                storageStatus = results[Permission.Storage];
            }

            if (cameraStatus == PermissionStatus.Granted && storageStatus == PermissionStatus.Granted)
            {
                //Take a photo

                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    Directory = "my_images",
                    Name = "test.jpg"
                });

                if (file == null)
                {
                    await DisplayAlert("Photo not took", "User cancelled", "OK");
                    return;
                }

                LabelStatus.Text = "loading AI...";

                string result = await GetOfflineAiDecision(file);

                await DisplayAlert("Offline AI", result, "OK");

                LabelStatus.Text = "Click to take picture";
            }
            else
            {
                await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                //On iOS you may want to send your user to the settings screen.
                //CrossPermissions.Current.OpenAppSettings();
            }
        }

        private async Task<string> GetOfflineAiDecision(MediaFile file)
        {
            var model = CrossImageClassifier.Current;
            if (model == null)
            {
                await DisplayAlert("error", "Cannot load offline model", "abort");
                return "Cannot load offline model";
            }

            var tags = await model.ClassifyImage(file.GetStream());

            var result = new List<string>();
            foreach (var tag in tags.OrderByDescending(t => t.Probability))
            {
                result.Add($"{tag.Tag}: {tag.Probability}");
            }
            return result.Aggregate((s1, s2) => $"{s1},\n{s2}");
        }
    }
}
