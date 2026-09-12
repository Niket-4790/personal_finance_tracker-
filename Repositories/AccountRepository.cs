using Dapper;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AccountRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Account>> GetAllAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Account>(
            "dbo.usp_Account_GetAll",
            new { UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Account?> GetByIdAsync(int id, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Account>(
            "dbo.usp_Account_GetById",
            new { Id = id, UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> InsertAsync(Account account)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", account.UserId);
        parameters.Add("@Name", account.Name);
        parameters.Add("@AccountType", account.AccountType);
        parameters.Add("@InitialBalance", account.Balance);
        parameters.Add("@NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.usp_Account_Insert",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }

    public async Task UpdateAsync(Account account)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "dbo.usp_Account_Update",
            new { account.Id, account.UserId, account.Name, account.AccountType },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "dbo.usp_Account_Delete",
            new { Id = id, UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
