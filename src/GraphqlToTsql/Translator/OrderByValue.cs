using GraphqlToTsql.CodeGen;
using GraphqlToTsql.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphqlToTsql.Translator
{
    internal class OrderByValue
    {
        public List<OrderByField> Fields { get; set; }

        private OrderByValue()
        {
            Fields = new List<OrderByField>();
        }

        // e.g. { "name": "desc" }
        public static OrderByValue FromRawValue(object rawValue)
        {
            if (rawValue == null)
            {
                return null;
            }

            // The rawValue could be a JObject, or it could be an anonymous object
            var jtoken = rawValue as JToken;
            if (jtoken == null)
            {
                jtoken = JToken.FromObject(rawValue);
            }

            var orderByValue = new OrderByValue();

            // The rawValue might be for a single Column or for an array of them
            if (jtoken.Type == JTokenType.Array)
            {
                var jarray = jtoken as JArray;
                if (jarray.Count == 0)
                {
                    throw Error();
                }

                foreach (var item in jarray)
                {
                    orderByValue.Fields.Add(GetOrderByFieldFromJToken(item));
                }
            }
            else
            {
                orderByValue.Fields.Add(GetOrderByFieldFromJToken(jtoken));
            }

            return orderByValue;
        }

        public static OrderByValue FromParse(GqlParser.ValueContext valueContext)
        {
            var orderByValue = new OrderByValue();

            var listValueContext = valueContext.listValue();
            if (listValueContext != null)
            {
                var childContexts = listValueContext.value();
                if (childContexts.Length == 0)
                {
                    throw Error();
                }

                foreach (var childContext in childContexts)
                {
                    orderByValue.Fields.Add(OrderByFieldFromParse(childContext));
                }
            }
            else
            {
                orderByValue.Fields.Add(OrderByFieldFromParse(valueContext));
            }

            return orderByValue;
        }

        private static OrderByField GetOrderByFieldFromJToken(JToken rawValue)
        {
            if (rawValue.Type != JTokenType.Object)
            {
                throw Error();
            }

            var jobject = rawValue as JObject;
            if (jobject.Count != 1)
            {
                throw Error();
            }

            var jproperty = jobject.Properties().First();
            if (jproperty.Value == null || jproperty.Value.Type != JTokenType.String)
            {
                throw Error();
            }

            var fieldName = jproperty.Name;
            if (!Enum.TryParse(jproperty.Value.ToString(), out OrderByEnum orderByEnum))
            {
                throw Error();
            }

            return new OrderByField { FieldName = fieldName, OrderByEnum = orderByEnum };
        }

        private static OrderByField OrderByFieldFromParse(GqlParser.ValueContext valueContext)
        {
            var objectValueContext = valueContext.objectValue();
            if (objectValueContext == null)
            {
                throw Error();
            }

            // The object must have exactly one field: {"city": asc}
            var objectFieldContexts = objectValueContext.objectField();
            if (objectFieldContexts.Length != 1)
            {
                throw Error();
            }

            // Pick out the fieldName and orderByEnum
            var objectFieldContext = objectFieldContexts[0];
            var name = objectFieldContext.name().GetText();

            var orderByEnumValueContext = objectFieldContext.value();
            var enumValueContext = orderByEnumValueContext.enumValue();
            if (enumValueContext == null)
            {
                throw Error();
            }

            var enumString = enumValueContext.GetText();
            if (!Enum.TryParse<OrderByEnum>(enumString, out var orderByEnum))
            {
                throw Error();
            }

            return new OrderByField
            {
                FieldName = name,
                OrderByEnum = orderByEnum
            };
        }

        private static InvalidRequestException Error()
        {
            return new InvalidRequestException(ErrorCode.V30, $"Invalid {Constants.ORDER_BY_ARGUMENT} value. Try something like {{ id: desc }}.");
        }
    }

    internal class OrderByField
    {
        public string FieldName { get; set; }
        public OrderByEnum OrderByEnum { get; set; }
    }

    internal enum OrderByEnum
    {
        asc,
        desc
    }
}
