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
    public class TestPaymentTypes
    {
        [Fact]
        public async Task Test_Get_All_PaymentTypes()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/paymentType");


                string responseBody = await response.Content.ReadAsStringAsync();
                var paymentTypes = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(paymentTypes.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_PaymentType()
        {

            using (var client = new APIClientProvider().Client)
            {
                var response = await client.GetAsync("/api/paymentType/4");


                string responseBody = await response.Content.ReadAsStringAsync();
                var paymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("mastercard", paymentType.Name);
                Assert.NotNull(paymentType);
            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_PaymentType()
        {

            using (var client = new APIClientProvider().Client)
            {
                PaymentType Visa = new PaymentType
                {
                    Name = "Visa",
                    AcctNumber = 123456789,
                    CustomerId = 1
                };
                var VisaAsJSON = JsonConvert.SerializeObject(Visa);

                var response = await client.PostAsync(
                    "/api/paymentType",
                    new StringContent(VisaAsJSON, Encoding.UTF8, "application/json")
                    );

                string responseBody = await response.Content.ReadAsStringAsync();
                var NewVisa = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(Visa.Name, NewVisa.Name);
                Assert.Equal(Visa.AcctNumber, NewVisa.AcctNumber);
                Assert.Equal(Visa.CustomerId, NewVisa.CustomerId);

                var deleteResponse = await client.DeleteAsync($"/api/paymentType/{NewVisa.Id}");

                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_PaymentType()
        {
            // New eating habit value to change to and test
            int NewAcctNumber = 135792468;

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                PaymentType ModifiedVisa = new PaymentType
                {
                    Name = "Visa",
                    AcctNumber = NewAcctNumber,
                    CustomerId = 2
                };
                var ModifiedVisaAsJSON = JsonConvert.SerializeObject(ModifiedVisa);

                var response = await client.PutAsync(
                    "/api/paymentType/5",
                    new StringContent(ModifiedVisaAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var GetVisa = await client.GetAsync("/api/paymentType/5");
                GetVisa.EnsureSuccessStatusCode();

                string GetVisaBody = await GetVisa.Content.ReadAsStringAsync();
                PaymentType NewVisa = JsonConvert.DeserializeObject<PaymentType>(GetVisaBody);

                Assert.Equal(HttpStatusCode.OK, GetVisa.StatusCode);
                Assert.Equal(NewAcctNumber, NewVisa.AcctNumber);
            }
        }
    }
}