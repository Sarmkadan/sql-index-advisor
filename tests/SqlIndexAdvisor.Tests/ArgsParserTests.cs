using System.Text;
using SqlIndexAdvisor.Core.ArgsParsing;
using SqlIndexAdvisor.Core.Model;
using Xunit;

namespace SqlIndexAdvisor.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ArgsParser"/> class.
/// </summary>
public class ArgsParserTests
{
    /// <summary>
    /// Tests that parsing an empty argument list returns a help request.
    /// </summary>
    [Fact]
    public void Parse_EmptyArgs_ReturnsHelpRequest()
    {
        // Act
        var result = ArgsParser.Parse(Array.Empty<string>());

        // Assert
        Assert.True(result.ShouldShowHelp);
        Assert.NotNull(result.HelpMessage);
        Assert.Contains(ArgsParserTestsConstants.ToolName, result.HelpMessage);
    }

    /// <summary>
    /// Tests that parsing the --help flag returns a help request.
    /// </summary>
    [Fact]
    public void Parse_HelpFlag_ReturnsHelpRequest()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.HelpFlag });

        // Assert
        Assert.True(result.ShouldShowHelp);
        Assert.NotNull(result.HelpMessage);
        Assert.Contains(ArgsParserTestsConstants.ToolName, result.HelpMessage);
    }

    /// <summary>
    /// Tests that parsing the -h flag returns a help request.
    /// </summary>
    [Fact]
    public void Parse_HelpShortFlag_ReturnsHelpRequest()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.HelpShortFlag });

        // Assert
        Assert.True(result.ShouldShowHelp);
        Assert.NotNull(result.HelpMessage);
        Assert.Contains(ArgsParserTestsConstants.ToolName, result.HelpMessage);
    }

    /// <summary>
    /// Tests that parsing an unknown option throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_UnknownOption_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.UnknownOptionFlag }));
        Assert.Contains(ArgsParserTestsConstants.UnknownOptionMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing an unknown short option throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_UnknownShortOption_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.UnknownShortOptionFlag }));
        Assert.Contains(ArgsParserTestsConstants.UnknownOptionMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing --format without a value throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_MissingFormatValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.FormatFlag }));
        Assert.Contains(ArgsParserTestsConstants.MissingFormatValueMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing --format with an invalid value throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_InvalidFormatValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.InvalidFormat }));
        Assert.Contains(ArgsParserTestsConstants.InvalidFormatMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing --format text returns the correct format.
    /// </summary>
    [Fact]
    public void Parse_ValidTextFormat_ReturnsCorrectFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.TextFormat });

        // Assert
        Assert.False(result.ShouldShowHelp);
        Assert.Equal(ArgsParserTestsConstants.TextFormat, result.Format);
        Assert.Null(result.Path);
        Assert.False(result.UseStdin);
        Assert.Equal(ArgsParserTestsConstants.DefaultMinImpact, result.MinImpact);
    }

    /// <summary>
    /// Tests that parsing --format json returns the correct format.
    /// </summary>
    [Fact]
    public void Parse_ValidJsonFormat_ReturnsCorrectFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.JsonFormat });

        // Assert
        Assert.False(result.ShouldShowHelp);
        Assert.Equal(ArgsParserTestsConstants.JsonFormat, result.Format);
    }

    /// <summary>
    /// Tests that parsing --format html returns the correct format.
    /// </summary>
    [Fact]
    public void Parse_ValidHtmlFormat_ReturnsCorrectFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.HtmlFormat });

        // Assert
        Assert.False(result.ShouldShowHelp);
        Assert.Equal(ArgsParserTestsConstants.HtmlFormat, result.Format);
    }

    /// <summary>
    /// Tests that parsing --format csv returns the correct format.
    /// </summary>
    [Fact]
    public void Parse_ValidCsvFormat_ReturnsCorrectFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.CsvFormat });

        // Assert
        Assert.False(result.ShouldShowHelp);
        Assert.Equal(ArgsParserTestsConstants.CsvFormat, result.Format);
    }

    /// <summary>
    /// Tests that parsing --format with case-insensitive value returns the lowercased format.
    /// </summary>
    [Fact]
    public void Parse_FormatIsCaseInsensitive_ReturnsLowercasedFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.UpperCaseJsonFormat });

        // Assert
        Assert.Equal(ArgsParserTestsConstants.JsonFormat, result.Format);
    }

    /// <summary>
    /// Tests that parsing --min-impact without a value throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_MissingMinImpactValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.MinImpactFlag }));
        Assert.Contains(ArgsParserTestsConstants.MissingMinImpactValueMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing --min-impact with a non-numeric value throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_InvalidMinImpactValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.MinImpactFlag, ArgsParserTestsConstants.NonNumericMinImpact }));
        Assert.Contains(ArgsParserTestsConstants.InvalidMinImpactMessage, ex.Message);
    }

    /// <summary>
    /// Tests that --min-impact values outside the 0..100 percent range are rejected.
    /// </summary>
    [Theory]
    [InlineData("-1")]
    [InlineData("100.01")]
    [InlineData("1000")]
    public void Parse_MinImpactOutOfRange_ThrowsArgumentException(string value)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.MinImpactFlag, value }));
        Assert.Contains(ArgsParserTestsConstants.MinImpactRangeMessage, ex.Message);
    }

    /// <summary>
    /// Tests that non-finite --min-impact values are rejected even though double.TryParse accepts them.
    /// </summary>
    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    public void Parse_MinImpactNonFinite_ThrowsArgumentException(string value)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.MinImpactFlag, value }));
        Assert.Contains(ArgsParserTestsConstants.MinImpactRangeMessage, ex.Message);
    }

    /// <summary>
    /// Tests that the --min-impact range bounds 0 and 100 are accepted.
    /// </summary>
    [Theory]
    [InlineData("0", 0)]
    [InlineData("100", 100)]
    public void Parse_MinImpactAtBounds_ReturnsValue(string value, double expected)
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.MinImpactFlag, value });

        // Assert
        Assert.Equal(expected, result.MinImpact);
    }

    /// <summary>
    /// Tests that parsing --min-impact with a valid number returns the correct min impact.
    /// </summary>
    [Fact]
    public void Parse_ValidMinImpactValue_ReturnsCorrectMinImpact()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.MinImpactFlag, ArgsParserTestsConstants.ValidMinImpactArg });

        // Assert
        Assert.Equal(ArgsParserTestsConstants.ValidMinImpact, result.MinImpact);
    }

    /// <summary>
    /// Tests that parsing --stdin sets UseStdin to true.
    /// </summary>
    [Fact]
    public void Parse_StdinFlag_ReturnsUseStdinTrue()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.StdinFlag });

        // Assert
        Assert.True(result.UseStdin);
        Assert.Null(result.Path);
    }

    /// <summary>
    /// Tests that parsing --stdin with --format sets UseStdin to true and the format.
    /// </summary>
    [Fact]
    public void Parse_StdinWithFormat_ReturnsUseStdinTrueAndFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.StdinFlag, ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.JsonFormat });

        // Assert
        Assert.True(result.UseStdin);
        Assert.Equal(ArgsParserTestsConstants.JsonFormat, result.Format);
    }

    /// <summary>
    /// Tests that parsing a file path returns a normalized absolute path.
    /// </summary>
    [Fact]
    public void Parse_FilePath_ReturnsNormalizedPath()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.TestFileName });

        // Assert - path should be normalized to absolute path
        Assert.NotNull(result.Path);
        Assert.EndsWith(ArgsParserTestsConstants.TestFileName, result.Path);
        Assert.False(result.UseStdin);
        Assert.Equal(ArgsParserTestsConstants.TextFormat, result.Format);
        Assert.Equal(ArgsParserTestsConstants.DefaultMinImpact, result.MinImpact);
    }

    /// <summary>
    /// Tests that parsing a single dash returns the dash as the path.
    /// </summary>
    [Fact]
    public void Parse_FilePathWithDash_ReturnsPath()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.DashPath });

        // Assert
        Assert.Equal(ArgsParserTestsConstants.DashPath, result.Path);
        Assert.False(result.UseStdin);
        Assert.Equal(ArgsParserTestsConstants.TextFormat, result.Format);
        Assert.Equal(ArgsParserTestsConstants.DefaultMinImpact, result.MinImpact);
    }

    /// <summary>
    /// Tests that parsing a file path with --format returns a normalized path and the format.
    /// </summary>
    [Fact]
    public void Parse_FilePathWithFormat_ReturnsNormalizedPathAndFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.TestFileName, ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.JsonFormat });

        // Assert - path should be normalized to absolute path
        Assert.NotNull(result.Path);
        Assert.EndsWith(ArgsParserTestsConstants.TestFileName, result.Path);
        Assert.Equal(ArgsParserTestsConstants.JsonFormat, result.Format);
    }

    /// <summary>
    /// Tests that parsing a file path with --min-impact returns a normalized path and the min impact.
    /// </summary>
    [Fact]
    public void Parse_FilePathWithMinImpact_ReturnsNormalizedPathAndMinImpact()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.TestFileName, ArgsParserTestsConstants.MinImpactFlag, ArgsParserTestsConstants.FilePathMinImpactArg });

        // Assert - path should be normalized to absolute path
        Assert.NotNull(result.Path);
        Assert.EndsWith(ArgsParserTestsConstants.TestFileName, result.Path);
        Assert.Equal(ArgsParserTestsConstants.FilePathMinImpact, result.MinImpact);
    }

    /// <summary>
    /// Tests that specifying both --stdin and a file path throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_StdinAndFilePath_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.StdinFlag, ArgsParserTestsConstants.TestFileName }));
        Assert.Contains(ArgsParserTestsConstants.StdinAndFilePathMessage, ex.Message);
    }

    /// <summary>
    /// Tests that specifying both --stdin and a dash throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_StdinAndDash_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.StdinFlag, ArgsParserTestsConstants.DashPath }));
        Assert.Contains(ArgsParserTestsConstants.StdinAndFilePathMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing all options together returns the correct values.
    /// </summary>
    [Fact]
    public void Parse_AllOptionsCombined_ReturnsAllValues()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.TestFileName, ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.CsvFormat, ArgsParserTestsConstants.MinImpactFlag, ArgsParserTestsConstants.CombinedMinImpactArg });

        // Assert - path should be normalized to absolute path
        Assert.NotNull(result.Path);
        Assert.EndsWith(ArgsParserTestsConstants.TestFileName, result.Path);
        Assert.Equal(ArgsParserTestsConstants.CsvFormat, result.Format);
        Assert.Equal(ArgsParserTestsConstants.CombinedMinImpact, result.MinImpact);
        Assert.False(result.UseStdin);
    }

    /// <summary>
    /// Tests that parsing a path with parent directory traversal (..) throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_PathWithParentDirectoryTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.ParentTraversalPath }));
        Assert.Contains(ArgsParserTestsConstants.PathTraversalMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing a path with nested parent directory traversal throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_PathWithNestedParentDirectoryTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.NestedParentTraversalPath }));
        Assert.Contains(ArgsParserTestsConstants.PathTraversalMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing a path with parent directory in the middle throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_PathWithParentDirectoryInMiddle_ThrowsArgumentException()
    {
        // Act & Assert
        // Note: Path.GetFullPath resolves ".." in the middle, so we need to check before normalization
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.ParentInMiddlePath }));
        Assert.Contains(ArgsParserTestsConstants.PathTraversalMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing an absolute path outside the current directory throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_AbsolutePathOutsideCurrentDirectory_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.AbsoluteOutsidePath }));
        Assert.Contains(ArgsParserTestsConstants.OutsideCurrentDirectoryMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing a UNC path throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_UncPath_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.BackslashUncPath }));
        Assert.Contains(ArgsParserTestsConstants.UncPathMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing a path with forward slash UNC throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Parse_PathWithForwardSlashUnc_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.ForwardSlashUncPath }));
        Assert.Contains(ArgsParserTestsConstants.UncPathMessage, ex.Message);
    }

    /// <summary>
    /// Tests that parsing a valid relative path returns a normalized absolute path.
    /// </summary>
    [Fact]
    public void Parse_ValidRelativePath_ReturnsNormalizedPath()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.DotSlashTestFilePath });

        // Assert - path should be normalized to absolute path starting with current directory
        Assert.NotNull(result.Path);
        Assert.StartsWith(Path.GetFullPath("."), result.Path);
        Assert.EndsWith(ArgsParserTestsConstants.TestFileName, result.Path);
    }

    /// <summary>
    /// Tests that parsing a simple path returns the normalized path.
    /// </summary>
    [Fact]
    public void Parse_ValidSimplePath_ReturnsPath()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.TestFileName });

        // Assert
        Assert.Equal(Path.GetFullPath(ArgsParserTestsConstants.TestFileName), result.Path);
    }

    /// <summary>
    /// Tests that parsing --stdin with all options returns the correct values.
    /// </summary>
    [Fact]
    public void Parse_StdinWithAllOptions_ReturnsAllValues()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.StdinFlag, ArgsParserTestsConstants.FormatFlag, ArgsParserTestsConstants.HtmlFormat, ArgsParserTestsConstants.MinImpactFlag, ArgsParserTestsConstants.StdinAllOptionsMinImpactArg });

        // Assert
        Assert.Null(result.Path);
        Assert.True(result.UseStdin);
        Assert.Equal(ArgsParserTestsConstants.HtmlFormat, result.Format);
        Assert.Equal(ArgsParserTestsConstants.StdinAllOptionsMinImpact, result.MinImpact);
    }

    /// <summary>
    /// Tests that reading from stdin with the dash argument returns the content.
    /// </summary>
    [Fact]
    public void ReadFileWithEncoding_Stdin_ReturnsContent()
    {
        // Arrange
        var testContent = ArgsParserTestsConstants.TestContent;
        var stdinBackup = Console.OpenStandardInput();
        try
        {
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            Console.SetIn(new StreamReader(inputStream));

            // Act
            var result = ArgsParser.ReadFileWithEncoding(ArgsParserTestsConstants.DashPath);

            // Assert
            Assert.Equal(testContent, result);
        }
        finally
        {
            Console.SetIn(new StreamReader(stdinBackup));
        }
    }

    /// <summary>
    /// Tests that reading a file with UTF-8 BOM returns the correct content.
    /// </summary>
    [Fact]
    public void ReadFileWithEncoding_FileWithUtf8Bom_ReturnsCorrectContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write UTF-8 with BOM
            File.WriteAllText(tempFile, ArgsParserTestsConstants.TestContent, Encoding.UTF8);

            // Act
            var result = ArgsParser.ReadFileWithEncoding(tempFile);

            // Assert
            Assert.Equal(ArgsParserTestsConstants.TestContent, result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that reading a file with UTF-16 LE BOM returns the correct content.
    /// </summary>
    [Fact]
    public void ReadFileWithEncoding_FileWithUtf16LeBom_ReturnsCorrectContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write UTF-16 LE with BOM
            File.WriteAllText(tempFile, ArgsParserTestsConstants.TestContent, Encoding.Unicode);

            // Act
            var result = ArgsParser.ReadFileWithEncoding(tempFile);

            // Assert
            Assert.Equal(ArgsParserTestsConstants.TestContent, result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that reading a file with UTF-16 BE BOM returns the correct content.
    /// </summary>
    [Fact]
    public void ReadFileWithEncoding_FileWithUtf16BeBom_ReturnsCorrectContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write UTF-16 BE with BOM
            File.WriteAllText(tempFile, ArgsParserTestsConstants.TestContent, Encoding.BigEndianUnicode);

            // Act
            var result = ArgsParser.ReadFileWithEncoding(tempFile);

            // Assert
            Assert.Equal(ArgsParserTestsConstants.TestContent, result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that reading a non-existent file throws a FileNotFoundException.
    /// </summary>
    [Fact]
    public void ReadFileWithEncoding_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => ArgsParser.ReadFileWithEncoding(ArgsParserTestsConstants.NonExistentFileName));
    }

    /// <summary>
    /// Tests that reading a UTF-16 LE sqlplan file returns the correct content.
    /// </summary>
    [Fact]
    public void ReadFileWithEncoding_Utf16LeSqlplan_ReturnsCorrectContent()
    {
        // Arrange
        var testFile = ArgsParserTestsConstants.Utf16LeSqlPlanSamplePath;

        // Act
        var result = ArgsParser.ReadFileWithEncoding(testFile);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(ArgsParserTestsConstants.ShowPlanXmlMarker, result);
        Assert.Contains(ArgsParserTestsConstants.MissingIndexesMarker, result);
    }

    /// <summary>
    /// Tests that <c>--fail-on</c> accepts a severity name case-insensitively and stores it as the threshold.
    /// </summary>
    [Fact]
    public void Parse_FailOnMedium_SetsFailOnThreshold()
    {
        // Act
        var result = ArgsParser.Parse(new[] { ArgsParserTestsConstants.StdinFlag, ArgsParserTestsConstants.FailOnFlag, ArgsParserTestsConstants.MixedCaseMediumSeverity });

        // Assert
        Assert.Equal(Confidence.Medium, result.FailOn);
    }

    /// <summary>
    /// Tests that an unknown <c>--fail-on</c> severity is rejected.
    /// </summary>
    [Fact]
    public void Parse_FailOnUnknownSeverity_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.StdinFlag, ArgsParserTestsConstants.FailOnFlag, ArgsParserTestsConstants.UnknownSeverity }));
        Assert.Contains(ArgsParserTestsConstants.FailOnSeverityMessage, ex.Message);
    }

    /// <summary>
    /// Tests that <c>--fail-on</c> without a value is rejected.
    /// </summary>
    [Fact]
    public void Parse_FailOnWithoutValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { ArgsParserTestsConstants.StdinFlag, ArgsParserTestsConstants.FailOnFlag }));
    }
}
