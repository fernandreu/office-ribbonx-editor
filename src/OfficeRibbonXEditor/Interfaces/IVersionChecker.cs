using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.Interfaces;

public interface IVersionChecker
{
    Task<string?> CheckToolVersionAsync(CancellationToken cancelToken = default);

    RedistributableDetails CheckRedistributableVersion();
}