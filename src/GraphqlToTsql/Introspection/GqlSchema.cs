//using GraphqlToTsql.Entities;
//using GraphqlToTsql.Translator;
//using System.Collections.Generic;
//using System.Linq;

//namespace GraphqlToTsql.Introspection
//{
//    // todo: What are InputFields?, what are args?, how to document MaxPageSize

//    internal class GqlSchema
//    {
//        public List<GqlType> Types { get; set; }
//        public GqlType QueryType { get; set; }
//        public GqlType MutationType => null;
//        public GqlType SubscriptionType => null;
//        public List<GqlDirective> Directives => new List<GqlDirective>();

//        public GqlSchema(List<EntityBase> entityList)
//        {
//            Types = new List<GqlType>();

//            BuildBaseTypes();

//            var entityTypeBuilder = new EntityTypeBuilder();
//            var entityTypes = entityTypeBuilder.Build(entityList);
//            Types.AddRange(entityTypes);

//            BuildQueryTypes(entityList);
//        }

//        private void BuildBaseTypes()
//        {
//            // Scalar types
//            var stringType = BuildScalarType(ValueType.String.ToString(), "The `String` scalar type represents textual data, represented as UTF-8 character sequences. The String type is most often used by GraphQL to represent free-form human-readable text.");
//            BuildScalarType(ValueType.Int.ToString(), "The `Int` scalar type represents non-fractional signed whole numeric values.");
//            BuildScalarType(ValueType.Float.ToString(), "The `Float` scalar type represents numeric values that may have fractional values.");
//            var boolType = BuildScalarType(ValueType.Boolean.ToString(), "The `Boolean` scalar type represents `true` or `false`.");
//            BuildScalarType("ID", "The `ID` scalar type represents a unique identifier, often used to refetch an object or as key for a cache. The ID type appears in a JSON response as a String; however, it is not intended to be human-readable. When expected as an input type, any string (such as `\"4\"`) or integer (such as `4`) input value will be accepted as an ID.");
//            BuildScalarType("Upload", "The `Upload` scalar type represents a file upload.");

//            // __TypeKind enum
//            var typeKindEnum = GqlType.Enum("__TypeKind",
//                "SCALAR", "OBJECT", "INTERFACE", "UNION", "ENUM", "INPUT_OBJECT", "LIST", "NON_NULL");
//            Types.Add(typeKindEnum);

//            // __DirectiveLocation enum
//            var directiveLocationEnum = GqlType.Enum("__DirectiveLocation",
//                "QUERY", "MUTATION", "SUBSCRIPTION", "FIELD", "FRAGMENT_DEFINITION", "FRAGMENT_SPREAD", "INLINE_FRAGMENT",
//                "VARIABLE_DEFINITION", "SCHEMA", "SCALAR", "OBJECT", "FIELD_DEFINITION", "ARGUMENT_DEFINITION", "INTERFACE",
//                "UNION", "ENUM", "ENUM_VALUE", "INPUT_OBJECT", "INPUT_FIELD_DEFINITION");
//            Types.Add(directiveLocationEnum);

//            // CacheControlScope enum
//            var cacheControlScopeEnum = GqlType.Enum("CacheControlScope", "PUBLIC", "PRIVATE");
//            Types.Add(cacheControlScopeEnum);


//            // Declare all the types
//            var directiveType = GqlType.Object("__Directive");
//            var enumValueType = GqlType.Object("__EnumValue");
//            var fieldType = GqlType.Object("__Field");
//            var inputValueType = GqlType.Object("__InputValue");
//            var schemaType = GqlType.Object("__Schema");
//            var typeType = GqlType.Object("__Type");

//            // __Directive fields
//            directiveType.Fields.AddRange(new[] {
//                new GqlField("name", GqlType.NonNullable(stringType)),
//                new GqlField("description", stringType),
//                new GqlField("locations", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(directiveLocationEnum)))),
//                new GqlField("args", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(inputValueType))))
//            });
//            Types.Add(directiveType);

//            // __EnumValue fields
//            enumValueType.Fields.AddRange(new[] {
//                new GqlField("name", GqlType.NonNullable(stringType)),
//                new GqlField("description", stringType),
//                new GqlField("isDeprecated", GqlType.NonNullable(boolType)),
//                new GqlField("deprecationReason", stringType)
//            });
//            Types.Add(enumValueType);

//            // __Field fields
//            fieldType.Fields.AddRange(new[] {
//                new GqlField("name", GqlType.NonNullable(stringType)),
//                new GqlField("description", stringType),
//                new GqlField("args", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(inputValueType)))),
//                new GqlField("type", GqlType.NonNullable(typeType)),
//                new GqlField("isDeprecated", GqlType.NonNullable(boolType)),
//                new GqlField("deprecationReason", stringType)
//            });
//            Types.Add(fieldType);

//            // __InputValue fields
//            inputValueType.Fields.AddRange(new[] {
//                new GqlField("name", GqlType.NonNullable(stringType)),
//                new GqlField("description", stringType),
//                new GqlField("type", GqlType.NonNullable(typeType)),
//                new GqlField("defaultValue", stringType)
//            });
//            Types.Add(inputValueType);

//            // __Schema fields
//            schemaType.Fields.AddRange(new[] {
//                new GqlField("types", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(typeType)))),
//                new GqlField("queryType", GqlType.NonNullable(typeType)),
//                new GqlField("mutationType", typeType),
//                new GqlField("subscriptionType", typeType),
//                new GqlField("directives", GqlType.NonNullable(GqlType.List(GqlType.NonNullable(directiveType))))
//            });
//            Types.Add(schemaType);

//            // __Type fields
//            typeType.Fields.AddRange(new[] {
//                new GqlField("kind", GqlType.NonNullable(typeKindEnum)),
//                new GqlField("name", stringType),
//                new GqlField("description", stringType),
//                new GqlField("fields", GqlType.List(GqlType.NonNullable(fieldType)), includedDeprecated: true),
//                new GqlField("interfaces", GqlType.List(GqlType.NonNullable(typeType))),
//                new GqlField("possibleTypes", GqlType.List(GqlType.NonNullable(typeType))),
//                new GqlField("enumValues", GqlType.List(GqlType.NonNullable(enumValueType)), includedDeprecated: true),
//                new GqlField("inputFields", GqlType.List(GqlType.NonNullable(inputValueType))),
//                new GqlField("ofType", typeType)
//            });
//            Types.Add(typeType);
//        }

//        private GqlType BuildScalarType(string name, string description)
//        {
//            var type = GqlType.Scalar(name, description);
//            Types.Add(type);
//            return type;
//        }

//        private void BuildQueryTypes(List<EntityBase> entityList)
//        {
//            QueryType = GqlType.Object("Query");
//            Types.Add(QueryType);

//            foreach (var entity in entityList)
//            {
//                var type = GetType(entity.EntityType);
//                var rowField = new GqlField(entity.Name, type);
//                QueryType.Fields.Add(rowField);

//                var listType = GqlType.List(type);
//                var setField = new GqlField(entity.PluralName, listType);
//                QueryType.Fields.Add(setField);
//            }
//        }

//        private GqlType GetType(string name)
//        {
//            return Types.Single(t => t.Name == name);
//        }
//    }
//}
