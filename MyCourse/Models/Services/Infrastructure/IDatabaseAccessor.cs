using System.Data;

namespace MyCourse.Models.Services.Infrastructure;
public interface IDatabaseAccessor
{
    Task<DataSet> QueryAsync(FormattableString query, CancellationToken token = default(CancellationToken));
    Task<T> QueryScalarAsync<T>(FormattableString query, CancellationToken token = default(CancellationToken));
    Task<int> CommandAsync(FormattableString command, CancellationToken token = default(CancellationToken));
}
