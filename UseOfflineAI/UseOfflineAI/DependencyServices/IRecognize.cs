using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UseOfflineAI.DependencyServices
{
    public interface IRecognize
    {
        Task<IList<(string Tag, double Probability)>> Recognize(System.IO.Stream stream);
    }
}