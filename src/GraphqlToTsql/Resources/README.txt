﻿The main ANTLR website is: https://www.antlr.org/
The ANTLR project website is: https://github.com/antlr/antlr4
The process for using ANTLR4 in C# is described here: https://github.com/antlr/antlr4/tree/master/runtime/CSharp

The compiled Java, antlr-4.8-complete.jar, was downloaded from: https://www.antlr.org/download.html 
(using the "Complete ANTLR 4.8 Java binaries jar" link). It is used for the build-time code generation.
See the Pre-Build event configured on this Translator project, and the resulting GraphqlParser/CodeGen folder.

The GraphQL grammer is Gql.g4, and was downloaded from: https://github.com/antlr/grammars-v4/tree/master/graphql 
and renamed as Gql.g4 (because the grammar name is used to name the code-gen'd classes and we wanted something short).


CODE GENERATION INSTRUCTIONS
The project includes the code-generated bits. But in case you need to regenerate them,
1. CD to src\GraphqlToSql\Resources
2. Execute this command:
     java -jar antlr-4.8-complete.jar -Dlanguage=CSharp -o ..\CodeGen Gql.g4 -package GraphqlToTsql.CodeGen
