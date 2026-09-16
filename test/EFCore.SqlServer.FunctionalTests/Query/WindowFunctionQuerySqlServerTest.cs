// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class WindowFunctionQuerySqlServerTest(NonSharedFixture fixture) : WindowFunctionQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder UseWindowFunctionAggregates(DbContextOptionsBuilder optionsBuilder)
    {
#pragma warning disable EF9001 // UseWindowFunctionAggregates is experimental
        new SqlServerDbContextOptionsBuilder(optionsBuilder).UseWindowFunctionAggregates();
#pragma warning restore EF9001

        return optionsBuilder;
    }

    public override async Task Elements_and_aggregate()
    {
        await base.Elements_and_aggregate();

        AssertSql(
            """
SELECT [e].[DepartmentName], [e].[Id], [e].[Bonus], [e].[Name], [e].[Salary], AVG(CAST([e].[Salary] AS float)) OVER(PARTITION BY [e].[DepartmentName])
FROM [Employees] AS [e]
ORDER BY [e].[DepartmentName]
""");
    }

    public override async Task Elements_and_multiple_aggregates()
    {
        await base.Elements_and_multiple_aggregates();

        AssertSql(
            """
SELECT [e].[DepartmentName], [e].[Id], [e].[Bonus], [e].[Name], [e].[Salary], COUNT(*) OVER(PARTITION BY [e].[DepartmentName]), SUM([e].[Salary]) OVER(PARTITION BY [e].[DepartmentName]), MAX([e].[Salary]) OVER(PARTITION BY [e].[DepartmentName])
FROM [Employees] AS [e]
ORDER BY [e].[DepartmentName]
""");
    }

    public override async Task Projected_elements_and_aggregate()
    {
        await base.Projected_elements_and_aggregate();

        AssertSql(
            """
SELECT [e].[DepartmentName], [e].[Name], SUM([e].[Salary]) OVER(PARTITION BY [e].[DepartmentName])
FROM [Employees] AS [e]
ORDER BY [e].[DepartmentName]
""");
    }

    public override async Task Nullable_aggregate_over_partition_without_values()
    {
        await base.Nullable_aggregate_over_partition_without_values();

        AssertSql(
            """
SELECT [e].[DepartmentName], [e].[Id], [e].[Bonus], [e].[Name], [e].[Salary], AVG(CAST([e].[Bonus] AS float)) OVER(PARTITION BY [e].[DepartmentName])
FROM [Employees] AS [e]
ORDER BY [e].[DepartmentName]
""");
    }

    public override async Task Elements_and_aggregate_without_opt_in()
    {
        await base.Elements_and_aggregate_without_opt_in();

        AssertSql(
            """
SELECT [e1].[DepartmentName], [e0].[Id], [e0].[Bonus], [e0].[DepartmentName], [e0].[Name], [e0].[Salary], [e1].[c]
FROM (
    SELECT [e].[DepartmentName], AVG(CAST([e].[Salary] AS float)) AS [c]
    FROM [Employees] AS [e]
    GROUP BY [e].[DepartmentName]
) AS [e1]
LEFT JOIN [Employees] AS [e0] ON [e1].[DepartmentName] = [e0].[DepartmentName]
ORDER BY [e1].[DepartmentName]
""");
    }

    public override async Task Aggregate_without_elements()
    {
        await base.Aggregate_without_elements();

        AssertSql(
            """
SELECT [e].[DepartmentName] AS [Key], AVG(CAST([e].[Salary] AS float)) AS [AverageSalary]
FROM [Employees] AS [e]
GROUP BY [e].[DepartmentName]
ORDER BY [e].[DepartmentName]
""");
    }

    public override async Task Elements_without_aggregate()
    {
        await base.Elements_without_aggregate();

        AssertSql(
            """
SELECT [e].[DepartmentName], [e].[Id], [e].[Bonus], [e].[Name], [e].[Salary]
FROM [Employees] AS [e]
ORDER BY [e].[DepartmentName]
""");
    }

    public override async Task Distinct_aggregate_is_not_lifted()
    {
        await base.Distinct_aggregate_is_not_lifted();

        AssertSql(
            """
SELECT [e1].[DepartmentName], [e0].[Id], [e0].[Bonus], [e0].[DepartmentName], [e0].[Name], [e0].[Salary], [e1].[c]
FROM (
    SELECT [e].[DepartmentName], COUNT(DISTINCT ([e].[Salary])) AS [c]
    FROM [Employees] AS [e]
    GROUP BY [e].[DepartmentName]
) AS [e1]
LEFT JOIN [Employees] AS [e0] ON [e1].[DepartmentName] = [e0].[DepartmentName]
ORDER BY [e1].[DepartmentName]
""");
    }

    public override async Task Filtered_aggregate_is_not_lifted()
    {
        await base.Filtered_aggregate_is_not_lifted();

        AssertSql(
            """
SELECT [e1].[DepartmentName], [e0].[Id], [e0].[Bonus], [e0].[DepartmentName], [e0].[Name], [e0].[Salary], [e1].[c]
FROM (
    SELECT [e].[DepartmentName], COUNT(CASE
        WHEN [e].[Salary] > 100 THEN 1
    END) AS [c]
    FROM [Employees] AS [e]
    GROUP BY [e].[DepartmentName]
) AS [e1]
LEFT JOIN [Employees] AS [e0] ON [e1].[DepartmentName] = [e0].[DepartmentName]
ORDER BY [e1].[DepartmentName]
""");
    }
}
