using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CallOnlineAI
{
    public partial class MainPage : ContentPage
    {
        private static string AiWebApiUrl = @"https://southcentralus.api.cognitive.microsoft.com/customvision/v2.0/Prediction/1650fdcb-a666-47c1-9f4d-4537ed96a165/image";
        private static System.Net.Http.Headers.MediaTypeHeaderValue AiUrlContenttype = new System.Net.Http.Headers.MediaTypeHeaderValue(@"application/octet-stream");
        private static string AiWebApiPredictionKey = @"d6181300f9054ff989e1baf3d9352418";

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
                    Directory = "Sample",
                    Name = "test.jpg"
                });

                if (file == null)
                {
                    await DisplayAlert("Photo not took","User cancelled", "OK");
                    return;
                }
                LabelStatus.Text = "Call online AI WebAPI....";
                string result = await GetWebAiDecision(file);

                await DisplayAlert("Ai Web API", result, "OK");
                LabelStatus.Text = "Click to take picture";

            }
            else
            {
                await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                //On iOS you may want to send your user to the settings screen.
                //CrossPermissions.Current.OpenAppSettings();
            }
        }

        private async Task<string> GetWebAiDecision(MediaFile file)
        {
            var fileContent = new StreamContent(file.GetStream());
            fileContent.Headers.ContentType = AiUrlContenttype;
            fileContent.Headers.Add("Prediction-Key", AiWebApiPredictionKey);

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(AiWebApiUrl, fileContent);

            return await response.Content.ReadAsStringAsync();
                       
        }      
    }
}
