using GraphqlToTsql.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Introspection
{
    internal class IntrospectionData
    {
        private List<GqlType> Types { get; set; }
        private List<GqlDirective> Directives { get; set; }

        #region Build data structures for type system

        /// <summary>
        /// Construct introspection type data
        /// </summary>
        /// <param name="entityList">List of entities INCLUDING the Gql entities</param>
        public IntrospectionData(List<EntityBase> entityList)
        {
            Types = new List<GqlType>();

            BuildScalarTypes();

            BuildEnums();

            BuildEntityTypes(entityList);

            BuildQueryTypes(entityList);

            BuildDirectives();
        }

        private void BuildScalarTypes()
        {
            BuildScalarType(ValueType.String.ToString(), "The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text.");
            BuildScalarType(ValueType.Int.ToString(), "The `Int` scalar type represents non-fractional signed whole numeric values.");
            BuildScalarType(ValueType.Float.ToString(), "The `Float` scalar type represents numeric values that may have fractional values.");
            BuildScalarType(ValueType.Boolean.ToString(), "The `Boolean` scalar type represents `true` or `false`.");
            BuildScalarType("ID", "The `ID` scalar type represents a unique identifier, often used to refetch an object or as key for a cache. The ID type appears in a JSON response as a String; however, it is not intended to be human-readable. When expected as an input type, any string (such as `\"4\"`) or integer (such as `4`) input value will be accepted as an ID.");
            BuildScalarType("Upload", "The `Upload` scalar type represents a file upload.");
        }

        private void BuildScalarType(string name, string description)
        {
            var type = GqlType.Scalar(name, description);
            Types.Add(type);
        }

        private void BuildEntityTypes(List<EntityBase> entityList)
        {
            foreach (var entity in entityList)
            {
                EntityType(entity);

                // Every entity has an associated ConnectionEntity and OrderByObject
                if (!entity.IsSystemEntity)
                {
                    var setField = Field.Set(entity, entity.PluralName + Constants.CONNECTION, join: null);
                    var connectionEntity = new ConnectionEntity(setField);
                    EntityType(connectionEntity);

                    OrderByObject(entity);
                }
            }
        }

        private GqlType EntityType(EntityBase entity)
        {
            // If this type has already been built (or at least initialized) return it
            var key = entity.EntityType;
            if (IsTypeRegistered(key))
            {
                return GetType(key);
            }

            // Initialize
            var type = GqlType.Object(key);
            Types.Add(type);

            // Build fields
            foreach (var field in entity.Fields)
            {
                if (field.Visibility == Visibility.Hidden)
                {
                    continue;
                }

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
                        var setField = SetField(field);
                        type.Fields.Add(setField);

                        // A "Set" field can also be queried using a Connection
                        if (!field.Entity.IsSystemEntity)
                        {
                            var connectionEntity = new ConnectionEntity(field);
                            var connectionType = EntityType(connectionEntity);
                            var connectionField = new GqlField(field.Name + Constants.CONNECTION, connectionType);
                            type.Fields.Add(connectionField);
                        }
                        break;

                    case FieldType.Edge:
                        type.Fields.Add(SetField(field));
                        break;
                }
            }

            return type;
        }

        private GqlType OrderByObject(EntityBase entity)
        {
            var type = GqlType.Object(entity.EntityType + Constants.ORDER_BY_OBJECT);
            if (IsTypeRegistered(type.Key))
            {
                return GetType(type.Key);
            }
            Types.Add(type);

            var orderByEnumType = GetType("OrderByEnum");

            foreach (var field in entity.Fields)
            {
                if (field.Visibility != Visibility.Hidden &&
                    field.FieldType == FieldType.Column)
                {
                    var orderByField = new GqlField(field.Name, orderByEnumType);
                    type.Fields.Add(orderByField);
                }
            }

            return type;
        }

        private GqlField ScalarField(Field field)
        {
            var baseType = GetType(field.ValueType.ToString());

            var type = field.IsNullable == IsNullable.Yes
                ? baseType
                : NonNullableType(baseType);

            return new GqlField(field.Name, type);
        }

        private GqlField RowField(Field field)
        {
            var type = EntityType(field.Entity);

            if (field.IsNullable == IsNullable.No)
            {
                type = NonNullableType(type);
            }

            return new GqlField(field.Name, type);
        }

        private GqlField SetField(Field field)
        {
            var type = EntityType(field.Entity);

            if (field.ListCanBeEmpty == ListCanBeEmpty.No)
            {
                type = NonNullableType(type);
            }

            type = SetType(type);

            if (field.IsNullable == IsNullable.No)
            {
                type = NonNullableType(type);
            }

            return new GqlField(field.Name, type);
        }

        private GqlType NonNullableType(GqlType baseType)
        {
            var type = GqlType.NonNullable(baseType);
            return LookupOrRegister(type);
        }

        private GqlType SetType(GqlType baseType)
        {
            var type = GqlType.List(baseType);
            return LookupOrRegister(type);
        }

        private void BuildEnums()
        {
            // __TypeKind enum
            var typeKindEnum = GqlType.Enum("__TypeKind",
                "SCALAR", "OBJECT", "INTERFACE", "UNION", "ENUM", "INPUT_OBJECT", "LIST", "NON_NULL");
            Types.Add(typeKindEnum);

            // __DirectiveLocation enum
            var directiveLocationEnum = GqlType.Enum("__DirectiveLocation",
                "QUERY", "MUTATION", "SUBSCRIPTION", "FIELD", "FRAGMENT_DEFINITION", "FRAGMENT_SPREAD", "INLINE_FRAGMENT",
                "VARIABLE_DEFINITION", "SCHEMA", "SCALAR", "OBJECT", "FIELD_DEFINITION", "ARGUMENT_DEFINITION", "INTERFACE",
                "UNION", "ENUM", "ENUM_VALUE", "INPUT_OBJECT", "INPUT_FIELD_DEFINITION");
            Types.Add(directiveLocationEnum);

            // CacheControlScope enum
            var cacheControlScopeEnum = GqlType.Enum("CacheControlScope", "PUBLIC", "PRIVATE");
            Types.Add(cacheControlScopeEnum);

            // OrderByEnum
            var orderByEnum = GqlType.Enum("OrderByEnum", "asc", "desc");
            Types.Add(orderByEnum);
        }

        private void BuildQueryTypes(List<EntityBase> entityList)
        {
            var queryType = GqlType.Object("Query");
            Types.Add(queryType);

            foreach (var entity in entityList)
            {
                if (entity.IsSystemEntity)
                {
                    continue;
                }

                var type = GetType(entity.EntityType);
                var rowField = new GqlField(entity.Name, type);
                queryType.Fields.Add(rowField);

                var listType = SetType(type);
                var setField = new GqlField(entity.PluralName, listType);
                queryType.Fields.Add(setField);

                var connectionType = GetType(entity.EntityType + Constants.CONNECTION);
                var connectionField = new GqlField(entity.PluralName + Constants.CONNECTION, connectionType);
                queryType.Fields.Add(connectionField);
            }
        }

        private void BuildDirectives()
        {
            var boolType = GetType("Boolean");

            Directives = new List<GqlDirective>();

            Directives.Add(new GqlDirective
            {
                Name = "include",
                Locations = new List<DirectiveLocation> { DirectiveLocation.FIELD },
                Args = new List<GqlInputValue> { new GqlInputValue { Name = "if", Type = boolType } }
            });

            Directives.Add(new GqlDirective
            {
                Name = "skip",
                Locations = new List<DirectiveLocation> { DirectiveLocation.FIELD },
                Args = new List<GqlInputValue> { new GqlInputValue { Name = "if", Type = boolType } }
            });
        }

        private GqlType LookupOrRegister(GqlType type)
        {
            if (IsTypeRegistered(type.Key))
            {
                return GetType(type.Key);
            }

            Types.Add(type);
            return type;
        }

        private bool IsTypeRegistered(string key)
        {
            return Types.Any(_ => _.Key == key);
        }

        private GqlType GetType(string key)
        {
            return Types.Single(t => t.Key == key);
        }

        #endregion

        #region SQL generation

        public string GetCteSql(string name)
        {
            if (name == GqlTypeEntity.Instance.Name)
            {
                return GetTypesSql();
            }
            if (name == GqlFieldEntity.Instance.Name)
            {
                return GetFieldsSql();
            }
            if (name == GqlEnumValueEntity.Instance.Name)
            {
                return GetEnumValuesSql();
            }
            if (name == GqlDirectiveEntity.Instance.Name)
            {
                return GetDirectivesSql();
            }
            if (name == GqlInputValueEntity.Instance.Name)
            {
                return GetInputValuesSql();
            }

            throw new Exception($"Unsupported Introspection type: {name}");
        }

        private string GetTypesSql()
        {
            var sb = new StringBuilder(1024);
            var isFirstRow = true;
            foreach (var type in Types)
            {
                AppendTypeRow(sb, isFirstRow, type);
                isFirstRow = false;
            }

            return sb.ToString().Trim();
        }

        private string GetFieldsSql()
        {
            var sb = new StringBuilder(1024);
            var isFirstRow = true;
            foreach (var type in Types)
            {
                if (type.Fields != null)
                {
                    foreach (var field in type.Fields)
                    {
                        AppendFieldRow(sb, isFirstRow, type.Key, field);
                        isFirstRow = false;
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private string GetEnumValuesSql()
        {
            var sb = new StringBuilder(1024);
            var isFirstRow = true;

            EnumValuesForOneType("__TypeKind", sb, ref isFirstRow);
            EnumValuesForOneType("__DirectiveLocation", sb, ref isFirstRow);
            EnumValuesForOneType("CacheControlScope", sb, ref isFirstRow);
            EnumValuesForOneType("OrderByEnum", sb, ref isFirstRow);

            return sb.ToString().Trim();
        }

        private string GetDirectivesSql()
        {
            var sb = new StringBuilder(1024);
            var isFirstRow = true;

            foreach (var directive in Directives)
            {
                var locationsJson = JsonConvert.SerializeObject(directive.Locations);
                AppendDirectiveRow(sb, isFirstRow, directive.Name, locationsJson);
                isFirstRow = false;
            }

            return sb.ToString().Trim();
        }

        private string GetInputValuesSql()
        {
            var sb = new StringBuilder(1024);
            var isFirstRow = true;

            // InputValues for row/set filtering
            foreach (var parentType in Types) // e.g. Seller
            {
                if (parentType.Fields != null)
                {
                    foreach (var field in parentType.Fields)
                    {
                        var unwrappedFieldType = UnwrapType(field.Type);

                        // The type has input values on each of its scalar fields
                        if (field.Type.Kind == TypeKind.SCALAR ||
                            (field.Type.Kind == TypeKind.NON_NULL && field.Type.OfType.Kind == TypeKind.SCALAR))
                        {
                            AppendInputValueRow(sb, isFirstRow, parentType.Key, null, field.Name, unwrappedFieldType.Key);
                            isFirstRow = false;
                        }

                        // Allow input value filters on this field if:
                        //   * The field's type is OBJECT
                        //   * The field's type has scalar fields
                        if (unwrappedFieldType.Kind == TypeKind.OBJECT)
                        {
                            foreach (var subfield in unwrappedFieldType.Fields)
                            {
                                var subfieldType = UnwrapType(subfield.Type);
                                if (subfieldType.Kind == TypeKind.SCALAR)
                                {
                                    AppendInputValueRow(sb, isFirstRow, parentType.Key, field.Name, subfield.Name, subfieldType.Key);
                                    isFirstRow = false;
                                }
                            }

                            // For a SET, add boilerplate input filters
                            if (field.Type.Kind == TypeKind.LIST ||
                                (field.Type.Kind == TypeKind.NON_NULL && field.Type.OfType.Kind == TypeKind.LIST))
                            {
                                AppendInputValueRow(sb, false, parentType.Key, field.Name, Constants.FIRST_ARGUMENT, ValueType.Int.ToString());
                                AppendInputValueRow(sb, false, parentType.Key, field.Name, Constants.OFFSET_ARGUMENT, ValueType.Int.ToString());
                                AppendInputValueRow(sb, false, parentType.Key, field.Name, Constants.AFTER_ARGUMENT, ValueType.String.ToString());
                                AppendInputValueRow(sb, false, parentType.Key, field.Name, Constants.ORDER_BY_ARGUMENT, unwrappedFieldType.Key + Constants.ORDER_BY_OBJECT);
                            }
                        }
                    }
                }
            }

            // InputValues for directives
            foreach (var directive in Directives)
            {
                foreach (var arg in directive.Args)
                {
                    AppendInputValueRow(sb, isFirstRow, Constants.DIRECTIVE_TYPE_KEY, directive.Name, arg.Name, arg.Type.Key);
                    isFirstRow = false;
                }
            }

            return sb.ToString().Trim();
        }

        private static GqlType UnwrapType(GqlType wrappedType)
        {
            var type = wrappedType;
            while (type.OfType != null)
            {
                type = type.OfType;
            }
            return type;
        }

        private void EnumValuesForOneType(string enumTypeKey, StringBuilder sb, ref bool isFirstRow)
        {
            var enumType = GetType(enumTypeKey);

            foreach (var enumValue in enumType.EnumValues)
            {
                AppendEnumValueRow(sb, isFirstRow, enumTypeKey, enumValue);
                isFirstRow = false;
            }
        }

        private void AppendTypeRow(StringBuilder sb, bool isFirstRow, GqlType type)
        {
            sb.Append(isFirstRow ? "SELECT" : "UNION ALL SELECT");
            AppendColumn(sb, isFirstRow, true, "[Key]", type.Key);
            AppendColumn(sb, isFirstRow, false, "Kind", type.Kind.ToString());
            AppendColumn(sb, isFirstRow, false, "Name", type.Name);
            AppendColumn(sb, isFirstRow, false, "OfTypeKey", type.OfType?.Key);
            AppendColumn(sb, isFirstRow, false, "Description", type.Description);
            sb.AppendLine();
        }

        private void AppendFieldRow(StringBuilder sb, bool isFirstRow, string parentTypeKey, GqlField field)
        {
            sb.Append(isFirstRow ? "SELECT" : "UNION ALL SELECT");
            AppendColumn(sb, isFirstRow, true, "ParentTypeKey", parentTypeKey);
            AppendColumn(sb, isFirstRow, false, "Name", field.Name);
            AppendColumn(sb, isFirstRow, false, "Description", field.Description);
            AppendColumn(sb, isFirstRow, false, "TypeKey", field.Type.Key);
            AppendColumn(sb, isFirstRow, false, "IsDeprecated", field.IsDeprecated);
            AppendColumn(sb, isFirstRow, false, "DeprecationReason", field.DeprecationReason);

            sb.AppendLine();
        }

        private void AppendEnumValueRow(StringBuilder sb, bool isFirstRow, string enumTypeKey, GqlEnumValue enumValue)
        {
            sb.Append(isFirstRow ? "SELECT" : "UNION ALL SELECT");
            AppendColumn(sb, isFirstRow, true, "EnumTypeKey", enumTypeKey);
            AppendColumn(sb, isFirstRow, false, "Name", enumValue.Name);
            AppendColumn(sb, isFirstRow, false, "Description", enumValue.Description);
            AppendColumn(sb, isFirstRow, false, "IsDeprecated", enumValue.IsDeprecated);
            AppendColumn(sb, isFirstRow, false, "DeprecationReason", enumValue.DeprecationReason);

            sb.AppendLine();
        }

        private void AppendDirectiveRow(StringBuilder sb, bool isFirstRow, string name, string locationsJson)
        {
            sb.Append(isFirstRow ? "SELECT" : "UNION ALL SELECT");
            AppendColumn(sb, isFirstRow, true, "Name", name);
            AppendColumn(sb, isFirstRow, false, "Description", null);
            AppendColumn(sb, isFirstRow, false, "LocationsJson", locationsJson);

            sb.AppendLine();
        }

        private void AppendInputValueRow(StringBuilder sb, bool isFirstRow, string parentTypeKey, string fieldName,
            string name, string typeKey)
        {
            sb.Append(isFirstRow ? "SELECT" : "UNION ALL SELECT");
            AppendColumn(sb, isFirstRow, true, "ParentTypeKey", parentTypeKey);
            AppendColumn(sb, isFirstRow, false, "FieldName", fieldName);
            AppendColumn(sb, isFirstRow, false, "Name", name);
            AppendColumn(sb, isFirstRow, false, "Description", null);
            AppendColumn(sb, isFirstRow, false, "TypeKey", typeKey);
            AppendColumn(sb, isFirstRow, false, "DefaultValue", null);

            sb.AppendLine();
        }

        private static void AppendColumn(StringBuilder sb, bool isFirstRow, bool isFirstColumn, string name, object value)
        {
            sb.Append(isFirstColumn ? " " : ", ");

            var valueString = value == null
                ? "null"
                : value.GetType().Name == "String"
                ? $"'{value}'"
                : value.GetType().Name == "Boolean" && (bool)value == true
                ? "1"
                : value.GetType().Name == "Boolean"
                ? "0"
                : value.ToString();
            sb.Append(isFirstRow ? $"{valueString} AS {name}" : valueString);
        }

        #endregion
    }
}
