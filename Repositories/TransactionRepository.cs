using Dapper;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Transaction>(
            "dbo.usp_Transaction_GetAll",
            new { UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Transaction?> GetByIdAsync(int id, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Transaction>(
            "dbo.usp_Transaction_GetById",
            new { Id = id, UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> InsertAsync(Transaction transaction)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", transaction.UserId);
        parameters.Add("@AccountId", transaction.AccountId);
        parameters.Add("@CategoryId", transaction.CategoryId);
        parameters.Add("@Amount", transaction.Amount);
        parameters.Add("@Description", transaction.Description);
        parameters.Add("@TransactionDate", transaction.TransactionDate);
        parameters.Add("@TransactionType", transaction.TransactionType);
        parameters.Add("@NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.usp_Transaction_Insert",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "dbo.usp_Transaction_Update",
            new
            {
                transaction.Id,
                transaction.UserId,
                transaction.AccountId,
                transaction.CategoryId,
                transaction.Amount,
                transaction.Description,
                transaction.TransactionDate,
                transaction.TransactionType
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "dbo.usp_Transaction_Delete",
            new { Id = id, UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Transaction>> GetAllForAdminAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Transaction>(
            "dbo.usp_Transaction_GetAllForAdmin",
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
