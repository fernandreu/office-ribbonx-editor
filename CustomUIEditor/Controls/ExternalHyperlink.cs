// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalHyperlink.cs" company="FA">
//   Fernando Andreu
// </copyright>
// <summary>
//   Defines the ExternalHyperlink type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CustomUIEditor.Controls
{
    using System.Diagnostics;
    using System.Windows.Documents;
    using System.Windows.Navigation;

    /// <summary>
    /// Hyperlink which automatically takes care of opening its Uri in an external browser
    /// </summary>
    public class ExternalHyperlink : Hyperlink
    {
        public ExternalHyperlink()
        {
            this.RequestNavigate += OnRequestNavigate;
        }

        private static void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
