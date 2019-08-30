using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UseOfflineAI.DependencyServices;
using Xamarin.Forms;

namespace UseOfflineAI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public ImageSource DisplaySource
        {
            get => PhotoImage.Source;
            set
            {
                PhotoImage.Source = value;
                OnPropertyChanged(nameof(PhotoImage));
            }
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

                MediaFile file;
                try
                {
                    file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        Directory = "my_images",
                        Name = "test.jpg"
                    });
                }
                catch (NotSupportedException)
                {
                    await DisplayAlert("Photo not took", "This device has no camera", "OK");
                    return;
                }

                if (file == null)
                {
                    await DisplayAlert("Photo not took", "User cancelled", "OK");
                    return;
                }
                TakePhotoButton.IsEnabled = false;
                DisplaySource = ImageSource.FromStream(file.GetStream);
                LabelStatus.Text = "loading AI...";

                var result = await AiIdentify(file);

                LabelStatus.Text = $"Result:\n{result}";
                TakePhotoButton.IsEnabled = true;
            }
            else
            {
                await DisplayAlert("Permissions Denied", "Unable to take photos.", "OK");
                //On iOS you may want to send your user to the settings screen.
                //CrossPermissions.Current.OpenAppSettings();
            }
        }

        private async Task<string> AiIdentify(MediaFile file)
        {
            var result = string.Empty;
            try
            {
                result = await Task.Run(async () => await GetOfflineAiDecision(file));
            }
            catch (Exception ex)
            {
                await DisplayAlert("AI error", ex.Message, "Abort");
            }

            await DisplayAlert("Offline AI got", result, "OK");
            return result;
        }

        private async Task<string> GetOfflineAiDecision(MediaFile file)
        {


            var recognizer = DependencyService.Get<IRecognize>();
            if (recognizer == null)
            {
                throw new Exception("No Implement Vision AI service");
            }

            var tags = await recognizer.Recognize(file.GetStream());
            
            var result = new List<string>();
            foreach (var tag in tags.OrderByDescending(t => t.Probability))
            {
                result.Add($"{tag.Tag}: {tag.Probability:P2}");
            }
            return result.Aggregate((s1, s2) => $"{s1},\n{s2}");
        }
    }
}
