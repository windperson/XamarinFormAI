using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Util;
using Org.Tensorflow;
using Org.Tensorflow.Contrib.Android;
using UseOfflineAI.DependencyServices;
using UseOfflineAI.Droid.DependencyServices;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidCatDogRecognizer))]

namespace UseOfflineAI.Droid.DependencyServices
{
    public class AndroidCatDogRecognizer : IRecognize
    {
        private const int InputSize = 227;
        private const string InputName = "Placeholder";
        private const string OutputName = "loss";
        private const string DataNormLayerPrefix = "data_bn";

        private readonly List<string> _labels;
        private readonly TensorFlowInferenceInterface _inferenceInterface;
        private readonly bool _hasNormalizationLayer;

        public AndroidCatDogRecognizer()
        {
            try
            {
                var assets = Android.App.Application.Context.Assets;
                _inferenceInterface = new TensorFlowInferenceInterface(assets, "model.pb");

                _hasNormalizationLayer = CheckIfNormalizeLayerExist(_inferenceInterface);

                using (var streamReader = new StreamReader(assets.Open("labels.txt")))
                {
                    var contentTxt = streamReader.ReadToEnd();
                    _labels = contentTxt
                        .Split("\n")
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrEmpty(line))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error("AI", $"Load TensorFlow model error={ex}");
                throw;
            }

        }

        private static bool CheckIfNormalizeLayerExist(TensorFlowInferenceInterface tensorFlowInferenceInterface)
        {
            var operations = tensorFlowInferenceInterface.Graph().Operations();
            while (operations.HasNext)
            {
                var op = (Operation) operations.Next();
                if (op.Name().Contains(DataNormLayerPrefix))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<IList<(string Tag, double Probability)>> Recognize(Stream stream)
        {
            try
            {
                using (var bitmap = await BitmapFactory.DecodeStreamAsync(stream))
                {
                    var retVal = await Task.Run(() => RecognizeImage(bitmap).AsReadOnly());
                    bitmap.Recycle();
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                Log.Error("AI", $"Tensor flow model recognize error={ex}");
                throw;
            }
        }

        private List<(string, double)> RecognizeImage(Bitmap bitmap)
        {
            var outputNames = new[] {OutputName};
            var floatValues = bitmap.GetBitmapPixels(InputSize, InputSize, ModelType.General, _hasNormalizationLayer);
            var outputs = new float[_labels.Count];

            _inferenceInterface.Feed(InputName, floatValues, 1, InputSize, InputSize, 3);
            _inferenceInterface.Run(outputNames);
            _inferenceInterface.Fetch(OutputName, outputs);

            var results = new List<(string Tag, double Probability)>();
            for (var i = 0; i < outputs.Length; i++)
            {
                results.Add((Tag: _labels[i], Probability: outputs[i]));
            }

            return results;
        }
    }
}