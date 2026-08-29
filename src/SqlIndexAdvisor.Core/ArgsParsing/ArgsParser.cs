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
    private static readonly System.Diagnostics.ActivitySource DiagnosticSource = new("SqlIndexAdvisor.ArgsParsing");

    private const int ParseStartedEventId = 100;
    private const int OptionRecognizedEventId = 110;
    private const int DefaultAppliedEventId = 120;
    private const int ParseCompletedEventId = 190;
    private const int SuspiciousArgumentEventId = 290;

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
        /// Gets a value indicating whether parsing succeeded without help or error.
        /// </summary>
        public bool IsSuccess => !ShouldShowHelp && !IsError;

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
        public static ParseResult Help(string message)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            return new(null, false, "text", false, 0) { HelpMessage = message };
        }

        /// <summary>
        /// Creates a <see cref="ParseResult"/> that signals a parsing error.
        /// </summary>
        /// <param name="message">Error description.</param>
        /// <returns>An error <see cref="ParseResult"/>.</returns>
        public static ParseResult Error(string message)
        {
            ArgumentException.ThrowIfNullOrEmpty(message);
            return new(null, false, "text", false, 0) { ErrorMessage = message };
        }
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

        using var activity = DiagnosticSource.HasListeners()
            ? DiagnosticSource.StartActivity("Parse")
            : null;
        if (activity is not null)
        {
            AddEvent(activity, ParseStartedEventId, "ParseStarted", "Information",
                Tag("args.count", args.Length));
        }

        // No arguments – show help (usage) immediately.
        if (args.Length == 0)
        {
            var result = ParseResult.Help(Usage);
            if (activity is not null)
            {
                AddEvent(activity, DefaultAppliedEventId, "FormatDefaulted", "Information",
                    Tag("format", result.Format));
                AddParseResultEvent(activity, result);
            }
            return result;
        }

        var useStdin = false;
        var format = "text";
        var formatSpecified = false;
        var failOnFindings = false;
        var minImpact = 0.0;
        string? path = null;

        // Flags that are mutually exclusive – we track their presence to detect conflicts.
        var sqlServerFlag = false;
        var postgresFlag = false;

        try
        {
            for (var i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    case "--stdin":
                        useStdin = true;
                        if (activity is not null)
                            AddOptionEvent(activity, "--stdin", useStdin);
                        break;

                    case "--plan":
                        // Explicit plan flag – expects a value.
                        var planPath = RequireValue(args, ref i, "--plan");
                        if (path is not null)
                            throw new ArgumentException("multiple plan sources specified (positional argument and --plan).");
                        path = ValidateAndResolvePath(planPath);
                        if (activity is not null)
                            AddOptionEvent(activity, "--plan", path);
                        break;

                    case "--format":
                        format = RequireValue(args, ref i, "--format").ToLowerInvariant();
                        formatSpecified = true;
                        if (activity is not null)
                            AddOptionEvent(activity, "--format", format);
                        break;

                    case "--min-impact":
                        var raw = RequireValue(args, ref i, "--min-impact");
                        if (!double.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture, out minImpact))
                            throw new ArgumentException($"--min-impact expects a number, got '{raw}'.");
                        if (activity is not null)
                            AddOptionEvent(activity, "--min-impact", minImpact);
                        break;

                    case "--fail-on-findings":
                        failOnFindings = true;
                        if (activity is not null)
                            AddOptionEvent(activity, "--fail-on-findings", failOnFindings);
                        break;

                    case "--sqlserver":
                        sqlServerFlag = true;
                        if (activity is not null)
                            AddOptionEvent(activity, "--sqlserver", sqlServerFlag);
                        break;

                    case "--postgres":
                        postgresFlag = true;
                        if (activity is not null)
                            AddOptionEvent(activity, "--postgres", postgresFlag);
                        break;

                    case "--version":
                        // Version is treated as a help screen that only shows version info.
                        if (activity is not null)
                            AddOptionEvent(activity, "--version", Version);
                        var versionResult = ParseResult.Help($"sql-index-advisor version {Version}");
                        if (activity is not null)
                            AddParseResultEvent(activity, versionResult);
                        return versionResult;

                    case "-h":
                    case "--help":
                        if (activity is not null)
                            AddOptionEvent(activity, a, true);
                        var helpResult = ParseResult.Help(Usage);
                        if (activity is not null)
                            AddParseResultEvent(activity, helpResult);
                        return helpResult;

                    default:
                        // Positional argument – treat as plan file unless it looks like an unknown option.
                        if (a.StartsWith('-') && a != "-")
                            throw new ArgumentException($"unknown option '{a}'.");
                        if (path is not null)
                            throw new ArgumentException("multiple plan sources specified.");
                        path = ValidateAndResolvePath(a);
                        if (activity is not null)
                            AddOptionEvent(activity, "plan-path", path);
                        break;
                }
            }

            // Validate mutually exclusive database flags.
            if (sqlServerFlag && postgresFlag)
                throw new ArgumentException("cannot specify both --sqlserver and --postgres.");

            // Validate format.
            if (format is not ("text" or "json" or "html" or "csv"))
                throw new ArgumentException($"--format must be 'text', 'json', 'html', or 'csv', got '{format}'.");

            // Validate stdin vs file path conflict.
            if (useStdin && path is not null)
                throw new ArgumentException("cannot specify both --stdin and a file path.");

            if (!formatSpecified && activity is not null)
            {
                AddEvent(activity, DefaultAppliedEventId, "FormatDefaulted", "Information",
                    Tag("format", format));
            }

            var result = new ParseResult(path, useStdin, format, failOnFindings, minImpact);
            if (activity is not null)
                AddParseResultEvent(activity, result);
            return result;
        }
        catch (ArgumentException ex)
        {
            return TraceErrorResult(activity, ex.Message);
        }
        catch (SecurityException ex)
        {
            return TraceErrorResult(activity, ex.Message);
        }
    }

    private static ParseResult TraceErrorResult(System.Diagnostics.Activity? activity, string message)
    {
        var result = ParseResult.Error(message);
        if (activity is not null)
        {
            AddEvent(activity, SuspiciousArgumentEventId, "ArgumentRejected", "Warning",
                Tag("error.message", message));
            AddParseResultEvent(activity, result);
        }
        return result;
    }

    private static void AddOptionEvent(System.Diagnostics.Activity activity, string option, object value) =>
        AddEvent(activity, OptionRecognizedEventId, "OptionRecognized", "Information",
            Tag("option.name", option), Tag("option.value", value));

    private static void AddParseResultEvent(System.Diagnostics.Activity activity, ParseResult result) =>
        AddEvent(activity, ParseCompletedEventId, "ParseCompleted", result.IsError ? "Warning" : "Information",
            Tag("result.status", result.IsError ? "error" : result.ShouldShowHelp ? "help" : "success"),
            Tag("result.path", result.Path),
            Tag("result.use_stdin", result.UseStdin),
            Tag("result.format", result.Format),
            Tag("result.fail_on_findings", result.FailOnFindings),
            Tag("result.min_impact", result.MinImpact),
            Tag("result.schema_version", result.SchemaVersion));

    private static System.Collections.Generic.KeyValuePair<string, object?> Tag(string key, object? value) =>
        new(key, value);

    private static void AddEvent(
        System.Diagnostics.Activity activity,
        int eventId,
        string eventName,
        string level,
        params System.Collections.Generic.KeyValuePair<string, object?>[] tags)
    {
        var eventTags = new System.Diagnostics.ActivityTagsCollection
        {
            ["event.id"] = eventId,
            ["event.level"] = level
        };
        foreach (var tag in tags)
            eventTags[tag.Key] = tag.Value;
        activity.AddEvent(new System.Diagnostics.ActivityEvent(eventName, tags: eventTags));
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

        // Normalize path separators for consistent checking.
        var normalizedInput = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        // Check for path traversal sequences (..) before normalization.
        var segments = normalizedInput.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var segment in segments)
        {
            if (segment == "..")
                throw new ArgumentException($"Path traversal sequences (..) are not allowed. Got: {path}");
        }

        // Disallow UNC/network paths.
        if (normalizedInput.StartsWith("\\\\") || normalizedInput.StartsWith("//"))
            throw new ArgumentException($"UNC paths are not allowed. Got: {path}");

        // Resolve to absolute path.
        var fullPath = Path.GetFullPath(path);

        // Ensure the path stays within the current directory.
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

    /// <summary>
    /// Current version of the tool (hard‑coded for now; can be replaced by a build‑time constant).
    /// </summary>
    private const string Version = "1.0.0";

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
--plan <path> Explicitly specify the plan file (alternative to positional argument).
--format <fmt> Output format: text (default), json, html, or csv.
--min-impact <n> Hide recommendations below this estimated impact percent.
--fail-on-findings Exit with code 1 if recommendations are found, 0 otherwise.
--sqlserver Use SQL Server specific parsing (mutually exclusive with --postgres).
--postgres Use PostgreSQL specific parsing (mutually exclusive with --sqlserver).
--version Show version information.
-h, --help Show this help.
""";
}
