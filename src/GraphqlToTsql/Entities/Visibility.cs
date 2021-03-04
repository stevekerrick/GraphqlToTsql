namespace GraphqlToTsql.Entities
{
    public enum Visibility
    {
        Unknown,

        /// <summary>
        /// Field has normal visibility
        /// </summary>
        Normal,

        /// <summary>
        /// Field is hidden, and inaccessible. This is useful for fields that are used for
        /// internal purposes only, such as fields that are used to join to other entities,
        /// but that you don't want to expose in your GraphQL.
        /// </summary>
        Hidden
    }
}
