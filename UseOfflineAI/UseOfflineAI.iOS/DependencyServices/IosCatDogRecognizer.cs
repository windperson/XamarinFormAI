using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using CoreML;
using Foundation;
using UseOfflineAI.DependencyServices;
using UseOfflineAI.iOS.DependencyServices;
using Vision;
using Xamarin.Forms;

[assembly: Dependency(typeof(IosCatDogRecognizer))]
namespace UseOfflineAI.iOS.DependencyServices
{
    public class IosCatDogRecognizer : IRecognize
    {
        private const string CoreMLCompiledModelFileExt = @"mlmodelc";
        private const string CoreMLModelFileExt = @"mlmodel";

        private static readonly CGSize _targetImageSize = new CGSize(227, 227);
        private VNCoreMLModel _model;

        public IosCatDogRecognizer()
        {
            _model = LoadModel("my_cat_dog");
        }

        public async Task<IList<(string Tag, double Probability)>> Recognize(Stream stream)
        {
            var tcs = new TaskCompletionSource<IList<(string Tag, double Probability)>>();

            var request = new VNCoreMLRequest(_model, (response, e) =>
            {
                if (e != null)
                    tcs.SetException(new NSErrorException(e));
                else
                {
                    var results = response.GetResults<VNClassificationObservation>();

                    var ret = new List<(string Tag, double Probability)>();
                    foreach (var observation in results)
                    {
                        ret.Add((Tag: observation.Identifier, Probability: observation.Confidence));
                    }
                    
                    tcs.SetResult(ret);
                }
            });

            var imageSource = await stream.ToUIImage();
            var buffer = imageSource.ToCVPixelBuffer(_targetImageSize);
            var requestHandler = new VNImageRequestHandler(buffer, new NSDictionary());

            requestHandler.Perform(new[] {request}, out NSError error);

            var classifications = await tcs.Task;

            if (error != null)
            {
                Console.WriteLine($"CoreML recognize data error={error}");
                throw new NSErrorException(error);
            }

            return classifications.OrderByDescending(p => p.Probability).ToList().AsReadOnly();
        }

        private static VNCoreMLModel LoadModel(string modelName)
        {
            var modelPath = NSBundle.MainBundle.GetUrlForResource(modelName, CoreMLCompiledModelFileExt) ?? CompileModel(modelName);

            if (modelPath == null)
            {
                var errorMsg = $"CoreML Model {modelName} does not exist";
                Console.WriteLine(errorMsg);
                throw new Exception(errorMsg);
            }             

            var mlModel = MLModel.Create(modelPath, out NSError err);
            if (err != null)
            {
                Console.WriteLine($"CoreML model loading from {modelPath} error={err}");
                throw new NSErrorException(err);
            }                

            var model = VNCoreMLModel.FromMLModel(mlModel, out err);

            if (err != null)
            {
                Console.WriteLine($"CoreML load model error={err}");
                throw new NSErrorException(err);
            }

            return model;
        }

        private static NSUrl CompileModel(string modelName)
        {
            var unCompiled = NSBundle.MainBundle.GetUrlForResource(modelName, CoreMLModelFileExt);
            var modelPath = MLModel.CompileModel(unCompiled, out NSError err);

            if (err != null)
            {
                Console.WriteLine($"CoreML compile model error={err}");
                throw new NSErrorException(err);
            }
                

            return modelPath;
        }
    }
}