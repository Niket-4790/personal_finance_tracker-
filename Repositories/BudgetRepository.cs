using Dapper;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BudgetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Budget>> GetAllAsync(
        int userId,
        DateTime month)
    {
        using var connection = _connectionFactory.CreateConnection();

        var budgetMonth = new DateTime(
            month.Year,
            month.Month,
            1);

        var budgets = await connection.QueryAsync<Budget>(
            "dbo.usp_Budget_GetAll",
            new
            {
                UserId = userId,
                BudgetMonth = budgetMonth
            },
            commandType: System.Data.CommandType.StoredProcedure);

        foreach (var budget in budgets)
        {
            budget.RemainingAmount =
                budget.Amount - budget.SpentAmount;

            budget.PercentageUsed =
                budget.Amount > 0
                    ? (budget.SpentAmount / budget.Amount) * 100
                    : 0;
        }

        return budgets;
    }

    public async Task<Budget?> GetByIdAsync(
        int id,
        int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var budget = await connection.QueryFirstOrDefaultAsync<Budget>(
            "dbo.usp_Budget_GetById",
            new
            {
                Id = id,
                UserId = userId
            },
            commandType: System.Data.CommandType.StoredProcedure);

        if (budget is null)
        {
            return null;
        }

        budget.RemainingAmount =
            budget.Amount - budget.SpentAmount;

        budget.PercentageUsed =
            budget.Amount > 0
                ? (budget.SpentAmount / budget.Amount) * 100
                : 0;

        return budget;
    }

    public async Task<int> InsertAsync(Budget budget)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@UserId", budget.UserId);
        parameters.Add("@CategoryId", budget.CategoryId);
        parameters.Add("@BudgetMonth", new DateTime(
            budget.BudgetMonth.Year,
            budget.BudgetMonth.Month,
            1));
        parameters.Add("@Amount", budget.Amount);

        parameters.Add(
            "@NewId",
            dbType: System.Data.DbType.Int32,
            direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.usp_Budget_Insert",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }

    public async Task UpdateAsync(Budget budget)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Budget_Update",
            new
            {   
                Id= budget.Id,
                UserId = budget.UserId,
                CategoryId = budget.CategoryId,
                BudgetMonth = new DateTime(
                    budget.BudgetMonth.Year,
                    budget.BudgetMonth.Month,
                    1),
                Amount = budget.Amount
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(
        int id,
        int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Budget_Delete",
            new
            {
                Id = id,
                UserId = userId
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
