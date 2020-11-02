using System.Collections.Generic;

namespace GraphqlToSql.Transpiler.Entities
{
    public class CodeDef : EntityBase
    {
        public static CodeDef Instance = new CodeDef();

        public override string Name => "code";
        public override string DbTableName => "Code";

        private CodeDef()
        {
            Fields = new List<Field>
            {
                Field.Scalar(this, "id", "CodeID"),
                Field.Scalar(this, "parentCodeId", "ParentCodeID"),
                Field.Scalar(this, "codeStatusId", "CodeStatusID"),
                Field.Scalar(this, "secureCode", "SecureCode")
            };
        }
    }
}
