namespace Generators.Extensions
{
    public static class StringExtensions
    {
        public static string StripStart(this string self, string prefix)
        {
            if (!self.StartsWith(prefix))
            {
                return self;
            }

            return self.Substring(prefix.Length);
        }

        public static string StripEnd(this string self, string suffix)
        {
            if (!self.EndsWith(suffix))
            {
                return self;
            }

            return self.Substring(0, self.Length - suffix.Length);
        }

        public static string ToFieldName(this string propertyName)
        {
            return $"_{propertyName.Substring(0, 1).ToLower()}{propertyName.Substring(1)}";
        }
    }
}
