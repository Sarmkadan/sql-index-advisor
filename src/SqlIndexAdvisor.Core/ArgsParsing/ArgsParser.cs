namespace SqlIndexAdvisor.Core.ArgsParsing;

public static class ArgsParser
{
	public static ParseResult Parse(string[] args)
	{
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
					path = a;
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
	--stdin		Read the plan from standard input instead of a file.
	--format <fmt>	Output format: text (default), json, html, or csv.
	--min-impact <n>	Hide recommendations below this estimated impact percent.
	--fail-on-findings	Exit with code 1 if recommendations are found, 0 otherwise.
	-h, --help		Show this help.
""";
}