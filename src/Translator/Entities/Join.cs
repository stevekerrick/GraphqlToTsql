using System;

namespace GraphqlToTsql.Translator.Entities
{
    public class Join
    {
        public Func<Field> ParentFieldFunc { get; }
        public Func<Field> ChildFieldFunc { get; }

        public Join(Func<Field> parentFieldFunc, Func<Field> childFieldFunc)
        {
            ParentFieldFunc = parentFieldFunc;
            ChildFieldFunc = childFieldFunc;
        }

    }
}
