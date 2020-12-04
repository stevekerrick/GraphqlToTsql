using System.Collections.Generic;

namespace GraphqlToTsql.Translator.Entities
{
    public class DispositionDef : EntityBase
    {
        public static DispositionDef Instance = new DispositionDef();

        public override string Name => "disposition";
        public override string DbTableName => "Disposition";

        private DispositionDef()
        {
            Fields = new List<Field>
            {
                Field.Scalar(this, "id", "Id"),
                Field.Scalar(this, "urn", "Urn"),
                Field.Scalar(this, "name", "Name"),

                Field.Set(EpcDef.Instance, "epcs", new Join(
                    ()=>this.GetField("id"),
                    ()=>EpcDef.Instance.GetField("dispositionId"))
                )
            };
        }
    }
}