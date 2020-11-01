using System;
using System.Collections.Generic;
using System.Text;

namespace GraphqlToSql.Transpiler.Models
{
    public class FieldDef
    {
        public EntityBase Entity { get; }
        public string Name { get; }
        public string DbColumnName { get; }

        // Properties for complex fields
        public bool IsList { get; }
        //public Func<string> JoinFunc { get; }

        //public FieldDef(ModelDefBase def, string name, string graphName = null, bool isList = false, Func<string> joinFunc = null)
        //{
        //    Def = def;
        //    Name = name;
        //    GraphName = graphName ?? name;
        //    IsList = isList;
        //    JoinFunc = joinFunc;
        //}

        public FieldDef(EntityBase entity, string name, string dbColumnName, bool isList = false)
        {
            Entity = entity;
            Name = name;
            DbColumnName = dbColumnName;
            IsList = isList;
        }

        public FieldDef Clone(string dbColumnName)
        {
            return new FieldDef(this.Entity, this.Name, dbColumnName, this.IsList);
        }
    }
}
