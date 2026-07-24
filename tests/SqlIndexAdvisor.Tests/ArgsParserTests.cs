using System.Text;
using SqlIndexAdvisor.Core.ArgsParsing;
using Xunit;

namespace SqlIndexAdvisor.Tests;

public class ArgsParserTests
{
    [Fact]
    public void Parse_EmptyArgs_ReturnsHelpRequest()
    {
        // Act
        var result = ArgsParser.Parse(Array.Empty<string>());

        // Assert
        Assert.True(result.ShouldShowHelp);
        Assert.NotNull(result.HelpMessage);
        Assert.Contains("sql-index-advisor", result.HelpMessage);
    }

    [Fact]
    public void Parse_HelpFlag_ReturnsHelpRequest()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--help" });

        // Assert
        Assert.True(result.ShouldShowHelp);
        Assert.NotNull(result.HelpMessage);
        Assert.Contains("sql-index-advisor", result.HelpMessage);
    }

    [Fact]
    public void Parse_HelpShortFlag_ReturnsHelpRequest()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "-h" });

        // Assert
        Assert.True(result.ShouldShowHelp);
        Assert.NotNull(result.HelpMessage);
        Assert.Contains("sql-index-advisor", result.HelpMessage);
    }

    [Fact]
    public void Parse_UnknownOption_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "--unknown" }));
        Assert.Contains("unknown option", ex.Message);
    }

    [Fact]
    public void Parse_UnknownShortOption_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "-x" }));
        Assert.Contains("unknown option", ex.Message);
    }

    [Fact]
    public void Parse_MissingFormatValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "--format" }));
        Assert.Contains("--format requires a value", ex.Message);
    }

    [Fact]
    public void Parse_InvalidFormatValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "--format", "invalid" }));
        Assert.Contains("--format must be", ex.Message);
    }

    [Fact]
    public void Parse_ValidTextFormat_ReturnsCorrectFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--format", "text" });

        // Assert
        Assert.False(result.ShouldShowHelp);
        Assert.Equal("text", result.Format);
        Assert.Null(result.Path);
        Assert.False(result.UseStdin);
        Assert.Equal(0, result.MinImpact);
    }

    [Fact]
    public void Parse_ValidJsonFormat_ReturnsCorrectFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--format", "json" });

        // Assert
        Assert.False(result.ShouldShowHelp);
        Assert.Equal("json", result.Format);
    }

    [Fact]
    public void Parse_ValidHtmlFormat_ReturnsCorrectFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--format", "html" });

        // Assert
        Assert.False(result.ShouldShowHelp);
        Assert.Equal("html", result.Format);
    }

    [Fact]
    public void Parse_ValidCsvFormat_ReturnsCorrectFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--format", "csv" });

        // Assert
        Assert.False(result.ShouldShowHelp);
        Assert.Equal("csv", result.Format);
    }

    [Fact]
    public void Parse_FormatIsCaseInsensitive_ReturnsLowercasedFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--format", "JSON" });

        // Assert
        Assert.Equal("json", result.Format);
    }

    [Fact]
    public void Parse_MissingMinImpactValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "--min-impact" }));
        Assert.Contains("--min-impact requires a value", ex.Message);
    }

    [Fact]
    public void Parse_InvalidMinImpactValue_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "--min-impact", "not-a-number" }));
        Assert.Contains("--min-impact expects a number", ex.Message);
    }

    [Fact]
    public void Parse_ValidMinImpactValue_ReturnsCorrectMinImpact()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--min-impact", "50.5" });

        // Assert
        Assert.Equal(50.5, result.MinImpact);
    }

    [Fact]
    public void Parse_StdinFlag_ReturnsUseStdinTrue()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--stdin" });

        // Assert
        Assert.True(result.UseStdin);
        Assert.Null(result.Path);
    }

    [Fact]
    public void Parse_StdinWithFormat_ReturnsUseStdinTrueAndFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--stdin", "--format", "json" });

        // Assert
        Assert.True(result.UseStdin);
        Assert.Equal("json", result.Format);
    }

    [Fact]
    public void Parse_FilePath_ReturnsNormalizedPath()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "test.xml" });

        // Assert - path should be normalized to absolute path
        Assert.NotNull(result.Path);
        Assert.EndsWith("test.xml", result.Path);
        Assert.False(result.UseStdin);
        Assert.Equal("text", result.Format);
        Assert.Equal(0, result.MinImpact);
    }

    [Fact]
    public void Parse_FilePathWithDash_ReturnsPath()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "-" });

        // Assert
        Assert.Equal("-", result.Path);
        Assert.False(result.UseStdin);
        Assert.Equal("text", result.Format);
        Assert.Equal(0, result.MinImpact);
    }

    [Fact]
    public void Parse_FilePathWithFormat_ReturnsNormalizedPathAndFormat()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "test.xml", "--format", "json" });

        // Assert - path should be normalized to absolute path
        Assert.NotNull(result.Path);
        Assert.EndsWith("test.xml", result.Path);
        Assert.Equal("json", result.Format);
    }

    [Fact]
    public void Parse_FilePathWithMinImpact_ReturnsNormalizedPathAndMinImpact()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "test.xml", "--min-impact", "25" });

        // Assert - path should be normalized to absolute path
        Assert.NotNull(result.Path);
        Assert.EndsWith("test.xml", result.Path);
        Assert.Equal(25, result.MinImpact);
    }

    [Fact]
    public void Parse_StdinAndFilePath_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "--stdin", "test.xml" }));
        Assert.Contains("cannot specify both --stdin and a file path", ex.Message);
    }

    [Fact]
    public void Parse_StdinAndDash_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "--stdin", "-" }));
        Assert.Contains("cannot specify both --stdin and a file path", ex.Message);
    }

    [Fact]
    public void Parse_AllOptionsCombined_ReturnsAllValues()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "test.xml", "--format", "csv", "--min-impact", "10.5" });

        // Assert - path should be normalized to absolute path
        Assert.NotNull(result.Path);
        Assert.EndsWith("test.xml", result.Path);
        Assert.Equal("csv", result.Format);
        Assert.Equal(10.5, result.MinImpact);
        Assert.False(result.UseStdin);
    }

    [Fact]
    public void Parse_PathWithParentDirectoryTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "../test.xml" }));
        Assert.Contains("Path traversal sequences (..) are not allowed", ex.Message);
    }

    [Fact]
    public void Parse_PathWithNestedParentDirectoryTraversal_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "../../test.xml" }));
        Assert.Contains("Path traversal sequences (..) are not allowed", ex.Message);
    }

    [Fact]
    public void Parse_PathWithParentDirectoryInMiddle_ThrowsArgumentException()
    {
        // Act & Assert
        // Note: Path.GetFullPath resolves ".." in the middle, so we need to check before normalization
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "subdir\\..\\test.xml" }));
        Assert.Contains("Path traversal sequences (..) are not allowed", ex.Message);
    }

    [Fact]
    public void Parse_AbsolutePathOutsideCurrentDirectory_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "/etc/passwd" }));
        Assert.Contains("Paths outside the current directory are not allowed", ex.Message);
    }

    [Fact]
    public void Parse_UncPath_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "\\\\server\\share\\file.txt" }));
        Assert.Contains("UNC paths are not allowed", ex.Message);
    }

    [Fact]
    public void Parse_PathWithForwardSlashUnc_ThrowsArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => ArgsParser.Parse(new[] { "//server/share/file.txt" }));
        Assert.Contains("UNC paths are not allowed", ex.Message);
    }

    [Fact]
    public void Parse_ValidRelativePath_ReturnsNormalizedPath()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "./test.xml" });

        // Assert - path should be normalized to absolute path starting with current directory
        Assert.NotNull(result.Path);
        Assert.StartsWith(Path.GetFullPath("."), result.Path);
        Assert.EndsWith("test.xml", result.Path);
    }

    [Fact]
    public void Parse_ValidSimplePath_ReturnsPath()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "test.xml" });

        // Assert
        Assert.Equal(Path.GetFullPath("test.xml"), result.Path);
    }

    [Fact]
    public void Parse_StdinWithAllOptions_ReturnsAllValues()
    {
        // Act
        var result = ArgsParser.Parse(new[] { "--stdin", "--format", "html", "--min-impact", "75" });

        // Assert
        Assert.Null(result.Path);
        Assert.True(result.UseStdin);
        Assert.Equal("html", result.Format);
        Assert.Equal(75, result.MinImpact);
    }

    [Fact]
    public void ReadFileWithEncoding_Stdin_ReturnsContent()
    {
        // Arrange
        var testContent = "test content";
        var stdinBackup = Console.OpenStandardInput();
        try
        {
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
            Console.SetIn(new StreamReader(inputStream));

            // Act
            var result = ArgsParser.ReadFileWithEncoding("-");

            // Assert
            Assert.Equal(testContent, result);
        }
        finally
        {
            Console.SetIn(new StreamReader(stdinBackup));
        }
    }

    [Fact]
    public void ReadFileWithEncoding_FileWithUtf8Bom_ReturnsCorrectContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write UTF-8 with BOM
            File.WriteAllText(tempFile, "test content", Encoding.UTF8);

            // Act
            var result = ArgsParser.ReadFileWithEncoding(tempFile);

            // Assert
            Assert.Equal("test content", result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ReadFileWithEncoding_FileWithUtf16LeBom_ReturnsCorrectContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write UTF-16 LE with BOM
            File.WriteAllText(tempFile, "test content", Encoding.Unicode);

            // Act
            var result = ArgsParser.ReadFileWithEncoding(tempFile);

            // Assert
            Assert.Equal("test content", result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ReadFileWithEncoding_FileWithUtf16BeBom_ReturnsCorrectContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write UTF-16 BE with BOM
            File.WriteAllText(tempFile, "test content", Encoding.BigEndianUnicode);

            // Act
            var result = ArgsParser.ReadFileWithEncoding(tempFile);

            // Assert
            Assert.Equal("test content", result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ReadFileWithEncoding_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => ArgsParser.ReadFileWithEncoding("non-existent-file.txt"));
    }

    [Fact]
    public void ReadFileWithEncoding_Utf16LeSqlplan_ReturnsCorrectContent()
    {
        // Arrange
        var testFile = "samples/sqlserver_orders_scan_utf16le.sqlplan";

        // Act
        var result = ArgsParser.ReadFileWithEncoding(testFile);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("ShowPlanXML", result);
        Assert.Contains("MissingIndexes", result);
    }
}
