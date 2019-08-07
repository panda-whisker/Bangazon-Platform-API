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
                TrainingProgram Program = new TrainingProgram
                {
                    Name = "Treveccll",
                    StartDate = new DateTime(2015, 08, 11, 00, 00, 00, 000),
                    EndDate = new DateTime(2016, 07, 13, 00, 00, 00, 000),
                    MaxAttendees = 55,
                    
                };
                var ProgramAsJSON = JsonConvert.SerializeObject(Program);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                    "/api/trainingPrograms",
                     new StringContent(ProgramAsJSON, Encoding.UTF8, "application/json")

                );


                string responseBody = await response.Content.ReadAsStringAsync();
                var NewProgram = JsonConvert.DeserializeObject<TrainingProgram>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(Program.Name, NewProgram.Name);
                Assert.Equal(Program.MaxAttendees, NewProgram.MaxAttendees);


                var deleteResponse = await client.DeleteAsync($"/api/trainingPrograms/{NewProgram.Id}");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_TrainingProgram()
        {
            // New eating habit value to change to and test
            string NewProgram = "Vanderbilt";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
               TrainingProgram ModifiedProgram = new TrainingProgram
                {
                    Name = NewProgram,              
                    StartDate = new DateTime(2019, 06, 06, 00, 00, 00, 000),
                    EndDate = new DateTime(2020, 08, 06, 00, 00, 00, 000)
                };
                var ModifiedProgramAsJSON = JsonConvert.SerializeObject(ModifiedProgram);

                var response = await client.PutAsync(
                    "/api/trainingprograms/1",
                    new StringContent(ModifiedProgramAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var Training = await client.GetAsync("/api/trainingprograms/1");
                Training.EnsureSuccessStatusCode();

                string GetTrainingBody = await Training.Content.ReadAsStringAsync();
                TrainingProgram NewTraining = JsonConvert.DeserializeObject<TrainingProgram>(GetTrainingBody);

                Assert.Equal(HttpStatusCode.OK, Training.StatusCode);
                Assert.Equal(NewProgram, NewTraining.Name);
            }
        }


    }
}
