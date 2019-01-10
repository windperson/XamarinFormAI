using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using UseOfflineAI.DependencyServices;
using UseOfflineAI.UWP.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(Uwp1809CatDogRecognizer))]
namespace UseOfflineAI.UWP.DependencyServices
{
    public class Uwp1809CatDogRecognizer : IRecognize
    {
        public async Task<IList<(string Tag, double Probability)>> Recognize(Stream stream)
        {
            var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/my_cat_dog.onnx"));
            var model = await my_cat_dogModel.CreateFromStreamAsync(modelFile);

            var inputData = new my_cat_dogInput
            {
                data = await CreateInputData(stream)
            };

            var results = await model.EvaluateAsync(inputData);
            var loss = results.loss;


            var ret = new List<(string Tag, double Probability)>();

            foreach (var item in loss)
            {
                foreach (var keyValuePair in item)
                {
                    ret.Add((Tag: keyValuePair.Key, Probability: keyValuePair.Value));
                }
            }

            return ret;
        }

        private static async Task<ImageFeatureValue> CreateInputData(Stream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var videoFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            return ImageFeatureValue.CreateFromVideoFrame(videoFrame);
        }
    }
}