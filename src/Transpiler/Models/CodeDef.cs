using System.Collections.Generic;

namespace GraphqlToSql.Transpiler.Models
{
    public class CodeDef : EntityBase
    {
        public static CodeDef Instance = new CodeDef();

        public override string Name => "code";
        public override string DbTableName => "Code";

        private CodeDef()
        {
            Fields = new List<FieldDef>
            {
                new FieldDef(this, "codeId", "CodeID"),
                new FieldDef(this, "parentCodeId", "ParentCodeID"),
                new FieldDef(this, "codeStatusId", "CodeStatusID"),
                new FieldDef(this, "secureCode", "SecureCode")
            };
        }
    }
}
