using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;
using System.Linq;

namespace GraphqlToTsql.Introspection
{
    internal class EntityTypeBuilder
    {
        private Dictionary<string, GqlType> _types;

        public List<GqlType> Build(List<EntityBase> entities)
        {
            _types = new Dictionary<string, GqlType>();

            foreach (var entity in entities)
            {
                EntityType(entity);
            }

            return _types.Values.ToList();
        }

        private GqlType EntityType(EntityBase entity)
        {
            // If this type has already been built (or at least initialized) return it
            var name = entity.EntityType;
            if (_types.ContainsKey(name))
            {
                return _types[name];
            }

            // Initialize
            var type = GqlType.Object(name);
            _types[name] = type;

            // Build fields
            foreach (var field in entity.Fields)
            {
                switch (field.FieldType)
                {
                    case FieldType.Column:
                    case FieldType.Cursor:
                    case FieldType.TotalCount:
                        type.Fields.Add(ScalarField(field));
                        break;

                    case FieldType.Row:
                    case FieldType.Node:
                    case FieldType.Connection:
                        type.Fields.Add(RowField(field));
                        break;

                    case FieldType.Set:
                    case FieldType.Edge:
                        type.Fields.Add(SetField(field));
                        break;
                }
            }

            return type;
        }

        private GqlField ScalarField(Field field)
        {
            var baseType = _types[field.FieldType.ToString()];

            var type = field.IsNullable == IsNullable.Yes
                ? baseType
                : GqlType.NonNullable(baseType);

            return new GqlField(field.Name, type);
        }

        private GqlField RowField(Field field)
        {
            var type = EntityType(field.Entity);

            return new GqlField(field.Name, type);
        }

        private GqlField SetField(Field field)
        {
            var baseType = EntityType(field.Entity);
            var type = GqlType.List(baseType);

            return new GqlField(field.Name, type);
        }
    }
}
