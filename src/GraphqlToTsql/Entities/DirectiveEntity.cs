using System;
using System.Collections.Generic;

namespace GraphqlToTsql.Entities
{
    internal class DirectiveEntity : EntityBase
    {
        private string _name;

        public override string Name => _name;
        public override string DbTableName => throw new Exception("Directives don't have a database table");
        public override string EntityType => throw new Exception("Directives don't have an entity type");
        public override string[] PrimaryKeyFieldNames => throw new Exception("Directives don't have a primary key");

        internal DirectiveEntity(string name)
        {
            _name = name;
        }

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>();
        }
    }
}
