using System;
using System.Collections.Generic;

namespace GraphqlToTsql.Entities
{
    internal class EdgeEntity : EntityBase
    {
        private readonly Field _setField;

        public override string Name => $"edge";
        public override string DbTableName => _setField.Entity.DbTableName;
        public override string EntityType => $"{_setField.Entity.EntityType}Edge";
        public override string[] PrimaryKeyFieldNames => throw new Exception("Edges don't have a primary key");
        internal override List<Field> PrimaryKeyFields => _setField.Entity.PrimaryKeyFields;

        internal EdgeEntity(Field setField)
        {
            _setField = setField;
        }

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Node(_setField),
                Field.Cursor(_setField)
            };
        }
    }
}
