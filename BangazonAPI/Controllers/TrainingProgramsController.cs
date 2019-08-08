using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingProgramsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TrainingProgramsController(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {

            //Join Table for Employees are completed
            //Department needs to be written
            string EmployeeSQL
            = @"SELECT tp.id AS TPId, Name, StartDate, EndDate, MaxAttendees, DepartmentId, IsSupervisor,
              e.id AS EmployeeId, FirstName, LastName FROM TrainingProgram tp
               
              LEFT JOIN EmployeeTraining et on et.TrainingProgramId = tp.id
              LEFT JOIN Employee e on et.EmployeeId = e.Id";

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = EmployeeSQL;

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    //List not taking arguments for ContainKey below
                    Dictionary<int, TrainingProgram> TrainingJoinList = new Dictionary<int, TrainingProgram>();

                    while (reader.Read())
                    {
                        int TrainingProgramId = reader.GetInt32(reader.GetOrdinal("TPId"));
                        bool NullEmployeeId = reader.IsDBNull(reader.GetOrdinal("EmployeeId"));

                        if (!TrainingJoinList.ContainsKey(TrainingProgramId))
                        {
                            TrainingJoinList[TrainingProgramId] = new TrainingProgram

                            {
                                Id = reader.GetInt32(reader.GetOrdinal("TPId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                            };
                        }

                        if (NullEmployeeId == false)
                        {

                            TrainingJoinList[TrainingProgramId].AttendingEmployees.Add(new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId"))
                            });
                        }

                    }

                    List<TrainingProgram> trainingPrograms = TrainingJoinList.Values.ToList();
                    reader.Close();

                    return Ok(trainingPrograms);
                }
            }
        }

        // GET api/values/5
        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name, StartDate, EndDate, MaxAttendees 
                        FROM TrainingProgram
                        WHERE Id = @id"; ;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    TrainingProgram trainingProgram = null;
                    if (reader.Read())
                    {
                        trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees"))
                        };
                    }

                    reader.Close();

                    return Ok(trainingProgram);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = @"
                        INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees)
                        OUTPUT INSERTED.Id
                        VALUES (@Name, @StartDate, @EndDate, @MaxAttendees);
                    ";
                    cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));

                    trainingProgram.Id = (int)await cmd.ExecuteScalarAsync();

                    return CreatedAtRoute("GetTrainingProgram", new { id = trainingProgram.Id }, trainingProgram);
                }
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM TrainingProgram WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }

            catch (Exception)
            {
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // PUT api/computers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TrainingProgram trainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                             UPDATE TrainingProgram
                             SET Name = @Name,
                                 StartDate = @StartDate,
                                 EndDate = @EndDate,
                                 MaxAttendees = @MaxAttendees
                             WHERE Id = @id
                         ";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@Name", trainingProgram.Name));
                        cmd.Parameters.Add(new SqlParameter("@StartDate", trainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@EndDate", trainingProgram.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@MaxAttendees", trainingProgram.MaxAttendees));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }

                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool TrainingProgramExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //More string interpolation
                    cmd.CommandText = "SELECT Id FROM TrainingProgram WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return reader.Read();
                }
            }


        }
    }




}
