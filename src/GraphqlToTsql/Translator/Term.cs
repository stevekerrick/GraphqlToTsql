using GraphqlToTsql.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GraphqlToTsql.Translator
{
    [DebuggerDisplay("{Name,nq}")]
    public class Term
    {
        public Term Parent { get; private set; }
        public List<Term> Children { get; private set; }
        public Field Field { get; private set; }
        public string Name { get; private set; }
        public TermType TermType { get; private set; }
        public Arguments Arguments { get; private set; }
        private string _tableAlias;

        private Term()
        {
            Children = new List<Term>();
            Arguments = new Arguments();
        }

        public static Term TopLevel()
        {
            return new Term
            {
                Name = "query",
                TermType = TermType.TopLevel,
            };
        }

        public Term(Term parent, Field field, string name) : this()
        {
            Parent = parent;
            Field = field;
            Name = name;

            switch (field.FieldType)
            {
                case FieldType.Scalar:
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
            while (parent.TermType != TermType.TopLevel)
            {
                path = parent.Name + "." + path;
                parent = parent.Parent;
            }
            return path;
        }

        public void AddArgument(string name, Value value)
        {
            Arguments.Add(Field, name, value);
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
    }

    public enum TermType
    {
        TopLevel,
        Scalar,
        Item,
        List,
        Fragment
    }
}
