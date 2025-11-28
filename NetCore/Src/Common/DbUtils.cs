using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Provar clau "Initial Catalog"
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
            return null;
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
        public static bool DatabaseExists(string connectionstring)
        {
            string nameDb = "Verifactu";
            string namedbInConnection = DbUtils.GetDatabaseName(connectionstring);
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
                using (var command = new System.Data.SqlClient.SqlCommand($"CREATE TABLE {tablename} (id INT IDENTITY(1,1) PRIMARY KEY, Valores NVARCHAR(300) NOT NULL);", connection))

                {
                    var result = command.ExecuteScalar();
                    return (result != null);
                }
            }
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
                using (var command = new System.Data.SqlClient.SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = 'Id%' ORDER BY TABLE_NAME", connection))
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

        internal static bool Update(string connectionstring, string blockchainVarName,ulong id, string value)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionstring))
            {
                connection.Open();
                if (TableExists(connectionstring, blockchainVarName))
                {
                    return false;
                }
                using (var command = new System.Data.SqlClient.SqlCommand($"INSERT INTO {blockchainVarName} (id, Valores) values ({id},{value} ", connection))
                {
                    command.Connection = connection;
                    var result = (int)command.ExecuteScalar();
                    return result > 0;
                }
            }
        }

        internal static string Read(string connectionstring, string blockchainVarName)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionstring))
            {
                using (var command = new System.Data.SqlClient.SqlCommand($"SELECT * FROM {blockchainVarName}", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        return reader.GetString(1);
                        
                    }
                }
            }
        }
    }
}
