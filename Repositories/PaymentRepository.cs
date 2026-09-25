using Dapper;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using System.Data;

namespace PersonalFinanceTracker.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PaymentRepository(
        IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(Payment payment)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            "dbo.usp_Payment_Insert",
            new
            {
                payment.UserId,
                payment.RazorpayOrderId,
                payment.Amount,
                payment.Status
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Payment?> GetLatestPaidPaymentAsync(
        int userId)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Payment>(
            "dbo.usp_Payment_GetLatestPaid",
            new
            {
                UserId = userId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Payment?> GetByOrderIdAsync(
        string razorpayOrderId)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Payment>(
            "dbo.usp_Payment_GetByOrderId",
            new
            {
                RazorpayOrderId = razorpayOrderId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task MarkAsPaidAsync(
        string razorpayOrderId,
        string razorpayPaymentId)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Payment_MarkAsPaid",
            new
            {
                RazorpayOrderId = razorpayOrderId,
                RazorpayPaymentId = razorpayPaymentId
            },
            commandType: CommandType.StoredProcedure);
    }
}