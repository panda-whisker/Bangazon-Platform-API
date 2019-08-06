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
    public class TestTrainingPrograms
    {
        [Fact]
        public async Task Test_Get_All_TrainingPrograms()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/trainingPrograms");


                string responseBody = await response.Content.ReadAsStringAsync();
                var trainingPrograms = JsonConvert.DeserializeObject<List<TrainingProgram>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(trainingPrograms.Count > 0);
            }
        }
        [Fact]
        public async Task Test_Get_One_TrainingProgram()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/trainingPrograms/1");


                string responseBody = await response.Content.ReadAsStringAsync();
                var trainingProgram = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("York University", trainingProgram.Name);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_TrainingProgram()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                TrainingProgram Berry = new TrainingProgram
                {
                    Name = "Trevecca",
                    StartDate = new DateTime (2019,1,2, 00, 00, 00, 000),
                    EndDate = new DateTime(2019, 1, 3, 00, 00, 00, 000),
                    MaxAttendees = 55,
                    
                };
                var BerryAsJSON = JsonConvert.SerializeObject(Berry);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                    "/api/trainingPrograms"
                   
                    
                );


                string responseBody = await response.Content.ReadAsStringAsync();
                var NewBerry = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(Berry.Name, NewBerry.Name);
                Assert.Equal(Berry.StartDate, NewBerry.StartDate);
                Assert.Equal(Berry.EndDate, NewBerry.EndDate);
                Assert.Equal(Berry.MaxAttendees, NewBerry.MaxAttendees);
                

                /*
                    ACT
                */
                var deleteResponse = await client.DeleteAsync($"/api/trainingPrograms/{NewBerry.Id}");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }



    }
}
