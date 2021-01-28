namespace GraphqlToTsql.Database
{
    /// <summary>
    /// Provider for database connection string
    /// </summary>
    public interface IConnectionStringProvider
    {
        /// <summary>
        /// Get the database connection string
        /// </summary>
        /// <returns></returns>
        string Get();
    }
}
