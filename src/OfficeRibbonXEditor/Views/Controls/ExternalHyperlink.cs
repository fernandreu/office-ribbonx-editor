using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Navigation;
using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.Views.Controls
{
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
            UrlUtils.OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
