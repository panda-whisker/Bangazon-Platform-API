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
    public class TestEmployees
    {
        [Fact]
        public async Task Test_Get_All_Employees()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/employees");


                string responseBody = await response.Content.ReadAsStringAsync();
                var employees = JsonConvert.DeserializeObject<List<Employee>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(employees.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_One_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/employees/1");


                string responseBody = await response.Content.ReadAsStringAsync();
                var employee = JsonConvert.DeserializeObject<Employee>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Ricky", employee.FirstName);
            }
        }

        [Fact]
        public async Task Test_Create_And_Delete_Employee()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                Employee Hacker = new Employee
                {
                    FirstName = "Ricky",
                    LastName = "McConnell",
                    DepartmentId = 1,
                    IsSupervisor = true,

                };
                var EmployeeAsJSON = JsonConvert.SerializeObject(Hacker);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                    "/api/employees",
                     new StringContent(EmployeeAsJSON, Encoding.UTF8, "application/json")

                );


                string responseBody = await response.Content.ReadAsStringAsync();
                var NewHacker = JsonConvert.DeserializeObject<Employee>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(Hacker.FirstName, NewHacker.FirstName);
                Assert.Equal(Hacker.LastName, NewHacker.LastName);


                var deleteResponse = await client.DeleteAsync($"/api/employees/{NewHacker.Id}");

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Employee()
        {
           
            string NewEmployee = "Ricky";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Employee ModifiedEmployee = new Employee
                {
                    FirstName = "Ricky",
                    LastName = "McConnell",
                    DepartmentId = 1,
                    IsSupervisor = true,
                };
                var ModifiedEmployeeAsJSON = JsonConvert.SerializeObject(ModifiedEmployee);

                var response = await client.PutAsync(
                    "/api/employees/1",
                    new StringContent(ModifiedEmployeeAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var Hacker = await client.GetAsync("/api/employees/1");
                Hacker.EnsureSuccessStatusCode();

                string GetEmployeeBody = await Hacker.Content.ReadAsStringAsync();
                Employee NewHacker = JsonConvert.DeserializeObject<Employee>(GetEmployeeBody);

                Assert.Equal(HttpStatusCode.OK, Hacker.StatusCode);
                Assert.Equal(NewEmployee, NewHacker.FirstName);
            }
        }




    }
}
        