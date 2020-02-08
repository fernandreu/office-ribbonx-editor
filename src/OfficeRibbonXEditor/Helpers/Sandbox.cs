using System;

namespace OfficeRibbonXEditor.Helpers
{
    /// <summary>
    /// Simple helper class that the application will check to alter some of its actions in
    /// certain way. This is done to ensure there are no undesired side effects when testing.
    /// Ideally, this class should not be needed because 100% of the code should be testable.
    /// However, it does simplify the testing in some occasions (e.g. navigating to help pages).
    /// </summary>
    public static class Sandbox
    {
        private const string VariableName = "OfficeRibbonXEditor:SandboxMode";

        public static bool IsEnabled
        {
            get => Environment.GetEnvironmentVariable(VariableName) == "1"; 
            set => Environment.SetEnvironmentVariable(VariableName, value ? "1" : null);
        }
    }
}
