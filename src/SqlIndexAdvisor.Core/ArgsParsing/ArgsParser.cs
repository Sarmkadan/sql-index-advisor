using System.Security;
using System.Text;

namespace SqlIndexAdvisor.Core.ArgsParsing;

public static class ArgsParser
{
    public record ParseResult(
        string? Path,
        bool UseStdin,
        string Format,
        bool FailOnFindings,
        double MinImpact)
    {
        public bool ShouldShowHelp => HelpMessage is not null;
        public string? HelpMessage { get; init; }
        public string SchemaVersion { get; init; } = "1.0";

        public static ParseResult Help(string message) => new(null, false, "text", false, 0) { HelpMessage = message };
    }

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

    private static string RequireValue(string[] args, ref int i, string name)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"{name} requires a value.");
        return args[++i];
    }

    /// <summary>
    /// Validates and resolves a file path to prevent path traversal attacks.
    /// </summary>
    /// <param name="path">The input path to validate.</param>
    /// <returns>The resolved and validated absolute path.</returns>
    /// <exception cref="ArgumentException">Thrown when path traversal is detected or path is invalid.</exception>
    /// <exception cref="SecurityException">Thrown when path is outside allowed boundaries.</exception>
    private static string ValidateAndResolvePath(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (path == "-")
            return path;

        // Normalize path separators for consistent checking
        var normalizedInput = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // Check for path traversal sequences (..) before normalization
        // These are security-sensitive patterns that should be rejected
        var segments = normalizedInput.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        var depth = 0;
        foreach (var segment in segments)
        {
            if (segment == "..")
            {
                // Any parent traversal (..) is a security risk
                throw new ArgumentException($"Path traversal sequences (..) are not allowed. Got: {path}");
            }
            else if (segment != ".")
            {
                depth++;
            }
        }


        // Check for UNC paths (network paths) which could be used for unintended access
        if (normalizedInput.StartsWith("\\\\") || normalizedInput.StartsWith("//"))
            throw new ArgumentException($"UNC paths are not allowed. Got: {path}");

        // Normalize path separators and resolve relative paths
        var fullPath = Path.GetFullPath(path);

        // Check for absolute paths that are not rooted in the current directory
        // (GetFullPath should convert relative to absolute, so this catches attempts to go outside)
        var currentDir = Path.GetFullPath(".");
        if (Path.IsPathRooted(fullPath) &&
            !fullPath.StartsWith(currentDir, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Paths outside the current directory are not allowed. Got: {path}");
        }

        return fullPath;
    }

    /// <summary>
    /// Reads file content with automatic encoding detection (handles BOM for UTF-8, UTF-16, UTF-32).
    /// </summary>
    /// <param name="path">File path, or "-" for stdin.</param>
    /// <returns>Decoded file content.</returns>
    /// <exception cref="FileNotFoundException">Thrown when file doesn't exist and path is not "-".</exception>
    /// <exception cref="IOException">Thrown when file cannot be read.</exception>
    /// <exception cref="ArgumentException">Thrown when path validation fails.</exception>
    public static string ReadFileWithEncoding(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (path == "-")
        {
            // Read from stdin - use UTF-8 encoding
            using var stdinReader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: false);
            return stdinReader.ReadToEnd();
        }

        if (!File.Exists(path))
            throw new FileNotFoundException($"plan file not found: {path}");

        // Read file with automatic encoding detection
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
