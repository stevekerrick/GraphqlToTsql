using System;
using System.Collections.Generic;

namespace GraphqlToTsql.Entities
{
    public class EdgeEntity : EntityBase
    {
        private Field _setField;

        public override string Name => $"edge";
        public override string DbTableName => _setField.Entity.DbTableName;
        public override string PrimaryKeyFieldName => throw new Exception("Edges don't have a primary key");

        internal EdgeEntity(Field setField)
        {
            _setField = setField;
        }

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Node(_setField),
            };
        }
    }
}
