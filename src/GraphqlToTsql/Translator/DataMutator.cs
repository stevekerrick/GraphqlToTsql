using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace GraphqlToTsql.Translator
{
    public interface IDataMutator
    {
        string Mutate(string dataJson, Term topTerm);
    }

    public class DataMutator : IDataMutator
    {
        /// <summary>
        /// Go through the DataJson, and replace values according to the
        /// Term structure from the parse.
        /// </summary>
        /// <returns>Updated DataJson</returns>
        public string Mutate(string dataJson, Term topTerm)
        {
            // Walking and mutating the DataJson is kind of expensive,
            // so exit early if no mutations are needed
            if (!QueryHasMutations(topTerm))
            {
                return dataJson;
            }

            // Walk the dataJson and do mutations
            var data = (JObject)JsonConvert.DeserializeObject(dataJson);
            MutateObject(data, topTerm);

            return JsonConvert.SerializeObject(data);
        }

        private bool QueryHasMutations(Term term)
        {
            if (term.Field?.MutatorFunc != null)
            {
                return true;
            }

            return term.Children.Any(QueryHasMutations);
        }

        private void MutateObject(JObject data, Term term)
        {
            foreach (var childTerm in term.Children)
            {
                if (data.ContainsKey(childTerm.Name))
                {
                    var childData = data[childTerm.Name];
                    switch (childData.Type)
                    {
                        case JTokenType.Object:
                            MutateObject((JObject)childData, childTerm);
                            break;
                        case JTokenType.Array:
                            MutateArray((JArray)childData, childTerm);
                            break;
                        default:
                            MutateProperty((JValue)childData, childTerm);
                            break;
                    }
                }
            }
        }

        private void MutateArray(JArray data, Term term)
        {
            foreach (var childData in data)
            {
                MutateObject((JObject)childData, term);
            }
        }

        private void MutateProperty(JValue data, Term term)
        {
            var info = $"{term.Name} = {data}";
            Console.WriteLine(info);

            if (term.Field.MutatorFunc != null)
            {
                var oldValue = data.Value == null ? null : data.Value.ToString();
                var newValue = term.Field.MutatorFunc(oldValue);
                data.Replace(new JValue(newValue));
            }
        }
    }
}
