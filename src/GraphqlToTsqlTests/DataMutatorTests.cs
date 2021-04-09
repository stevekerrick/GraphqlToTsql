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
        public void NoMutationsNeededRetnamesOriginalDataJsonTest()
        {
            // Build up the term tree
            var rootTerm = Term.RootTerm();
            var product = ProductEntity.Instance;

            var productField = Field.Row(product, "product", null);
            var productTerm = new Term(rootTerm, productField, "product");
            rootTerm.Children.Add(productTerm);

            var nameField = product.GetField("name");
            var nameTerm = new Term(productTerm, nameField, "name");
            productTerm.Children.Add(nameTerm);

            // Act
            var dataJson = "{foo: 1}";
            var dataMutator = new DataMutator();
            var mutatedJson = dataMutator.Mutate(dataJson, rootTerm);

            // The original DataJson object should have been retnameed
            Assert.AreSame(dataJson, mutatedJson);
        }

        [Test]
        public void MutationsAreAppliedTest()
        {
            // Create a couple of cursors to use
            var order1 = _fixture.Create<int>();
            var cursorData1 = CursorUtility.CursorDataFunc(new Value(order1), "Order");
            var cursor1 = CursorUtility.CreateCursor(cursorData1);

            var order2 = _fixture.Create<int>();
            var cursorData2 = CursorUtility.CursorDataFunc(new Value(order2), "Order");
            var cursor2 = CursorUtility.CreateCursor(cursorData2);

            // Build up the term tree
            var rootTerm = Term.RootTerm();
            var seller = SellerEntity.Instance;

            var sellerField = Field.Row(seller, "seller", null);
            var sellerTerm = new Term(rootTerm, sellerField, "seller");
            rootTerm.Children.Add(sellerTerm);

            var nameField = seller.GetField("name");
            var nameTerm = new Term(sellerTerm, nameField, "name");
            sellerTerm.Children.Add(nameTerm);

            var ordersField = seller.GetField("orders");
            var ordersTerm = new Term(sellerTerm, ordersField, "orders");
            sellerTerm.Children.Add(ordersTerm);

            var orderIdField = ordersField.Entity.GetField("id");
            var orderIdTerm = new Term(ordersTerm, orderIdField, "id");
            ordersTerm.Children.Add(orderIdTerm);

            var orderCursorField = Field.Cursor(ordersField);
            var orderCursorTerm = new Term(ordersTerm, orderCursorField, "cursor");
            ordersTerm.Children.Add(orderCursorTerm);

            // Starting data
            var sellerName = _fixture.Create<string>();
            var orderId1 = _fixture.Create<int>();
            var orderId2 = _fixture.Create<int>();

            var startingData = new
            {
                seller = new
                {
                    name = sellerName,
                    orders = new[] {
                        new {
                            id = orderId1,
                            cursor = cursorData1
                        },
                        new {
                            id = orderId2,
                            cursor = cursorData2
                        }
                    }
                }
            };
            var startingDataJson = JsonConvert.SerializeObject(startingData);

            // Expected data
            var expectedData = new
            {
                seller = new
                {
                    name = sellerName,
                    orders = new[] {
                        new {
                            id = orderId1,
                            cursor = cursor1
                        },
                        new {
                            id = orderId2,
                            cursor = cursor2
                        }
                    }
                }
            };
            var expectedDataJson = JsonConvert.SerializeObject(expectedData);

            // Act
            var dataMutator = new DataMutator();
            var mutatedJson = dataMutator.Mutate(startingDataJson, rootTerm);

            Assert.AreEqual(expectedDataJson, mutatedJson);
        }
    }
}
