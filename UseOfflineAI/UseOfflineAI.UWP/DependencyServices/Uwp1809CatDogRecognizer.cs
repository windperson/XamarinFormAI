using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UseOfflineAI.DependencyServices;

namespace UseOfflineAI.UWP.DependencyServices
{
    public class Uwp1809CatDogRecognizer : IRecognize
    {
        public Task<IList<(string Tag, double Probability)>> Recognize(Stream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}