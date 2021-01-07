﻿using System.Collections.Generic;

namespace GraphqlToTsql
{
    public class RunnerResult
    {
        public string Tsql { get; set; }
        public Dictionary<string, object> TsqlParameters { get; set; }
        public string DataJson { get; set; }
        public string ParseError { get; set; }
        public string DbError { get; set; }
        public bool IsSuccessful => string.IsNullOrWhiteSpace(ParseError) || string.IsNullOrWhiteSpace(DbError);
        public List<Statistic> Statistics { get; set; }
    }

    public class Statistic
    {
        public string Name { get; set; }
        public long? Value { get; set; }

        public Statistic(string name, long? value)
        {
            Name = name;
            Value = value;
        }
    }
}
