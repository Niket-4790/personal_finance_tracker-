using Dapper;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(string name, string email, string passwordHash, string role)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Name", name);
        parameters.Add("@Email", email);
        parameters.Add("@PasswordHash", passwordHash);
        parameters.Add("@Role", role);
        parameters.Add("@NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.usp_User_Create",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }

    public async Task<UserCredential?> GetByEmailWithHashAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<UserCredential>(
            "dbo.usp_User_GetByEmail",
            new { Email = email },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<AppUser?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<AppUser>(
            "dbo.usp_User_GetById",
            new { Id = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<AppUser>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<AppUser>(
            "dbo.usp_User_GetAll",
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task SetRoleAsync(int id, string role)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "dbo.usp_User_SetRole",
            new { Id = id, Role = role },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "dbo.usp_User_SetActive",
            new { Id = id, IsActive = isActive },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> GetAdminCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Count", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.usp_User_GetAdminCount",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@Count");
    }

    public async Task<int> GetActiveAdminCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Count", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.usp_User_GetActiveAdminCount",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@Count");
    }

    public async Task<AppUser?> GetByGoogleSubjectIdAsync(
    string googleSubjectId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<AppUser>(
            "dbo.usp_User_GetByGoogleSubjectId",
            new { GoogleSubjectId = googleSubjectId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CreateGoogleUserAsync(
    string name,
    string email,
    string googleSubjectId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@Name", name);
        parameters.Add("@Email", email);
        parameters.Add("@GoogleSubjectId", googleSubjectId);

        parameters.Add(
            "@NewId",
            dbType: System.Data.DbType.Int32,
            direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(
            "dbo.usp_User_CreateGoogle",
            parameters,
            commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }

    public async Task SetGoogleSubjectIdAsync(
   int userId,
   string googleSubjectId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_User_SetGoogleSubjectId",
            new
            {
                Id = userId,
                GoogleSubjectId = googleSubjectId
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}


