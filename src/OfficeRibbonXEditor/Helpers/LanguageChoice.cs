using System.Collections.Generic;

namespace OfficeRibbonXEditor.Helpers
{
    public class LanguageChoice
    {
        public static ICollection<LanguageChoice> All { get; } = new[]
        {
            new LanguageChoice("English", string.Empty),
            new LanguageChoice("Español", "es"),
        };

        public LanguageChoice(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }

        public string Id { get; }
    }
}
