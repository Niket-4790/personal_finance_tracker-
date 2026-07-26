using Dapper;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReportRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DashboardSummary?> GetDashboardSummaryAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<DashboardSummary>(
            "dbo.usp_Dashboard_GetSummary",
            new { UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<MonthlyReport>> GetMonthlyTotalsAsync(int userId, int monthsBack = 6)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<MonthlyReport>(
            "dbo.usp_Report_GetMonthlyTotals",
            new { UserId = userId, MonthsBack = monthsBack },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAsync(int userId, int? month = null, int? year = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<CategoryBreakdown>(
            "dbo.usp_Report_GetCategoryBreakdown",
            new { UserId = userId, Month = month, Year = year },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<decimal> GetSavingsRateAsync(int userId, int? month = null, int? year = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        parameters.Add("@Month", month);
        parameters.Add("@Year", year);
        parameters.Add("@SavingsRate", dbType: System.Data.DbType.Decimal, direction: System.Data.ParameterDirection.Output, precision: 10, scale: 2);

        await connection.ExecuteAsync(
            "dbo.usp_Report_CalculateSavingsRate",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<decimal>("@SavingsRate");
    }

    public async Task<SystemSummary?> GetSystemSummaryAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<SystemSummary>(
            "dbo.usp_Dashboard_GetSystemSummary",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<MonthlyReport>> GetMonthlyTotalsAllUsersAsync(int monthsBack = 6)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<MonthlyReport>(
            "dbo.usp_Report_GetMonthlyTotalsAllUsers",
            new { MonthsBack = monthsBack },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CategoryBreakdown>> GetCategoryBreakdownAllUsersAsync(int? month = null, int? year = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<CategoryBreakdown>(
            "dbo.usp_Report_GetCategoryBreakdownAllUsers",
            new { Month = month, Year = year },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
