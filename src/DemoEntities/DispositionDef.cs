using GraphqlToTsql.Entities;
using System.Collections.Generic;

namespace DemoEntities
{
    public class DispositionDef : EntityBase
    {
        public static DispositionDef Instance = new DispositionDef();

        public override string Name => "disposition";
        public override string DbTableName => "Disposition";
        public override string PrimaryKeyFieldName => "urn";

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "urn", "Urn"),
                Field.Scalar(this, "name", "Name"),

                Field.Set(EpcDef.Instance, "epcs", new Join(
                    ()=>this.GetField("id"),
                    ()=>EpcDef.Instance.GetField("dispositionUrn"))
                )
            };
        }
    }
}
