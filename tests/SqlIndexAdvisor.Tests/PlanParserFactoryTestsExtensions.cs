using System;
using System.Collections.Generic;
using System.Globalization;

namespace SqlIndexAdvisor.Tests
{
    /// <summary>
    /// Extension methods for <see cref="PlanParserFactoryTests"/> that provide additional functionality
    /// for testing plan parser factory behavior and edge cases.
    /// </summary>
    public static class PlanParserFactoryTestsExtensions
    {
        /// <summary>
        /// Creates a test case with XML content that represents a SQL Server execution plan
        /// with the specified operation type.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="operationType">The operation type to include in the XML plan.</param>
        /// <returns>A string containing XML execution plan content.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
        public static string CreateSqlServerXmlPlanWithOperation(this PlanParserFactoryTests tests, string operationType)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(operationType);

            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                   "<ShowPlanXML>\n" +
                   "    <Batch>\n" +
                   "        <Statements>\n" +
                   "            <StmtSimple StatementText=\"SELECT * FROM Table WHERE Operation = '" + operationType + "'\" StatementId=\"1\" StatementCompId=\"1\" StatementType=\"SELECT\" QueryHash=\"0xABCD\" QueryPlanHash=\"0x1234\">\n" +
                   "                <QueryPlan>\n" +
                   "                    <RelOp Op=\"" + operationType + "\" PhysicalOp=\"Clustered Index Scan\" LogicalOp=\"Index Scan\">\n" +
                   "                        <OutputList>\n" +
                   "                            <ColumnReference Database=\"[TestDB]\" Schema=\"[dbo]\" Table=\"[TestTable]\" Column=\"Id\" />\n" +
                   "                        </OutputList>\n" +
                   "                        <IndexScan Ordered=\"true\" ScanDirection=\"FORWARD\" ForcedIndex=\"false\" ForceSeek=\"false\">\n" +
                   "                            <DefinedValues />\n" +
                   "                            <Object Database=\"[TestDB]\" Schema=\"[dbo]\" Table=\"[TestTable]\" Index=\"[PK_TestTable]\" />\n" +
                   "                        </IndexScan>\n" +
                   "                    </RelOp>\n" +
                   "                </QueryPlan>\n" +
                   "            </StmtSimple>\n" +
                   "        </Statements>\n" +
                   "    </Batch>\n" +
                   "</ShowPlanXML>";
        }

        /// <summary>
        /// Creates a test case with JSON content that represents a PostgreSQL execution plan
        /// with the specified node type.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <param name="nodeType">The node type to include in the JSON plan.</param>
        /// <returns>A string containing JSON execution plan content.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
        public static string CreatePostgresJsonPlanWithNode(this PlanParserFactoryTests tests, string nodeType)
        {
            ArgumentNullException.ThrowIfNull(tests);
            ArgumentException.ThrowIfNullOrEmpty(nodeType);

            return "{\n    \"Plan\": {\n        \"Node Type\": \"" + nodeType + "\",\n" +
                   "        \"Actual Total Time\": 0.123,\n        \"Actual Rows\": 100,\n        \"Actual Loops\": 1,\n        \"Output\": [\"id\", \"name\"],\n" +
                   "        \"Plans\": [\n" +
                   "            {\n                \"Node Type\": \"Index Scan\",\n" +
                   "                \"Actual Total Time\": 0.045,\n                \"Actual Rows\": 100,\n                \"Actual Loops\": 1,\n                \"Index Name\": \"idx_test_table_id\"\n" +
                   "            }\n" +
                   "        ]\n" +
                   "    }\n" +
                   "}";
        }

        /// <summary>
        /// Creates a collection of test cases with various whitespace prefixes to verify
        /// that the parser correctly handles leading whitespace.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>An enumerable of test cases with different whitespace prefixes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
        public static IEnumerable<string> CreateWhitespacePrefixedTestCases(this PlanParserFactoryTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            yield return string.Empty;
            yield return " ";
            yield return "  ";
            yield return "\t";
            yield return "\n";
            yield return " \n\t ";
            yield return "  \n\t  " + CreateSqlServerXmlPlanWithOperation(tests, "SELECT");
        }

        /// <summary>
        /// Creates a collection of unrecognized content types that should cause
        /// <see cref="PlanParseException"/> to be thrown when parsing.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>An enumerable of unrecognized content strings.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
        public static IEnumerable<string> CreateUnrecognizedContentCases(this PlanParserFactoryTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            yield return null!;
            yield return string.Empty;
            yield return "  ";
            yield return "SELECT * FROM Table";
            yield return "{ \"invalid\": \"json\" }";
            yield return "<!-- HTML comment --><div>Not a plan</div>";
            yield return "This is just plain text without any structure";
        }

        /// <summary>
        /// Verifies that the parser can handle execution plans with different
        /// culture-invariant number formats.
        /// </summary>
        /// <param name="tests">The test instance.</param>
        /// <returns>A string containing a plan with invariant culture number formatting.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tests"/> is null.</exception>
        public static string CreatePlanWithInvariantCultureNumbers(this PlanParserFactoryTests tests)
        {
            ArgumentNullException.ThrowIfNull(tests);

            // Use invariant culture for machine-facing numbers
            return "<?xml version=\"1.0\"?>\n" +
                   "<ShowPlanXML>\n" +
                   "    <Batch>\n" +
                   "        <Statements>\n" +
                   "            <StmtSimple StatementText=\"SELECT * FROM Table\" StatementId=\"1\" StatementCompId=\"1\" QueryHash=\"0xABCD\">\n" +
                   "                <QueryPlan>\n" +
                   "                    <RelOp PhysicalOp=\"Index Scan\" LogicalOp=\"Index Scan\">\n" +
                   "                        <IndexScan>\n" +
                   "                            <IndexScanStats\n" +
                   "                                ActualPages=" + 123456.ToString(CultureInfo.InvariantCulture) + "\n" +
                   "                                EstimatedPages=" + 98765.ToString(CultureInfo.InvariantCulture) + "\n" +
                   "                                ActualRows=" + 1000.ToString(CultureInfo.InvariantCulture) + " />\n" +
                   "                        </IndexScan>\n" +
                   "                    </RelOp>\n" +
                   "                </QueryPlan>\n" +
                   "            </StmtSimple>\n" +
                   "        </Statements>\n" +
                   "    </Batch>\n" +
                   "</ShowPlanXML>";
        }
    }
}
