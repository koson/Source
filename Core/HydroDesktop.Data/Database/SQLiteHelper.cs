﻿using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace HydroDesktop.Database
{
    /// <summary>
    /// This class contains methods for working with the
    /// SQLite database
    /// </summary>
    public static class SQLiteHelper
    {
        /// <summary>
        /// To get the SQLite database path given the SQLite connection string
        /// </summary>
        public static string GetSQLiteFileName(string sqliteConnString)
        {
            if (String.IsNullOrEmpty(sqliteConnString))
                throw new ArgumentException("sqliteConnString is null or empty.", "sqliteConnString");

            var conn = new SQLiteConnectionStringBuilder(sqliteConnString);
            return conn.DataSource;
        }
        /// <summary>
        /// To get the full SQLite connection string given the SQLite database path
        /// </summary>
        public static string GetSQLiteConnectionString(string dbFileName)
        {
            if (String.IsNullOrEmpty(dbFileName))
                throw new ArgumentException("dbFileName is null or empty.", "dbFileName");

            var conn = new SQLiteConnectionStringBuilder { DataSource = dbFileName, Version = 3, FailIfMissing = true };
            conn.Add("Compress", true);

            return conn.ConnectionString;

        }

        /// <summary>
        /// Create the default .SQLITE database in the user-specified path
        /// </summary>
        /// <returns>true if database was created, false otherwise</returns>
        public static Boolean CreateSQLiteDatabase(string dbPath)
        {
            if (String.IsNullOrEmpty(dbPath))
                throw new ArgumentException("dbPath is null or empty.", "dbPath");
         
            var asm = Assembly.GetAssembly(typeof(DbOperations));

            //to create the default.sqlite database file
            try
            {
                using (Stream input = asm.GetManifestResourceStream("HydroDesktop.Resources.defaultDatabase.sqlite"))
                {
                    using (Stream output = File.Create(dbPath))
                    {
                        CopyStream(input, output);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("Error creating the default database " + dbPath +
                    ". Please check the write permissions for HydroDesktop. " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating the default database " + dbPath +
                    ". Error details: " + ex.Message);
                return false;
            }
            return File.Exists(dbPath);
        }


        /// <summary>
        /// Create the default empty MetadataCache.SQLITE database in the user-specified path
        /// The created database has the correct db schema.
        /// </summary>
        /// <returns>true if database was created, false otherwise</returns>
        public static Boolean CreateMetadataCacheDb(string dbPath)
        {
            if (String.IsNullOrEmpty(dbPath))
                throw new ArgumentException("dbPath is null or empty.", "dbPath");

            var asm = Assembly.GetAssembly(typeof(DbOperations));

            //to create the default.sqlite database file
            try
            {
                using (var input = asm.GetManifestResourceStream("HydroDesktop.Resources.MetadataCache.sqlite"))
                {
                    using (Stream output = File.Create(dbPath))
                    {
                        CopyStream(input, output);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("Error creating the metadata cache database " + dbPath +
                    ". Please check the write permissions for HydroDesktop. " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error creating the metadata cache database " + dbPath +
                    ". Error details: " + ex.Message);
                return false;
            }
            return File.Exists(dbPath);
        }

        private static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        /// <summary>
        /// Check if the path is a valid SQLite database
        /// This function returns false, if the SQLite db
        /// file doesn't exist or if the file size is 0 Bytes
        /// </summary>
        public static bool DatabaseExists(string dbPath)
        {
            if (!File.Exists(dbPath))
            {
                return false;
            }
            var dbFileInfo = new FileInfo(dbPath);
            if (dbFileInfo.Length == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validate database schema.
        /// If schema not valid - InvalidDatabaseSchemaException throws.
        /// </summary>
        /// <param name="databaseToCheck">Path to SQLite checked database.</param>
        /// <param name="databaseType">Database type.</param>
        /// <exception cref="InvalidDatabaseSchemaException">Throws if database schema not valid.</exception>
        public static void CheckDatabaseSchema(string databaseToCheck, DatabaseType databaseType)
        {
            if (!DatabaseExists(databaseToCheck))
                throw new InvalidDatabaseSchemaException("Database not exists");

            var originalDbPath = Path.GetTempFileName();
            try
            {
                bool dbCreated;
                switch (databaseType)
                {
                    case DatabaseType.DefaulDatabase:
                        dbCreated = CreateSQLiteDatabase(originalDbPath);
                        break;
                    case DatabaseType.MetadataCacheDatabase:
                        dbCreated = CreateMetadataCacheDb(originalDbPath);
                        break;
                    default:
                        dbCreated = false;
                        break;
                }
                if (!dbCreated)
                    throw new InvalidDatabaseSchemaException("Unable to create original database");

                var originalDbOperations = new DbOperations(GetSQLiteConnectionString(originalDbPath),
                                                    Interfaces.DatabaseTypes.SQLite);

                var checkedlDbOperations = new DbOperations(GetSQLiteConnectionString(databaseToCheck),
                                                    Interfaces.DatabaseTypes.SQLite);

                const string allTablesQuery = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";

                var originalTables = originalDbOperations.LoadTable(allTablesQuery);
                var checkedTables = checkedlDbOperations.LoadTable(allTablesQuery);

                var sbErrors = new StringBuilder();
                foreach(DataRow row in originalTables.Rows)
                {
                    var tableName = row["name"].ToString();

                    // Check for table existing
                    var rows = checkedTables.Select(string.Format("name = '{0}'", tableName));
                    if (rows.Length == 0)
                    {
                        sbErrors.AppendLine(string.Format("Table '{0}' not found", tableName));
                        continue;
                    }

                    // Check for table schema
                    var schemaQuery = string.Format("PRAGMA table_info({0})", tableName);
                    var originalSchema = originalDbOperations.LoadTable(schemaQuery);
                    var checkedSchema = checkedlDbOperations.LoadTable(schemaQuery);

                    foreach(DataRow schemaRow in originalSchema.Rows)
                    {
                        var columnName = schemaRow["name"].ToString();

                        var columnRows = checkedSchema.Select(string.Format("name = '{0}'", columnName));
                        if (columnRows.Length == 0)
                        {
                            sbErrors.AppendLine(string.Format("Table '{0}': column '{1}' not found", tableName, columnName));
                        }
                    }
                }

                if (sbErrors.Length != 0)
                {
                    sbErrors.Remove(sbErrors.Length - Environment.NewLine.Length, Environment.NewLine.Length); // remove last new line delimeter
                    throw new InvalidDatabaseSchemaException(sbErrors.ToString());
                }

            }catch(Exception ex)
            {
                if (ex is InvalidDatabaseSchemaException) throw;
                throw new InvalidDatabaseSchemaException("Unable to check database schema", ex);
            }
            finally
            {
                try
                {
                    File.Delete(originalDbPath);
                }
                catch (IOException ex)
                {
                    Trace.TraceError("Unable to delete temp file: {0}. Reason: {1}" , originalDbPath, ex.Message);   
                }
            }
        }
    }
    /// <summary>
    /// The type of SQLite database (data repositor or metadata cache)
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Data repository sqlite database
        /// </summary>
        DefaulDatabase,
        /// <summary>
        /// Metadata cache SQLite database
        /// </summary>
        MetadataCacheDatabase
    }
    /// <summary>
    /// This exception occurs in case of invalid database schema
    /// </summary>
    public class InvalidDatabaseSchemaException : Exception
    {
        /// <summary>
        /// new instance of invalid database schema exception
        /// </summary>
        public InvalidDatabaseSchemaException()
        {
        }

        /// <summary>
        /// invalid database schema exception with message and inner exception
        /// </summary>
        /// <param name="message">the error messsage</param>
        /// <param name="inner">the inner exception</param>
        public InvalidDatabaseSchemaException(string message, Exception inner = null)
            : base(message, inner)
        {

        }
        /// <summary>
        /// invalid database schema exception with serialization info and streaming context
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">streaming context</param>
        protected InvalidDatabaseSchemaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
