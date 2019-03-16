// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IVersionChecker.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the IVersionChecker type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Services
{
    using System.Threading;
    using System.Threading.Tasks;

    /**
     * Service which will check online whether a newer version of the tool exists
     */
    public interface IVersionChecker
    {
        Task<string> CheckVersionAsync(CancellationToken cancelToken = default(CancellationToken));
    }
}
