using System;
using System.IO;
using System.Security;
using System.Text;

namespace SqlIndexAdvisor.Core.ArgsParsing;

/// <summary>
/// Parses command‑line arguments for the sql‑index‑advisor tool.
/// </summary>
public static class ArgsParser
{
    /// <summary>
    /// Result of parsing the command‑line arguments.
    /// </summary>
    /// <param name="Path">Path to the plan file, or <c>null</c> when reading from stdin.</param>
    /// <param name="UseStdin">Whether the <c>--stdin</c> flag was supplied.</param>
    /// <param name="Format">Desired output format (text, json, html, csv).</param>
    /// <param name="FailOnFindings">Whether the <c>--fail-on-findings</c> flag was supplied.</param>
    /// <param name="MinImpact">Minimum impact threshold.</param>
    public sealed record ParseResult(
        string? Path,
        bool UseStdin,
        string Format,
        bool FailOnFindings,
        double MinImpact)
    {
        /// <summary>
        /// Gets a value indicating whether the parser decided to show the help message.
        /// </summary>
        public bool ShouldShowHelp => HelpMessage is not null;

        /// <summary>
        /// Gets a value indicating whether an error occurred during parsing.
        /// </summary>
        public bool IsError => ErrorMessage is not null;

        /// <summary>
        /// Optional help text to display.
        /// </summary>
        public string? HelpMessage { get; init; }

        /// <summary>
        /// Optional error text describing why parsing failed.
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Schema version of the output (default <c>1.0</c>).
        /// </summary>
        public string SchemaVersion { get; init; } = "1.0";

        /// <summary>
        /// Creates a <see cref="ParseResult"/> that signals the help screen should be shown.
        /// </summary>
        /// <param name="message">Help text.</param>
        /// <returns>A help <see cref="ParseResult"/>.</returns>
        public static ParseResult Help(string message) =>
            new(null, false, "text", false, 0) { HelpMessage = message };

        /// <summary>
        /// Creates a <see cref="ParseResult"/> that signals a parsing error.
        /// </summary>
        /// <param name="message">Error description.</param>
        /// <returns>An error <see cref="ParseResult"/>.</returns>
        public static ParseResult Error(string message) =>
            new(null, false, "text", false, 0) { ErrorMessage = message };
    }

    /// <summary>
    /// Parses the supplied command‑line arguments.
    /// </summary>
    /// <param name="args">Array of arguments (typically <c>Environment.GetCommandLineArgs()</c> without the executable name).</param>
    /// <returns>A <see cref="ParseResult"/> describing the outcome.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="args"/> is <c>null</c>.</exception>
    public static ParseResult Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0)
            return ParseResult.Help(Usage);

        var useStdin = false;
        var format = "text";
        var failOnFindings = false;
        double minImpact = 0;
        string? path = null;

        try
        {
            for (var i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    case "--stdin":
                        useStdin = true;
                        break;
                    case "--format":
                        format = RequireValue(args, ref i, "--format").ToLowerInvariant();
                        break;
                    case "--min-impact":
                        var raw = RequireValue(args, ref i, "--min-impact");
                        if (!double.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture, out minImpact))
                            throw new ArgumentException($"--min-impact expects a number, got '{raw}'.");
                        break;
                    case "--fail-on-findings":
                        failOnFindings = true;
                        break;
                    case "-h":
                    case "--help":
                        return ParseResult.Help(Usage);
                    default:
                        if (a.StartsWith('-') && a != "-")
                            throw new ArgumentException($"unknown option '{a}'.");
                        path = ValidateAndResolvePath(a);
                        break;
                }
            }

            if (format != "text" && format != "json" && format != "html" && format != "csv")
                throw new ArgumentException($"--format must be 'text', 'json', 'html', or 'csv', got '{format}'.");

            if (useStdin && path is not null)
                throw new ArgumentException("cannot specify both --stdin and a file path.");

            return new ParseResult(path, useStdin, format, failOnFindings, minImpact);
        }
        catch (ArgumentException ex)
        {
            return ParseResult.Error(ex.Message);
        }
        catch (SecurityException ex)
        {
            return ParseResult.Error(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the next argument value for an option that expects one.
    /// </summary>
    /// <param name="args">Full argument array.</param>
    /// <param name="i">Current index; will be incremented to point at the value.</param>
    /// <param name="name">Option name (used for error messages).</param>
    /// <returns>The value following the option.</returns>
    /// <exception cref="ArgumentException">When the value is missing.</exception>
    private static string RequireValue(string[] args, ref int i, string name) =>
        i + 1 >= args.Length
            ? throw new ArgumentException($"{name} requires a value.")
            : args[++i];

    /// <summary>
    /// Validates and resolves a file path to prevent path traversal attacks.
    /// </summary>
    /// <param name="path">The input path to validate.</param>
    /// <returns>The resolved and validated absolute path.</returns>
    /// <exception cref="ArgumentException">When path traversal is detected or path is invalid.</exception>
    /// <exception cref="SecurityException">When path is outside allowed boundaries.</exception>
    private static string ValidateAndResolvePath(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (path == "-")
            return path;

        // Normalize path separators for consistent checking
        var normalizedInput = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // Check for path traversal sequences (..) before normalization
        var segments = normalizedInput.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (segment == "..")
                throw new ArgumentException($"Path traversal sequences (..) are not allowed. Got: {path}");
        }

        // Disallow UNC/network paths
        if (normalizedInput.StartsWith("\\\\") || normalizedInput.StartsWith("//"))
            throw new ArgumentException($"UNC paths are not allowed. Got: {path}");

        // Resolve to absolute path
        var fullPath = Path.GetFullPath(path);

        // Ensure the path stays within the current directory
        var currentDir = Path.GetFullPath(".");
        if (Path.IsPathRooted(fullPath) &&
            !fullPath.StartsWith(currentDir, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Paths outside the current directory are not allowed. Got: {path}");
        }

        return fullPath;
    }

    /// <summary>
    /// Reads file content with automatic encoding detection (handles BOM for UTF‑8, UTF‑16, UTF‑32).
    /// </summary>
    /// <param name="path">File path, or <c>-</c> for stdin.</param>
    /// <returns>Decoded file content.</returns>
    /// <exception cref="ArgumentException">When <paramref name="path"/> is null or empty.</exception>
    /// <exception cref="FileNotFoundException">When the file does not exist and <paramref name="path"/> is not <c>-</c>.</exception>
    /// <exception cref="IOException">When the file cannot be read.</exception>
    public static string ReadFileWithEncoding(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (path == "-")
        {
            using var stdinReader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: false);
            return stdinReader.ReadToEnd();
        }

        if (!File.Exists(path))
            throw new FileNotFoundException($"plan file not found: {path}");

        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, FileOptions.SequentialScan);
        using var fileReader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096);
        return fileReader.ReadToEnd();
    }

    private const string Usage = """
sql-index-advisor - recommend missing indexes from a query execution plan

USAGE:
sql-index-advisor <plan-file> [--format text|json|html|csv] [--min-impact <n>] [--fail-on-findings]
sql-index-advisor - [--format text|json|html|csv] < input.json
sql-index-advisor --stdin [--format text|json|html|csv] [--fail-on-findings]

ARGUMENTS:
<plan-file> Path to a SQL Server showplan XML or Postgres EXPLAIN (FORMAT JSON) file.
Use "-" to read from standard input.

OPTIONS:
--stdin Read the plan from standard input instead of a file.
--format <fmt> Output format: text (default), json, html, or csv.
--min-impact <n> Hide recommendations below this estimated impact percent.
--fail-on-findings Exit with code 1 if recommendations are found, 0 otherwise.
-h, --help Show this help.
""";
}
