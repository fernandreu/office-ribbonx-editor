using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[assembly: SuppressMessage("SonarLint", "S3267", Justification = "This warning is about 'simplifying' foreach loops with LINQ expressions. More often than not, the alternative is less readable, or there's no alternative at all")]

[assembly: InternalsVisibleTo("UnitTests")]
[assembly: InternalsVisibleTo("IntegrationTests")]
[assembly: InternalsVisibleTo("FunctionalTests")]
[assembly: InternalsVisibleTo("UITests")]
