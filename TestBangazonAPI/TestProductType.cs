using System;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TestBangazonAPI
{
    public class TestProductTypes
    {
        [Fact]
        public async Task Test_Get_All_ProductTypes()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/productType");


                string responseBody = await response.Content.ReadAsStringAsync();
                var productTypes = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(productTypes.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_ProductType()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/api/productType/2");


                string responseBody = await response.Content.ReadAsStringAsync();
                var productType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Electronics", productType.Name);
                Assert.NotNull(productType);
            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_ProductType()
        {

            using (var client = new APIClientProvider().Client)
            {
                ProductType Shoe = new ProductType
                {
                    Name = "Shoe"
                };
                var ShoeAsJSON = JsonConvert.SerializeObject(Shoe);

                var response = await client.PostAsync(
                    "/api/productType",
                    new StringContent(ShoeAsJSON, Encoding.UTF8, "application/json")
                    );

                string responseBody = await response.Content.ReadAsStringAsync();
                var NewShoe = JsonConvert.DeserializeObject<ProductType>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(Shoe.Name, NewShoe.Name);

                var deleteResponse = await client.DeleteAsync($"/api/productType/{NewShoe.Id}");

                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }
        [Fact]
        public async Task Test_Modify_PaymentType()
        {
            // New shoe name value to change to and test
            string NewName = "Loafer";

            using (var client = new APIClientProvider().Client)
            { 
                /*
                      PUT section
                   */
                ProductType ModifiedShoe = new ProductType
        {
            Name = NewName       
        };

        var ModifiedShoeAsJSON = JsonConvert.SerializeObject(ModifiedShoe);

        var response = await client.PutAsync(
            "/api/productType/4",
            new StringContent(ModifiedShoeAsJSON, Encoding.UTF8, "application/json")
        );
        string responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var GetShoe = await client.GetAsync("/api/productType/4");
        GetShoe.EnsureSuccessStatusCode();

                string GetShoeBody = await GetShoe.Content.ReadAsStringAsync();
        ProductType NewShoe = JsonConvert.DeserializeObject<ProductType>(GetShoeBody);

        Assert.Equal(HttpStatusCode.OK, GetShoe.StatusCode);
                Assert.Equal(NewName, NewShoe.Name);
            }
        }
         
        [Fact]
        public async Task Test_Get_NonExistant_ProductType_Fails()
        {
            
            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/api/productType/999999999");

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistant_ProductType_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                var deleteResponse = await client.DeleteAsync("/api/productType/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }


    }
}
