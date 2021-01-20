using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using System.Collections.Generic;

namespace DemoEntities
{
    public class BadgeDef : EntityBase
    {
        public static BadgeDef Instance = new BadgeDef();

        public override string Name => "badge";
        public override string DbTableName => "Badge";
        public override string PrimaryKeyFieldName => "name";

        protected override List<Field> BuildFieldList()
        {
            return new List<Field>
            {
                Field.Scalar(this, "name", "Name", ValueType.String),
                Field.Scalar(this, "isSpecial", "IsSpecial", ValueType.Boolean),

                Field.Set(SellerBadgeDef.Instance, "sellerBadges", new Join(
                    ()=>this.GetField("name"),
                    ()=>SellerBadgeDef.Instance.GetField("badgeName"))
                )
            };
        }
    }
}
