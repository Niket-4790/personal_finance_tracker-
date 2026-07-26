using Dapper;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Category>(
            "dbo.usp_Category_GetAll",
            new { UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Category?> GetByIdAsync(int id, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Category>(
            "dbo.usp_Category_GetById",
            new { Id = id, UserId = userId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> InsertAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", category.UserId);
        parameters.Add("@Name", category.Name);
        parameters.Add("@Type", category.Type);
        parameters.Add("@Color", category.Color);
        parameters.Add("@NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.usp_Category_Insert",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }
}
