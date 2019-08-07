using System;
using System.Net;
using Newtonsoft.Json;
using Xunit;
using BangazonAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace TestBangazonAPI
{
    public class TestComputers
    {
        [Fact]
        public async Task Test_Get_All_Computers()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/computers");


                string responseBody = await response.Content.ReadAsStringAsync();
                var computers = JsonConvert.DeserializeObject<List<Computer>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(computers.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Computer()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var response = await client.GetAsync("/api/computers/2");


                string responseBody = await response.Content.ReadAsStringAsync();
                var computer = JsonConvert.DeserializeObject<Computer>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(computer.Make, computer.Make);
                Assert.Equal(computer.Manufacturer, computer.Manufacturer);
                Assert.Equal(new DateTime(2019, 06, 06, 00, 00, 00, 0000000), computer.PurchaseDate);
                Assert.NotNull(computer);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Computer_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var response = await client.GetAsync("/api/computers/999999999");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                Computer Apple = new Computer
                {
                    Make = "MacBook Pro",
                    Manufacturer = "Apple",
                    PurchaseDate = new DateTime(2015, 08, 11, 00, 00, 00, 000),
                    DecomissionDate = new DateTime(2016, 07, 13, 00, 00, 00, 000)
                };
                var AppleAsJSON = JsonConvert.SerializeObject(Apple);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                    "/api/computers",
                    new StringContent(AppleAsJSON, Encoding.UTF8, "application/json")
                );


                string responseBody = await response.Content.ReadAsStringAsync();
                var NewApple = JsonConvert.DeserializeObject<Computer>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(Apple.Make, NewApple.Make);
                Assert.Equal(Apple.Manufacturer, NewApple.Manufacturer);
                // Keep this block in mind, if it were to fail again. 
                Assert.Equal(Apple.PurchaseDate, NewApple.PurchaseDate);
                Assert.Equal(Apple.DecomissionDate, NewApple.DecomissionDate);

                /*
                    ACT
                */
                var deleteResponse = await client.DeleteAsync($"/api/computers/{NewApple.Id}");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Computer()
        {
            // New eating habit value to change to and test
            string NewMake = "MacBook Air";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Computer ModifiedApple = new Computer
                {
                    Make = NewMake,
                    Manufacturer = "Apple",
                    PurchaseDate = new DateTime (2019, 06, 06, 00, 00, 00, 000),
                    DecomissionDate = new DateTime (2020, 08, 06, 00, 00, 00, 000)
                };
                var ModifiedAppleAsJSON = JsonConvert.SerializeObject(ModifiedApple);

                var response = await client.PutAsync(
                    "/api/computers/2",
                    new StringContent(ModifiedAppleAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var GetButter = await client.GetAsync("/api/computers/2");
                GetButter.EnsureSuccessStatusCode();

                string GetButterBody = await GetButter.Content.ReadAsStringAsync();
                Computer NewApple = JsonConvert.DeserializeObject<Computer>(GetButterBody);

                Assert.Equal(HttpStatusCode.OK, GetButter.StatusCode);
                Assert.Equal(NewMake, NewApple.Make);
            }
        }
    }
}

