using OfficeRibbonXEditor.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace OfficeRibbonXEditor.Interfaces
{
    public interface IVersionChecker
    {
        Task<string?> CheckToolVersionAsync(CancellationToken cancelToken = default);

        RedistributableDetails CheckRedistributableVersion();
    }
}
