using System.Threading;
using System.Threading.Tasks;

namespace OfficeRibbonXEditor.Interfaces
{
    /**
     * Service which will check online whether a newer version of the tool exists
     */
    public interface IVersionChecker
    {
        Task<string?> CheckVersionAsync(CancellationToken cancelToken = default(CancellationToken));
    }
}
