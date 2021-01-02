using AutoFixture;
using DemoEntities;
using GraphqlToTsql.Entities;
using GraphqlToTsql.Translator;
using GraphqlToTsql.Util;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GraphqlToTsqlTests
{
    [TestFixture]
    public class DataMutatorTests
    {
        private Fixture _fixture = new Fixture();

        [Test]
        public void NoMutationsNeededReturnsOriginalDataJsonTest()
        {
            // Build up the term tree
            var topTerm = Term.TopLevel();
            var product = ProductDef.Instance;

            var productField = Field.Row(product, "product", null);
            var productTerm = new Term(topTerm, productField, "product");
            topTerm.Children.Add(productTerm);

            var urnField = product.GetField("urn");
            var urnTerm = new Term(productTerm, urnField, "urn");
            productTerm.Children.Add(urnTerm);

            // Act
            var dataJson = "{foo: 1}";
            var dataMutator = new DataMutator();
            var mutatedJson = dataMutator.Mutate(dataJson, topTerm);

            // The original DataJson object should have been returned
            Assert.AreSame(dataJson, mutatedJson);
        }

        [Test]
        public void MutationsAreAppliedTest()
        {
            // Create a couple of cursors to use
            var lotId1 = _fixture.Create<int>();
            var cursorData1 = $"{lotId1}|Lot";
            var cursor1 = CursorUtility.CreateCursor(cursorData1);

            var lotId2 = _fixture.Create<int>();
            var cursorData2 = $"{lotId2}|Lot";
            var cursor2 = CursorUtility.CreateCursor(cursorData2);

            // Build up the term tree
            var topTerm = Term.TopLevel();
            var product = ProductDef.Instance;

            var productField = Field.Row(product, "product", null);
            var productTerm = new Term(topTerm, productField, "product");
            topTerm.Children.Add(productTerm);

            var urnField = product.GetField("urn");
            var urnTerm = new Term(productTerm, urnField, "urn");
            productTerm.Children.Add(urnTerm);

            var lotsField = product.GetField("lots");
            var lotsTerm = new Term(productTerm, lotsField, "lots");
            productTerm.Children.Add(lotsTerm);

            var lotNumberField = lotsField.Entity.GetField("lotNumber");
            var lotNumberTerm = new Term(lotsTerm, lotNumberField, "lotNumber");
            lotsTerm.Children.Add(lotNumberTerm);

            var lotCursorField = Field.Cursor(lotsField);
            var lotCursorTerm = new Term(lotsTerm, lotCursorField, "cursor");
            lotsTerm.Children.Add(lotCursorTerm);

            // Starting data
            var productUrn = _fixture.Create<string>();
            var lotNumber1 = _fixture.Create<string>();
            var lotNumber2 = _fixture.Create<string>();

            var startingData = new
            {
                product = new
                {
                    urn = productUrn,
                    lots = new[] {
                        new {
                            lotNumber = lotNumber1,
                            cursor = cursorData1
                        },
                        new {
                            lotNumber = lotNumber2,
                            cursor = cursorData2
                        },
                        new {
                            lotNumber = (string)null,
                            cursor = (string)null
                        }
                    }
                }
            };
            var startingDataJson = JsonConvert.SerializeObject(startingData);

            // Expected data
            var expectedData = new
            {
                product = new
                {
                    urn = productUrn,
                    lots = new[] {
                        new {
                            lotNumber = lotNumber1,
                            cursor = cursor1
                        },
                        new {
                            lotNumber = lotNumber2,
                            cursor = cursor2
                        },
                        new {
                            lotNumber = (string)null,
                            cursor = (string)null
                        }
                    }
                }
            };
            var expectedDataJson = JsonConvert.SerializeObject(expectedData);

            // Act
            var dataMutator = new DataMutator();
            var mutatedJson = dataMutator.Mutate(startingDataJson, topTerm);

            Assert.AreEqual(expectedDataJson, mutatedJson);
        }
    }
}
