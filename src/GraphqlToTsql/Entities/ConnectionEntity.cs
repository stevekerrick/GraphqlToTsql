﻿using System;
using System.Collections.Generic;

namespace GraphqlToTsql.Entities
{
    internal class ConnectionEntity : EntityBase
    {
        private readonly Field _setField;

        public override string Name => $"{_setField.Name}{Constants.CONNECTION}";
        public override string DbTableName => _setField.Entity.DbTableName;
        public override string EntityType => $"{_setField.Entity.EntityType}{Constants.CONNECTION}";
        public override string[] PrimaryKeyFieldNames => _setField.Entity.PrimaryKeyFieldNames;

        internal ConnectionEntity(Field setField)
        {
            _setField = setField;
        }

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.TotalCount(_setField),
                Field.Edges(_setField)
            };
        }
    }
}
