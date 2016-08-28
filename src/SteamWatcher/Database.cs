using SteamWatcher.Data;
using SteamWatcher.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher
{
    public class Database : IDisposable
    {
        public static void Create(string fileName)
        {
            SQLiteConnection.CreateFile(fileName);
            using (var connection = Connect(fileName))
            {
                var query =
                @"create table apps          (appid int, name varchar(1024));
                  create table prices        (appid int, price int, discount int, 
                                              last_update int);
                  create table price_changes (appid int, p_prev int, p_new int, 
                                              d_prev int, d_new int, updated_time int)";

                var command = new SQLiteCommand(query, connection);
                command.ExecuteNonQuery();
            }
        }

        private static SQLiteConnection Connect(string fileName)
        {
            var connection = new SQLiteConnection($"Data Source={fileName};Version=3;");
            connection.Open();

            return connection;
        }

        private SQLiteConnection connection;

        public Database(string fileName = null)
        {
            if (fileName != null && !File.Exists(fileName))
            {
                throw new FileNotFoundException("Database file not exists.", fileName);
            }

            connection = Connect(fileName ?? Program.Config.DatabaseFile);
        }

        #region Executions

        private int Execute(SQLiteCommand command)
        {
            using (command)
            {
                return command.ExecuteNonQuery();
            }
        }

        private int Execute(string query, params SQLiteParameter[] parameters)
        {
            return Execute(CreateCommand(query, parameters));
        }

        private T ExecuteScalar<T>(SQLiteCommand command)
        {
            using (command)
            {
                var result = command.ExecuteScalar();
                if (result == null)
                {
                    return default(T);
                }

                return (T)result;
            }
        }

        private T ExecuteScalar<T>(string query, params SQLiteParameter[] parameters)
        {
            return ExecuteScalar<T>(CreateCommand(query, parameters));
        }

        private DataTable ExecuteSelect(SQLiteCommand command)
        {
            var dataTable = new DataTable();
            var dataAdapter = new SQLiteDataAdapter();
            var commandBuilder = new SQLiteCommandBuilder(dataAdapter);

            using (command)
            {
                dataAdapter.SelectCommand = command;
                dataAdapter.Fill(dataTable);

                return dataTable;
            }
        }

        private DataTable ExecuteSelect(string query, params SQLiteParameter[] parameters)
        {
            return ExecuteSelect(CreateCommand(query, parameters));
        }

        private SQLiteCommand CreateCommand(string query)
        {
            var command = new SQLiteCommand();

            command.Connection = connection;
            command.CommandText = query;

            return command;
        }

        private SQLiteCommand CreateCommand(string query, params SQLiteParameter[] parameters)
        {
            var command = new SQLiteCommand();

            command.Connection = connection;
            command.CommandText = query;

            foreach (var p in parameters)
            {
                command.Parameters.Add(p);
            }

            return command;
        }

        #endregion

        public bool AppInfoExists(int appId)
        {
            return ExecuteScalar<long>($"select exists(select 1 from apps where appid={appId})") != 0;
        }

        public bool PriceInfoExists(int appId)
        {
            return ExecuteScalar<long>($"select exists(select 1 from prices where appid={appId})") != 0;
        }

        public void InsertAppInfo(AppInfo app)
        {
            var query = $"insert into apps (appid, name) values ({app.AppID}, @{nameof(app.Name)})";
            Execute(query, new SQLiteParameter($"@{nameof(app.Name)}", app.Name));
        }

        public void InsertAppsInfo(AppInfo[] apps)
        {
            var bulkInsert = new SQLiteBulkInsert(connection, "apps");
            bulkInsert.AddParameter("appid", DbType.Int32);
            bulkInsert.AddParameter("name", DbType.String);

            for (int i = 0; i < apps.Length; ++i)
            {
                bulkInsert.Insert(new object[] { apps[i].AppID, apps[i].Name });
            }

            bulkInsert.Flush();
        }

        public void DeleteAppsInfo()
        {
            var query = "delete from apps";
            Execute(query);
        }

        public AppInfo? SelectAppInfo(int appId)
        {
            var query = $"select * from apps where appid={appId}";
            var selected = ExecuteSelect(query);

            if (selected.Rows.Count == 0)
            {
                return null;
            }

            var row = selected.Rows[0];
            return new AppInfo((int)row["appid"], (string)row["name"]);
        }

        public PriceInfo? SelectPriceInfo(int appId)
        {
            var query = $"select * from prices where appid={appId}";
            var selected = ExecuteSelect(query);

            if (selected.Rows.Count == 0)
            {
                return null;
            }

            var row = selected.Rows[0];
            return new PriceInfo((int)row["appid"], (int)row["price"], (int)row["discount"],
                                 ((int)row["last_update"]).FromUnixTime());
        }

        public void InsertPriceInfo(PriceInfo info)
        {
            var query = $@"insert into prices (appid, price, discount, last_update)
                           values ({info.AppID}, {info.Price}, {info.Discount}, 
                                   {(int)info.Updated.ToUnixTime()})";
            Execute(query);
        }

        public void UpdatePriceInfo(PriceInfo info)
        {
            var query = $@"update prices set price={info.Price},discount={info.Discount},
                                             last_update={(int)info.Updated.ToUnixTime()}
                           where appid={info.AppID}";
            Execute(query);
        }

        public void InsertPriceChange(PriceChange priceChange)
        {
            var query = $@"insert into price_changes (appid, p_prev, p_new, d_prev, d_new, updated_time)
                           values ({priceChange.AppID}, {priceChange.Previous.Price}, 
                                   {priceChange.New.Price}, {priceChange.Previous.Discount},
                                   {priceChange.New.Discount}, {(int)priceChange.Updated.ToUnixTime()})";
            Execute(query);
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
