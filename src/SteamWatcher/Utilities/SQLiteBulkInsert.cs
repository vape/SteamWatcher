using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Utilities
{
    // http://procbits.com/2009/09/08/sqlite-bulk-insert
    public class SQLiteBulkInsert
    {
        public uint CommitMax
        {
            get;
            set;
        }

        public string TableName
        {
            get;
            private set;
        }

        public string ParamDelimiter
        {
            get;
            private set;
        }

        private SQLiteConnection connection;
        private SQLiteCommand command;
        private SQLiteTransaction transaction;
        private Dictionary<string, object> parameters = new Dictionary<string, object>();

        private string beginInsertText;
        private uint counter;

        public SQLiteBulkInsert(SQLiteConnection dbConnection, string tableName)
        {
            connection = dbConnection;
            TableName = tableName;
            ParamDelimiter = "@";

            StringBuilder query = new StringBuilder(255);
            query.Append("INSERT INTO [");
            query.Append(tableName);
            query.Append("] (");
            beginInsertText = query.ToString();
        }

        public string CommandText
        {
            get
            {
                if (parameters.Count == 0)
                {
                    throw new SQLiteException("You must add at least one parameter.");
                }

                StringBuilder sb = new StringBuilder(255);
                sb.Append(beginInsertText);

                foreach (string param in parameters.Keys)
                {
                    sb.Append('[');
                    sb.Append(param);
                    sb.Append(']');
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2);

                sb.Append(") VALUES (");

                foreach (string param in parameters.Keys)
                {
                    sb.Append(ParamDelimiter);
                    sb.Append(param);
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2);

                sb.Append(")");

                return sb.ToString();
            }
        }

        public void AddParameter(string name, DbType dbType)
        {
            SQLiteParameter param = new SQLiteParameter(ParamDelimiter + name, dbType);
            parameters.Add(name, param);
        }

        public void Flush()
        {
            try
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not commit transaction. See InnerException for more details", ex);
            }
            finally
            {
                if (transaction != null)
                    transaction.Dispose();

                transaction = null;
                counter = 0;
            }
        }

        public void Insert(object[] paramValues)
        {
            if (paramValues.Length != parameters.Count)
            {
                throw new Exception("The values array count must be equal to the count of the number of parameters.");
            }

            counter++;

            if (counter == 1)
            {
                transaction = connection.BeginTransaction();
                command = connection.CreateCommand();

                foreach (SQLiteParameter par in parameters.Values)
                {
                    command.Parameters.Add(par);
                }

                command.CommandText = CommandText;
            }

            int i = 0;
            foreach (SQLiteParameter par in parameters.Values)
            {
                par.Value = paramValues[i];
                i++;
            }

            command.ExecuteNonQuery();

            if (counter == CommitMax)
            {
                try
                {
                    if (transaction != null)
                        transaction.Commit();
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Dispose();
                        transaction = null;
                    }

                    counter = 0;
                }
            }
        }
    }
}
