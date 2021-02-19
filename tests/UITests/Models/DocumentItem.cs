using System.Collections.Generic;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using OfficeRibbonXEditor.UITests.Extensions;

namespace OfficeRibbonXEditor.UITests.Models
{
    public class DocumentItem : TreeItem
    {
        public DocumentItem(FrameworkAutomationElementBase frameworkAutomationElement) 
            : base(frameworkAutomationElement)
        {
        }

        public string? Title => FindAllChildren().Last().AsLabel().Text;

        public new List<DocumentItem> Items
        {
            get
            {
                // This could all be turned into a one-line LINQ expression, but it shows a warning due to the null values (even if you discard them)
                // Monitor any evolution in nullable reference types processing to see if it eventually becomes smart enough with LINQ expressions
                var result = new List<DocumentItem>();
                foreach (var item in base.Items)
                {
                    var converted = item.AsDocumentItem();
                    if (converted != null)
                    {
                        result.Add(converted);
                    }
                }

                return result;
            }
        }
    }
}
