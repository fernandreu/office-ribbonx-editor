using NUnit.Framework;
using OfficeRibbonXEditor.Properties;
using OfficeRibbonXEditor.ViewModels.Tabs;

namespace OfficeRibbonXEditor.UnitTests.Models
{
    /// <summary>
    /// Testing of several static methods scattered throughout different classes
    /// </summary>
    public class StaticTests
    {
        [Test]
        public void CanResetIconGrid()
        {
            // Arrange
            Settings.Default.IconGridSize = -16; // An unrealistic value that should not be set manually
            Settings.Default.Save();

            // Act
            IconTabViewModel.ResetGridSettings();

            // Assert
            Assert.AreNotEqual(-16, Settings.Default.IconGridSize);
        }
    }
}
