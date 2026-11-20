using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VeriFactu.Config;
using VeriFactu.Net.Rest.Json.Parser.Lexer.Tokens;

namespace VeriFactu.Common
{
    /// <summary>
    /// Algunas utilidades de base de datos.
    /// </summary>
    public static class DbUtils
    {
        public static string GetServerName(string connectionString)
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            // Provar clau "Data Source"
            if (builder.ContainsKey("Data Source"))
            {
                return builder["Data Source"]?.ToString();
            }

            // Provar clau "Database"
            if (builder.ContainsKey("Database"))
            {
                return builder["Database"]?.ToString();
            }

            // No hi ha base de dades especificada
            return string.Empty;
        }
        /// <summary>
        /// Verifica si una cadena de conexión contiene el nombre de la base de datos
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static string GetDatabaseName(string connectionString)
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            // Provar clau "Initial Catalog"
            if (builder.ContainsKey("Initial Catalog"))
            {
                return builder["Initial Catalog"]?.ToString();
            }
            // Provar clau "Provider"
            if (builder.ContainsKey("Provider"))
            {
                return builder["Provider"]?.ToString();
            }
            // Provar clau "Database"
            if (builder.ContainsKey("Server"))
            {
                return builder["Server"]?.ToString();
            }

            // No hi ha base de dades especificada
            return null;
        }
        /// <summary>
        /// Verifica si "Verifactu" existe en la base de datos especificada en la cadena de conexión.
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <returns></returns>
        public static bool DatabaseSqlExists(string connectionstring)
        {
            string nameDb = "Verifactu";
            string namedbInConnection = DbUtils.GetDatabaseName(connectionstring);
            if (namedbInConnection is null)
                return false;
            if ( !namedbInConnection.Equals(nameDb, StringComparison.InvariantCultureIgnoreCase))
                return false;
#pragma warning disable CS0618 // Review SQL queries for security vulnerabilities
            string server = GetServerName(connectionstring);
            string masterConnectionString = $"Server={server};Integrated Security=True;";
            using (var connection = new System.Data.SqlClient.SqlConnection(masterConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand("SELECT database_id FROM sys.databases WHERE name = @name", connection))

                {
                    command.Parameters.AddWithValue("@name", nameDb);
                    var result = command.ExecuteScalar();
                    return (result != null);
                }
            }
#pragma warning restore CS0618 // Review SQL queries for security vulnerabilities
        }
        /// <summary>
        /// Verifica si "Verifactu" existe en la base de datos especificada en la cadena de conexión.
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <returns></returns>
        public static bool DatabaseMdbExists(string connectionstring)
        {
            string server = GetServerName(connectionstring);
            if (!File.Exists(server))
                return false;
            else
                return true;
        }
        /// <summary>
        /// CREA LA BASE DE DATOS "Verifactu" en la base de datos especificada en la cadena de conexión.
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <returns></returns>
        public static bool CreateDatabase(string connectionstring)
        {
            string nameDb = "Verifactu";
            if (!DbUtils.GetDatabaseName(connectionstring).Equals(nameDb, StringComparison.InvariantCultureIgnoreCase))
                return false;
#pragma warning disable CS0618 // Review SQL queries for security vulnerabilities
            string server = GetServerName(connectionstring);
            string masterConnectionString = $"Server={server};Integrated Security=True;";
            using (var connection = new System.Data.SqlClient.SqlConnection(masterConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand($"CREATE DATABASE {nameDb}", connection))

                {
                    try
                    {
                     command.ExecuteScalar();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <returns></returns>
        public static bool CreateDatabaseMdb(string connectionstring, string password)
        {
#if LE_461 || LE_472 || LE_480
            const string nameDb = "Verifactu";

            var builder = new OleDbConnectionStringBuilder(connectionstring);
            string path = builder.DataSource;

            if (string.IsNullOrWhiteSpace(path) ||
                !Path.GetFileNameWithoutExtension(path)
                     .Equals(nameDb, StringComparison.InvariantCultureIgnoreCase))
                return false;

            try
            {
                if (File.Exists(path))
                    return true;

                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                Type catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
                if (catalogType == null)
                    return false;

                string engine = path.EndsWith(".mdb", StringComparison.OrdinalIgnoreCase)
                    ? ";Jet OLEDB:Engine Type=5"
                    : "";

                string pwd = string.IsNullOrEmpty(password)
                    ? ""
                    : $";Jet OLEDB:Database Password={password}";

                dynamic catalog = Activator.CreateInstance(catalogType);
                try
                {
                    catalog.Create(
                        $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path}{engine}{pwd};");
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(catalog);
                }

                return File.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
#else
    return false;
       
#endif
        }     /// <summary>
            /// Crea la tabla en la base de datos
            /// </summary>
            /// <param name="connectionstring"></param>
            /// <param name="tablename"></param>
            /// <returns></returns>
        public static bool CreateTable(string connectionstring, string tablename)
        {
#pragma warning disable CS0618 // Review SQL queries for security vulnerabilities
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionstring))
            {
                connection.Open();
                //using (var command = new System.Data.SqlClient.SqlCommand($"CREATE TABLE [{tablename}] ([id] INT IDENTITY(1,1) PRIMARY KEY, Valores NVARCHAR(300) NOT NULL);", connection))
                using (var command = new System.Data.SqlClient.SqlCommand($"CREATE TABLE [{tablename}] ([id] INT PRIMARY KEY, Valores NVARCHAR(300) NOT NULL);", connection))

                {
                    var result = command.ExecuteScalar();
                    return (result != null);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public static bool CreateTableMdb(string connectionstring, string tablename)
        {
#if LE_461 || LE_472 || LE_480
            // El nom de taula no es pot parametritzar: el validem
            if (string.IsNullOrWhiteSpace(tablename) ||
                !Regex.IsMatch(tablename, @"^[A-Za-z0-9_]+$"))
                return false;

            try
            {
                using (var connection = new OleDbConnection(connectionstring))
                {
                    connection.Open();

                    // Si ja existeix, no la tornem a crear
                    var schema = connection.GetOleDbSchemaTable(
                        OleDbSchemaGuid.Tables,
                        new object[] { null, null, tablename, "TABLE" });
                    if (schema != null && schema.Rows.Count > 0)
                        return true;

                    string sql = $"CREATE TABLE [{tablename}] ([id] LONG PRIMARY KEY, [Valores] LONGTEXT NOT NULL)";
                    using (var command = new OleDbCommand(sql, connection))
                    {
                        command.ExecuteNonQuery(); // si falla, llança excepció
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
#else
            return false;
#endif
        }
        /// <summary>
        /// Devuelve todas las tablas de la base de datos especificada en la cadena de conexión.
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <returns></returns>
        public static List<string> GetDatabaseTablesOfId(string connectionstring)
        {
            var tables = new List<string>();
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionstring))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME LIKE 'Blockchain%' ORDER BY TABLE_NAME", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return tables;
        }
        /// <summary>
        /// Devuelve todas las tablas de la base de datos especificada en la cadena de conexión.
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <returns></returns>
        public static List<string> GetDatabaseTablesMdbOfId(string connectionstring)
        {
            var tables = new List<string>();
#if LE_480 || LE_472 || LE_461
            using (var connection = new OleDbConnection(connectionstring))
            {
                connection.Open();

                DataTable schema = connection.GetOleDbSchemaTable(
                    OleDbSchemaGuid.Tables,
                    new object[] { null, null, null, "TABLE" });

                if (schema != null)
                {
                    tables = schema.Rows.Cast<DataRow>()
                        .Select(r => r["TABLE_NAME"].ToString())
                        .Where(n => n.StartsWith("Blockchain", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }
#endif
            return tables;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool TableExists(string connectionstring, string tableName)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionstring))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = @tableName", connection))
                {
                    command.Parameters.AddWithValue("@tableName", tableName);
                    var result = (int)command.ExecuteScalar();
                    return result > 0;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionstring"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool TableMdbExists(string connectionstring, string tableName)
        {
#if LE_461 || LE_472 || LE_480
            using (var connection = new OleDbConnection(connectionstring))
            {
                connection.Open();
                DataTable schema = connection.GetOleDbSchemaTable(
                OleDbSchemaGuid.Tables,
                new object[] { null, null, tableName, "TABLE" });

                return schema != null && schema.Rows.Count > 0;
            }
#else
            return false;
#endif

        }
        internal static bool Update(string connectionstring, string blockchainVarName,ulong id, string value)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionstring))
            {
                connection.Open();
                if (!TableExists(connectionstring, blockchainVarName))
                {
                    return false;
                }
                using (var command = new System.Data.SqlClient.SqlCommand($"INSERT INTO [{blockchainVarName}] ([id], [Valores]) values ({id},{value})", connection))
                {
                    command.Connection = connection;
                    var result = (int)command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }
        internal static bool UpdateMdb(string connectionstring, string blockchainVarName, ulong id, string value)
        {
#if LE_461 || LE_472 || LE_480
            using (var connection = new OleDbConnection(connectionstring))
            {
                connection.Open();
                if (!TableMdbExists(connectionstring, blockchainVarName))
                {
                    return false;
                }
                using (var command = new OleDbCommand($"INSERT INTO [{blockchainVarName}] ([id], [Valores]) values ({id},{value})", connection))
                {
                    command.Connection = connection;
                    var result = (int)command.ExecuteNonQuery();
                    return result > 0;
                }
            }
#else
    return false;      
#endif
        }
        internal static string Read(string connectionstring, string blockchainVarName)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionstring))
            {
             connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand($"SELECT TOP(1) * FROM [{blockchainVarName}] ORDER BY id DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                       var resultvalue = string.Empty;
                        if (reader.Read())
                        {
                         resultvalue = reader.GetInt32(0) + ";" + reader.GetString(1);
                        }

                        return resultvalue;
                    }
                }
            }
        }
        internal static string ReadMdb(string connectionstring, string blockchainVarName)
        {
#if LE_461 || LE_472 || LE_480
            using (var connection = new OleDbConnection(connectionstring))
            {
                connection.Open();
                using (var command = new OleDbCommand($"SELECT TOP(1) * FROM [{blockchainVarName}] ORDER BY id DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var resultvalue = string.Empty;
                        if (reader.Read())
                        {
                            resultvalue = reader.GetInt32(0) + ";" + reader.GetString(1);
                        }

                        return resultvalue;
                    }
                }
            }
#else
            return string.Empty;
#endif
        }
    }
}
