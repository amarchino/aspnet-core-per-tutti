using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.Options;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.Services.Infrastructure
{
    public class SqliteDatabaseAccessor : IDatabaseAccessor
    {
        private readonly IOptionsMonitor<ConnectionStringsOptions> connectionStringsOptions;
        private readonly ILogger<SqliteDatabaseAccessor> logger;
        public SqliteDatabaseAccessor(IOptionsMonitor<ConnectionStringsOptions> connectionStringsOptions, ILogger<SqliteDatabaseAccessor> logger)
        {
            this.logger = logger;
            this.connectionStringsOptions = connectionStringsOptions;
        }

        public async Task<DataSet> QueryAsync(FormattableString formattableQuery, CancellationToken token = default(CancellationToken))
        {
            logger.LogDebug(formattableQuery.Format, formattableQuery.GetArguments());

            using SqliteConnection conn = await GetOpenedConnection(token);
            using SqliteCommand cmd = GetCommand(formattableQuery, conn);

            using var reader = await cmd.ExecuteReaderAsync(token);
            var dataSet = new DataSet();

            do
            {
                var dataTable = new DataTable();
                dataSet.Tables.Add(dataTable);
                dataTable.Load(reader);
            } while (!reader.IsClosed);

            return dataSet;
        }

        private static SqliteCommand GetCommand(FormattableString formattableQuery, SqliteConnection conn)
        {
            var queryArguments = formattableQuery.GetArguments();
            var sqliteParameters = new List<SqliteParameter>();
            for (var i = 0; i < queryArguments.Length; i++)
            {
                if (queryArguments[i] is Sql)
                {
                    continue;
                }
                var parameter = new SqliteParameter(i.ToString(), queryArguments[i] ?? DBNull.Value);
                sqliteParameters.Add(parameter);
                queryArguments[i] = "@" + i;
            }
            string query = formattableQuery.ToString();

            var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddRange(sqliteParameters);
            return cmd;
        }

        private async Task<SqliteConnection> GetOpenedConnection(CancellationToken token)
        {
            var conn = new SqliteConnection(connectionStringsOptions.CurrentValue.Default);
            await conn.OpenAsync(token);
            return conn;
        }

        public async Task<int> CommandAsync(FormattableString formattableCommand, CancellationToken token = default(CancellationToken))
        {
            using SqliteConnection conn = await GetOpenedConnection(token);
            using SqliteCommand cmd = GetCommand(formattableCommand, conn);

            try
            {
                int affectedRows = await cmd.ExecuteNonQueryAsync(token);
                return affectedRows;
            }
            catch (SqliteException exc) when (exc.SqliteErrorCode == 19)
            {
                throw new ConstraintViolationException(exc);
            }
        }

        public async Task<T> QueryScalarAsync<T>(FormattableString formattableQuery, CancellationToken token = default(CancellationToken))
        {
            using SqliteConnection conn = await GetOpenedConnection(token);
            using SqliteCommand cmd = GetCommand(formattableQuery, conn);
            object result = await cmd.ExecuteScalarAsync(token);
            return (T) Convert.ChangeType(result, typeof(T));
        }
    }
}
