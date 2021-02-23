using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphqlToTsql.Introspection
{
    internal static class IntrospectionData
    {
        private static List<GqlType> Types { get; set; }


        #region Build data structures for type system

        /// <summary>
        /// Construct introspection type data
        /// </summary>
        /// <param name="entityList">List of entities INCLUDING the Gql entities</param>
        public static void Initialize(List<EntityBase> entityList)
        {
            Types = new List<GqlType>();

            BuildScalarTypes();

            BuildEntityTypes(entityList);

            BuildBaseTypes();

            BuildQueryTypes(entityList);
        }

        private static void BuildScalarTypes()
        {
            BuildScalarType(ValueType.String.ToString(), "The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text.");
            BuildScalarType(ValueType.Int.ToString(), "The `Int` scalar type represents non-fractional signed whole numeric values.");
            BuildScalarType(ValueType.Float.ToString(), "The `Float` scalar type represents numeric values that may have fractional values.");
            BuildScalarType(ValueType.Boolean.ToString(), "The `Boolean` scalar type represents `true` or `false`.");
            BuildScalarType("ID", "The `ID` scalar type represents a unique identifier, often used to refetch an object or as key for a cache. The ID type appears in a JSON response as a String; however, it is not intended to be human-readable. When expected as an input type, any string (such as `\"4\"`) or integer (such as `4`) input value will be accepted as an ID.");
            BuildScalarType("Upload", "The `Upload` scalar type represents a file upload.");
        }

        private static void BuildScalarType(string name, string description)
        {
            var type = GqlType.Scalar(name, description);
            Types.Add(type);
        }

        private static void BuildEntityTypes(List<EntityBase> entityList)
        {
            foreach (var entity in entityList)
            {
                EntityType(entity);
            }
        }

        private static GqlType EntityType(EntityBase entity)
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

        private static GqlField ScalarField(Field field)
        {
            var baseType = GetType(field.ValueType.ToString());

            var type = field.IsNullable == IsNullable.Yes
                ? baseType
                : NonNullableType(baseType);

            return new GqlField(field.Name, type);
        }

        private static GqlField RowField(Field field)
        {
            var type = EntityType(field.Entity);

            return new GqlField(field.Name, type);
        }

        private static GqlField SetField(Field field)
        {
            var baseType = EntityType(field.Entity);
            var type = SetType(baseType);

            return new GqlField(field.Name, type);
        }

        private static GqlType NonNullableType(GqlType baseType)
        {
            var type = GqlType.NonNullable(baseType);
            return LookupOrRegister(type);
        }

        private static GqlType SetType(GqlType baseType)
        {
            var type = GqlType.List(baseType);
            return LookupOrRegister(type);
        }

        private static void BuildBaseTypes()
        {
            //TODO
            var stringType = GetType("String");
            var boolType = GetType("Boolean");

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


            //// Declare all the types
            //var directiveType = GqlType.Object("__Directive");
            //var enumValueType = GqlType.Object("__EnumValue");
            //var fieldType = GqlType.Object("__Field");
            //var inputValueType = GqlType.Object("__InputValue");
            //var schemaType = GqlType.Object("__Schema");
            //var typeType = GqlType.Object("__Type");

            //// __Directive fields
            //directiveType.Fields.AddRange(new[] {
            //    new GqlField("name", GqlType.NonNullable(stringType)),
            //    new GqlField("description", stringType),
            //    new GqlField("locations", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(directiveLocationEnum)))),
            //    new GqlField("args", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(inputValueType))))
            //});
            //Types.Add(directiveType);

            //// __EnumValue fields
            //enumValueType.Fields.AddRange(new[] {
            //    new GqlField("name", GqlType.NonNullable(stringType)),
            //    new GqlField("description", stringType),
            //    new GqlField("isDeprecated", GqlType.NonNullable(boolType)),
            //    new GqlField("deprecationReason", stringType)
            //});
            //Types.Add(enumValueType);

            //// __Field fields
            //fieldType.Fields.AddRange(new[] {
            //    new GqlField("name", GqlType.NonNullable(stringType)),
            //    new GqlField("description", stringType),
            //    new GqlField("args", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(inputValueType)))),
            //    new GqlField("type", GqlType.NonNullable(typeType)),
            //    new GqlField("isDeprecated", GqlType.NonNullable(boolType)),
            //    new GqlField("deprecationReason", stringType)
            //});
            //Types.Add(fieldType);

            //// __InputValue fields
            //inputValueType.Fields.AddRange(new[] {
            //    new GqlField("name", GqlType.NonNullable(stringType)),
            //    new GqlField("description", stringType),
            //    new GqlField("type", GqlType.NonNullable(typeType)),
            //    new GqlField("defaultValue", stringType)
            //});
            //Types.Add(inputValueType);

            //// __Schema fields
            //schemaType.Fields.AddRange(new[] {
            //    new GqlField("types", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(typeType)))),
            //    new GqlField("queryType", GqlType.NonNullable(typeType)),
            //    new GqlField("mutationType", typeType),
            //    new GqlField("subscriptionType", typeType),
            //    new GqlField("directives", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(directiveType))))
            //});
            //Types.Add(schemaType);

            //// __Type fields
            //typeType.Fields.AddRange(new[] {
            //    new GqlField("kind", GqlType.NonNullable(typeKindEnum)),
            //    new GqlField("name", stringType),
            //    new GqlField("description", stringType),
            //    new GqlField("fields", GqlType.List(GqlType.NonNullable(fieldType)), includedDeprecated: true),
            //    new GqlField("interfaces", GqlType.List(GqlType.NonNullable(typeType))),
            //    new GqlField("possibleTypes", GqlType.List(GqlType.NonNullable(typeType))),
            //    new GqlField("enumValues", GqlType.List(GqlType.NonNullable(enumValueType)), includedDeprecated: true),
            //    new GqlField("inputFields", GqlType.List(GqlType.NonNullable(inputValueType))),
            //    new GqlField("ofType", typeType)
            //});
            //Types.Add(typeType);
        }

        private static void BuildQueryTypes(List<EntityBase> entityList)
        {
            var queryType = GqlType.Object("Query");
            Types.Add(queryType);

            foreach (var entity in entityList)
            {
                var type = GetType(entity.EntityType);
                var rowField = new GqlField(entity.Name, type);
                queryType.Fields.Add(rowField);

                var listType = GqlType.List(type);
                var setField = new GqlField(entity.PluralName, listType);
                queryType.Fields.Add(setField);
            }
        }

        private static GqlType LookupOrRegister(GqlType type)
        {
            if (IsTypeRegistered(type.Key))
            {
                return GetType(type.Key);
            }

            Types.Add(type);
            return type;
        }

        private static bool IsTypeRegistered(string key)
        {
            return Types.Any(_ => _.Key == key);
        }

        private static GqlType GetType(string key)
        {
            return Types.Single(t => t.Key == key);
        }

        #endregion

        #region SQL generation

        public static string GetTypesSql()
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

        public static string GetFieldsSql()
        {
            var sb = new StringBuilder(1024);
            var isFirstRow = true;
            foreach (var type in Types)
            {
                if (type.Fields != null)
                {
                    foreach (var field in type.Fields)
                    {
                        AppendFieldRow(sb, isFirstRow, type.Name, field);
                        isFirstRow = false;
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private static void AppendTypeRow(StringBuilder sb, bool isFirstRow, GqlType type)
        {
            sb.Append(isFirstRow ? "SELECT" : "UNION ALL SELECT");
            AppendColumn(sb, isFirstRow, true, "[Key]", type.Key);
            AppendColumn(sb, isFirstRow, false, "Kind", type.Kind.ToString());
            AppendColumn(sb, isFirstRow, false, "Name", type.Name);
            AppendColumn(sb, isFirstRow, false, "OfTypeKey", type.OfType?.Key);
            AppendColumn(sb, isFirstRow, false, "Description", type.Description);
            sb.AppendLine();
        }

        private static void AppendFieldRow(StringBuilder sb, bool isFirstRow, string typeName, GqlField field)
        {
            sb.Append(isFirstRow ? "SELECT" : "UNION ALL SELECT");
            AppendColumn(sb, isFirstRow, true, "ParentTypeKey", typeName);
            AppendColumn(sb, isFirstRow, false, "Name", field.Name);
            AppendColumn(sb, isFirstRow, false, "Description", field.Description);
            AppendColumn(sb, isFirstRow, false, "TypeKey", field.Type.Key);
            AppendColumn(sb, isFirstRow, false, "IsDeprecated", field.IsDeprecated);
            AppendColumn(sb, isFirstRow, false, "DeprecationReason", field.DeprecationReason);

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
