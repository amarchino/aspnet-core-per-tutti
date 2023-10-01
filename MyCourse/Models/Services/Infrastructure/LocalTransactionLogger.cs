using MyCourse.Models.InputModels.Courses;

namespace MyCourse.Models.Services.Infrastructure;
public class LocalTransactionLogger : ITransactionLogger
{
    private readonly IHostEnvironment env;
    private readonly SemaphoreSlim semaphore = new(1);

    public LocalTransactionLogger(IHostEnvironment env)
    {
        this.env = env;
    }

    public async Task LogTransactionAsync(CourseSubscribeInputModel inputModel)
    {
        string filePath = Path.Combine(env.ContentRootPath, "Data", "transactions.txt");
        string content = $"\r\n{inputModel.TransactionId}\t{inputModel.PaymentDate}\t{inputModel.PaymentType}\t{inputModel.CourseId}\t{inputModel.UserId}\t{inputModel.Paid.Amount}\t{inputModel.Paid.Currency}";
        try
        {
            await semaphore.WaitAsync();
            await File.AppendAllTextAsync(filePath, content);
        }
        finally
        {
            semaphore.Release();
        }
        return;
    }
}
