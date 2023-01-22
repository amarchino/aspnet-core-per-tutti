using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyCourse.Models.Options;

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

        public async Task<DataSet> QueryAsync(FormattableString formattableQuery)
        {
            logger.LogDebug(formattableQuery.Format, formattableQuery.GetArguments());
            var queryArguments = formattableQuery.GetArguments();
            var sqliteParameters = new List<SqliteParameter>();
            for(var i = 0; i < queryArguments.Length; i++)
            {
                var parameter = new SqliteParameter(i.ToString(), queryArguments[i]);
                sqliteParameters.Add(parameter);
                queryArguments[i] = "@" + i;
            }
            string query = formattableQuery.ToString();

            using (var conn = new SqliteConnection(connectionStringsOptions.CurrentValue.Default))
            {
                await conn.OpenAsync();
                using(var cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddRange(sqliteParameters);
                    using(var reader = await cmd.ExecuteReaderAsync())
                    {
                        var dataSet = new DataSet();
                        dataSet.EnforceConstraints = false;

                        do
                        {
                            var dataTable = new DataTable();
                            dataSet.Tables.Add(dataTable);
                            dataTable.Load(reader);
                        } while(!reader.IsClosed);

                        return dataSet;
                    }
                }
            }
        }
    }
}
