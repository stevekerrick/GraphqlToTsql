using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ValueType = GraphqlToTsql.Entities.ValueType;

namespace GraphqlToTsql.Translator
{
    [DebuggerDisplay("{Name,nq}")]
    internal class Term
    {
        public Term Parent { get; private set; }
        public List<Term> Children { get; private set; }
        public Field Field { get; private set; }
        public string Name { get; private set; }
        public TermType TermType { get; private set; }

        private Arguments _arguments;
        private OrderByValue _orderByValue;
        private string _tableAlias;

        private Term()
        {
            Children = new List<Term>();
            Arguments = new Arguments();
        }

        public static Term RootTerm()
        {
            return new Term
            {
                Name = "query",
                TermType = TermType.Root,
            };
        }

        public Term(Term parent, Field field, string name) : this()
        {
            Parent = parent;
            Field = field;
            Name = name;

            switch (field.FieldType)
            {
                case FieldType.Column:
                case FieldType.TotalCount:
                case FieldType.Cursor:
                    TermType = TermType.Scalar;
                    break;
                case FieldType.Row:
                case FieldType.Connection:
                case FieldType.Node:
                    TermType = TermType.Item;
                    break;
                case FieldType.Set:
                case FieldType.Edge:
                    TermType = TermType.List;
                    break;
                default:
                    throw new NotImplementedException($"Unexpected FieldType: {field.FieldType}");
            }
        }

        public static Term Fragment(Term parent, string name)
        {
            return new Term
            {
                Parent = parent,
                Name = name,
                TermType = TermType.Fragment
            };
        }

        public static Term Directive(Term parent, Field field)
        {
            return new Term
            {
                Parent = parent,
                Field = field,
                Name = field.Name,
                TermType = TermType.Directive
            };
        }

        public string TableAlias(AliasSequence aliasSequence)
        {
            // For Edges and Node, the alias is managed by the Connection
            if (Field.FieldType == FieldType.Edge)
            {
                return Parent.TableAlias(aliasSequence);
            }
            if (Field.FieldType == FieldType.Node)
            {
                return Parent.Parent.TableAlias(aliasSequence);
            }


            if (_tableAlias == null)
            {
                _tableAlias = aliasSequence.Next();
            }
            return _tableAlias;
        }

        public Term ParentForJoin
        {
            get
            {
                // When building the Where clause for Edges, the join parent has to skip over the Connection
                if (Field.FieldType == FieldType.Edge)
                {
                    return Parent.Parent;
                }
                return Parent;
            }
        }

        public string FullPath()
        {
            var path = Name;
            var parent = Parent;
            while (parent.TermType != TermType.Root)
            {
                path = parent.Name + "." + path;
                parent = parent.Parent;
            }
            return path;
        }

        public void AddArgument(string name, Value value, Context context)
        {
            if (Field.FieldType == FieldType.Edge || Field.FieldType == FieldType.Node || TermType == TermType.Scalar)
            {
                throw new InvalidRequestException(ErrorCode.V16, $"Arguments are not allowed on [{Name}]", context);
            }

            Arguments.Add(Field, name, value, context);
            CheckForConflictBetweenOrderByAndCursorBasedPaging(context);
        }

        public Arguments Arguments
        {
            get
            {
                // The GraphQL for edges have no arguments -- they appear on the Connection.
                // But when the TSQL is formed those arguments apply to the edge.
                if (Field != null && Field.FieldType == FieldType.Edge)
                {
                    return Parent.Arguments;
                }
                return _arguments;
            }
            private set
            {
                _arguments = value;
            }
        }

        public OrderByValue OrderByValue
        {
            get
            {
                // The GraphQL for edges have no arguments -- they appear on the Connection.
                // But when the TSQL is formed those arguments apply to the edge.
                if (Field != null && Field.FieldType == FieldType.Edge)
                {
                    return Parent.OrderByValue;
                }
                return _orderByValue;
            }
            private set
            {
                _orderByValue = value;
            }
        }

        public void SetOrderBy(OrderByValue orderByValue, Context context)
        {
            if (Field.FieldType != FieldType.Set && Field.FieldType != FieldType.Connection)
            {
                throw new InvalidRequestException(ErrorCode.V30, $"{Constants.ORDER_BY} is not allowed on [{Name}]", context);
            }

            //var orderBy = new OrderBy();
            //foreach (var objectField in objectValue.ObjectFields)
            //{
            //    var field = Field.Entity.GetField(objectField.Name, context);

            //    if (objectField.Value.ValueType != ValueType.String)
            //    {
            //        throw new InvalidRequestException(ErrorCode.V30, $"{Constants.ORDER_BY} must be either {Constants.ASC} or {Constants.DESC}, not [{objectField.Value.RawValue}]", context);
            //    }

            //    var canParseOrderByEnum = Enum.TryParse<OrderByEnum>(objectField.Value.RawValue.ToString(), ignoreCase: true, result: out var orderByEnum);
            //    if (!canParseOrderByEnum)
            //    {
            //        throw new InvalidRequestException(ErrorCode.V30, $"{Constants.ORDER_BY} must be either {Constants.ASC} or {Constants.DESC}, not [{objectField.Value.RawValue}]", context);
            //    }

            //    orderBy.Add(field, orderByEnum);
            //}

            OrderByValue = orderByValue;
            CheckForConflictBetweenOrderByAndCursorBasedPaging(context);
        }

        public bool IsFirstChild
        {
            get
            {
                var i = 0;
                while (i < Parent.Children.Count)
                {
                    var child = Parent.Children[i];
                    if (child == this) return true;
                    if (child.TermType != TermType.Fragment) return false;
                    i++;
                }
                return false;
            }
        }

        public Term Clone(Term newParent)
        {
            var term = new Term
            {
                Parent = newParent,
                Field = Field,
                Name = Name,
                TermType = TermType
            };

            term.Arguments = this.Arguments;
            term.Children.AddRange(Children.Select(_ => _.Clone(term)));
            return term;
        }

        private void CheckForConflictBetweenOrderByAndCursorBasedPaging(Context context)
        {
            if (Arguments.After == null || OrderByValue == null)
            {
                return;
            }

            // Cursors only work when there's exactly 1 PK field
            var pkFieldName = Field.Entity.PrimaryKeyFieldNames.FirstOrDefault();
            if (OrderByValue.Fields.Count == 1 && OrderByValue.Fields[0].FieldName == pkFieldName)
            {
                return;
            }

            throw new InvalidRequestException(ErrorCode.V30, $"Because you are using cursor-based paging, you can only {Constants.ORDER_BY} {pkFieldName}", context);
        }
    }

    public enum TermType
    {
        Root,
        Scalar,
        Item,
        List,
        Fragment,
        Directive
    }
}
