using System.Collections.Generic;

namespace GraphqlToTsql.Entities
{
    public class NodeEntity : EntityBase
    {
        private Field _setField;

        public override string Name => Constants.NODE;
        public override string DbTableName => _setField.Entity.DbTableName;
        public override string PrimaryKeyFieldName => _setField.Entity.PrimaryKeyFieldName;

        internal NodeEntity(Field setField)
        {
            _setField = setField;
        }

        protected override List<Field> BuildFieldList()
        {
            return _setField.Entity.Fields;
        }
    }
}
