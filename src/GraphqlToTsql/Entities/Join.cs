using System;

namespace GraphqlToTsql.Entities
{
    /// <summary>
    /// Specifies the columns used to join two entities.
    /// </summary>
    public class Join
    {
        internal Func<Field> ParentFieldFunc { get; }

        internal Func<Field> ChildFieldFunc { get; }

        /// <summary>
        /// Specifies the fields to join a parent entity to a child entity
        /// </summary>
        /// <param name="parentFieldFunc">Func that returns the Field instance for the Parent in the join</param>
        /// <param name="childFieldFunc">Func that returns the Field instance for the Child in the join</param>
        public Join(Func<Field> parentFieldFunc, Func<Field> childFieldFunc)
        {
            ParentFieldFunc = parentFieldFunc;
            ChildFieldFunc = childFieldFunc;
        }
    }
}
