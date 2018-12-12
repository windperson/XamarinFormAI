using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.AI.MachineLearning;
namespace UseOfflineAI.UWP
{
    
    public sealed class my_cat_dogInput
    {
        public ImageFeatureValue data; // BitmapPixelFormat: Bgra8, BitmapAlphaMode: Premultiplied, width: 227, height: 227
    }
    
    public sealed class my_cat_dogOutput
    {
        public TensorString classLabel; // shape(-1,1)
        public IList<Dictionary<string,float>> loss;
    }
    
    public sealed class my_cat_dogModel
    {
        private LearningModel model;
        private LearningModelSession session;
        private LearningModelBinding binding;
        public static async Task<my_cat_dogModel> CreateFromStreamAsync(IRandomAccessStreamReference stream)
        {
            my_cat_dogModel learningModel = new my_cat_dogModel();
            learningModel.model = await LearningModel.LoadFromStreamAsync(stream);
            learningModel.session = new LearningModelSession(learningModel.model);
            learningModel.binding = new LearningModelBinding(learningModel.session);
            return learningModel;
        }
        public async Task<my_cat_dogOutput> EvaluateAsync(my_cat_dogInput input)
        {
            binding.Bind("data", input.data);
            var result = await session.EvaluateAsync(binding, "0");
            var output = new my_cat_dogOutput();
            output.classLabel = result.Outputs["classLabel"] as TensorString;
            output.loss = result.Outputs["loss"] as IList<Dictionary<string,float>>;
            return output;
        }
    }
}
