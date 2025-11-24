using System;
using System.Collections.Generic;
using System.Data.Common;
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
            if (builder.ContainsKey("Database"))
            {
                return builder["Database"]?.ToString();
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
            if (!DbUtils.GetDatabaseName(connectionstring).Equals(nameDb, StringComparison.InvariantCultureIgnoreCase))
                return false;
#pragma warning disable CS0618 // Review SQL queries for security vulnerabilities
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionstring))
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
    }
}
