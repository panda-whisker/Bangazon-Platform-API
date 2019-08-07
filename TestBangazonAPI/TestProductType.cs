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

                //var deleteResponse = await client.DeleteAsync($"/api/productType/{NewShoe.Id}");

                //Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }


    }
}
