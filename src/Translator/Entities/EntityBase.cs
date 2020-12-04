﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GraphqlToTsql.Translator.Entities
{
    [DebuggerDisplay("{Name,nq}")]
    public abstract class EntityBase
    {
        public abstract string Name { get; }
        //public virtual string PluralName => $"{Name}s";
        public virtual string DbTableName { get; }
        public List<Field> Fields { get; protected set; }

        public Field GetField(string name)
        {
            var field = Fields.FirstOrDefault(_ => _.Name == name);
            if (field == null)
            {
                throw new Exception($"Unknown field: {Name}.{name}");
            }
            return field;
        }

        // public string SortField => Fields[0].Name; //TODO
    }
}