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
using UseOfflineAI.DependencyServices;
using UseOfflineAI.UWP.DependencyServices;
using Xamarin.Forms;

[assembly:Dependency(typeof(Uwp1809CatDogRecognizer))]
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
            var labels = results.classLabel;

            var ret = new List<(string Tag, double Probability)>();
            foreach (Dictionary<string, float> item in loss)
            {
                foreach (var keyValuePair in item)
                {
                    ret.Add((Tag:keyValuePair.Key, Probability:keyValuePair.Value));
                }
            }

            return ret;
        }

        private static async Task<ImageFeatureValue> CreateInputData(Stream stream)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
            var sfbmp = await decoder.GetSoftwareBitmapAsync();
            var videoFrame = await ProcessVideoFrame(VideoFrame.CreateWithSoftwareBitmap(sfbmp));
            return ImageFeatureValue.CreateFromVideoFrame(videoFrame);
        }

        private static async Task<VideoFrame> ProcessVideoFrame(VideoFrame inputVideoFrame)
        {
            bool useDX = inputVideoFrame.SoftwareBitmap == null;

            BitmapBounds cropBounds = new BitmapBounds();

            var frameHeight = useDX ? inputVideoFrame.Direct3DSurface.Description.Height : inputVideoFrame.SoftwareBitmap.PixelHeight;
            var frameWidth = useDX ? inputVideoFrame.Direct3DSurface.Description.Width : inputVideoFrame.SoftwareBitmap.PixelWidth;

            var requiredAR = ((float)227 / 227);
            uint w = Math.Min((uint)(requiredAR * frameHeight), (uint)frameWidth);
            uint h = Math.Min((uint)(frameWidth / requiredAR), (uint)frameHeight);

            cropBounds.X = (uint)((frameWidth - w) / 2);
            cropBounds.Y = 0;

            cropBounds.Width = w;
            cropBounds.Height = h;

            var ret = new VideoFrame(BitmapPixelFormat.Bgra8, 227, 227, BitmapAlphaMode.Ignore);

            await inputVideoFrame.CopyToAsync(ret, cropBounds, null);

            return ret;
        }

    }
}