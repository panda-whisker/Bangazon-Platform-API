using BangazonAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestBangazonAPI
{
    public class TestDepartments
    {
        [Fact]
        public async Task Test_Get_All_Departments()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */


                /*
                    ACT
                */
                var response = await client.GetAsync("/api/departments");


                string responseBody = await response.Content.ReadAsStringAsync();
                var departments = JsonConvert.DeserializeObject<List<Department>>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(departments.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Department()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */

                /*
                    ACT
                */
                var response = await client.GetAsync("/api/departments/1");


                string responseBody = await response.Content.ReadAsStringAsync();
                var department = JsonConvert.DeserializeObject<Department>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(department.Name, department.Name);
                Assert.Equal(department.Budget, department.Budget);
                Assert.NotNull(department);
            }
        }

        [Fact]
        public async Task Test_Create_Department()
        {
            using (var client = new APIClientProvider().Client)
            {
                /*
                    ARRANGE
                */
                Department HR = new Department
                {
                    Name = "Human Resources",
                    Budget = 7000
                };
                var HRAsJSON = JsonConvert.SerializeObject(HR);

                /*
                    ACT
                */
                var response = await client.PostAsync(
                    "/api/departments",
                    new StringContent(HRAsJSON, Encoding.UTF8, "application/json")
                );


                string responseBody = await response.Content.ReadAsStringAsync();
                var NewDepartment = JsonConvert.DeserializeObject<Department>(responseBody);

                /*
                    ASSERT
                */
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal(HR.Name, NewDepartment.Name);
                Assert.Equal(HR.Budget, NewDepartment.Budget);

                /*
                    ACT
                */
            }
        }

        [Fact]
        public async Task Test_Modify_Department()
        {
            // New eating habit value to change to and test
            string NewDept = "Management";

            using (var client = new APIClientProvider().Client)
            {
                /*
                    PUT section
                 */
                Department ModifiedDepartment = new Department
                {
                    Name = NewDept,
                    Budget = 30000
                };
                var ModifiedDeptAsJSON = JsonConvert.SerializeObject(ModifiedDepartment);

                var response = await client.PutAsync(
                    "/api/departments/3",
                    new StringContent(ModifiedDeptAsJSON, Encoding.UTF8, "application/json")
                );
                string responseBody = await response.Content.ReadAsStringAsync();

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                var GetDept = await client.GetAsync("/api/departments/3");
                GetDept.EnsureSuccessStatusCode();

                string GetDeptBody = await GetDept.Content.ReadAsStringAsync();
                Department NewDepart = JsonConvert.DeserializeObject<Department>(GetDeptBody);

                Assert.Equal(HttpStatusCode.OK, GetDept.StatusCode);
                Assert.Equal(NewDept, NewDepart.Name);
            }
        }
    }
}
