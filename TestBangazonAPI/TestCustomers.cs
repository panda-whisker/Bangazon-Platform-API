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
    public class TestCustomers
    {
        [Fact]
        public async Task Test_Get_All_Customers()
        {
            using (var client = new APIClientProvider().Client)
            {
                
                
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/customer");


                string responseBody = await response.Content.ReadAsStringAsync();
                var customers = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customers.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {


                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/customer");


                string responseBody = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<List<Customer>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(customer.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Post_Customer()
        {
            using (var client = new APIClientProvider().Client)
            {
                Customer Jimmy = new Customer
                {
                    FirstName = "david",
                    LastName = "mike"
                };
                var JimmyAsJSON = JsonConvert.SerializeObject(Jimmy);

                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.PostAsync("/api/customer",
                    new StringContent(JimmyAsJSON, Encoding.UTF8, "application/json"));


                string responseBody = await response.Content.ReadAsStringAsync();
                var newCustomer = JsonConvert.DeserializeObject<Customer>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(Jimmy.FirstName, newCustomer.FirstName);
            }
        }
    }
}
