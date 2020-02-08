using NUnit.Framework;
using OfficeRibbonXEditor.Helpers;

namespace OfficeRibbonXEditor.UnitTests.Models
{
    public class StringUtilsTests
    {
        [Test]
        [TestCase(@"C:\file.txt", 5, ExpectedResult = @"C:\fil...")]
        [TestCase(@"C:\file.txt", 100, ExpectedResult = @"C:\file.txt")]
        [TestCase(@"C:\A\Short\Path.txt", 100, ExpectedResult = @"C:\A\Short\Path.txt")]
        [TestCase(@"\\ReallyLongServerName\ReallyLongShareName\file.txt", 10, ExpectedResult = @"\\ReallyLongServerName\ReallyLongShareName\fil...")]
        [TestCase(@"\\ReallyLongServerName\ReallyLongShareName\file.txt", 100, ExpectedResult = @"\\ReallyLongServerName\ReallyLongShareName\file.txt")]
        [TestCase(@"C:\ReallyLongSingleFolderName\file.txt", 10, ExpectedResult = @"C:\...\fil...")]
        [TestCase(@"C:\ReallyLongSingleFolderName\file.txt", 20, ExpectedResult = @"C:\...\file.txt")]
        [TestCase(@"C:\ReallyLongFolder1\ReallyLongFolder2\ReallyLongFolder3\file.txt", 20, ExpectedResult = @"C:\...\file.txt")]
        [TestCase(@"C:\ReallyLongFolder1\ReallyLongFolder2\ReallyLongFolder3\file.txt", 40, ExpectedResult = @"C:\...\ReallyLongFolder3\file.txt")]
        public string ShortenPathNameTest(string pathName, int maxLength)
        {
            return StringUtils.ShortenPathName(pathName, maxLength);
        }
    }
}
