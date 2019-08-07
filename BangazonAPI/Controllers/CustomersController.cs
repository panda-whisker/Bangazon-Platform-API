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
    public class CustomersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomersController(IConfiguration config)
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
        public async Task<IActionResult> Get( string q)
        {
            string SqlCommandText = @" 
                SELECT p.Id as paymentId, p.[Name] as paymentName, p.AcctNumber,
                 c.Id as customerId, c.FirstName as customerFirst, c.LastName as customerLast
               FROM PaymentType p
                JOIN customer c ON c.Id = p.Id; ";
            if (q != null)
            {
                SqlCommandText = $@"{SqlCommandText} WHERE (
                c.FirstName LIKE @q OR c.LastName = @q
                )";
            }

            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())

                    
                    {
                        cmd.CommandText = SqlCommandText;
                        if (q != null)
                        {
                            cmd.Parameters.Add(new SqlParameter("@q", $"%{q}"));
                        }
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        List<Customer> customers = new List<Customer>();
                        while (reader.Read())
                        {
                            Customer customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                paymentId = reader.GetInt32(reader.GetOrdinal("Id"))
                                // You might have more columns
                            };
                            PaymentType payment = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber"))
                            };
                            if( customers.Any(z => z.Id == customer.Id))
                            {
                                //?
                                Customer ExistingCustomer = customers.Find(z => z.Id == customer.Id);
                                ExistingCustomer.payments.Add(payment);
                            }
                            else
                            {
                                customer.payments.Add(payment);
                                customers.Add(customer);
                            }
                        }

                        reader.Close();

                        return Ok(customers);
                    }
                }
            }
        }
        // GET api/values/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get([FromRoute] int id, string _include = null)

        {
            if (!CustomerExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            string CommandText;

            if (_include == "products" && _include == "payments")
            {
                CommandText = @"
                SELECT p.Id as paymentId, p.[Name] as paymentName, p.AcctNumber,
                 c.Id as customerId, c.FirstName as customerFirst, c.LastName as customerLast
               FROM PaymentType p
                JOIN customer c ON c.Id = p.Id;";
            }
            else
            {
                CommandText = @"
                SELECT p.Id as PaymentId, p.[Name] as paymentType, p.AcctNumber as PaymentAcc
                FROM PaymentType p";
            }

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())

                {
                    cmd.CommandText = $"{CommandText} WHERE p.Id = c.Id";
                    cmd.Parameters.Add(new SqlParameter("@customerId", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer customer = null;
                    while (reader.Read())
                    {
                        if (customer == null)
                        {
                            Customer customer1 = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                paymentId = reader.GetInt32(reader.GetOrdinal("paymentId"))
                                // You might have more columns
                            };

                        }

                        if (_include == "Products" && _include == "payments")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("PaymentId")))
                            {
                                customer.payments.Add(
                                    new PaymentType
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                        Name = reader.GetString(reader.GetOrdinal("Name")),
                                        AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                                        CustomerId = reader.GetInt32(reader.GetOrdinal("customerId"))
                                    });

                            }
                        }


                        reader.Close();

                        return Ok(customer);
                    }
                }
            }
        }

        // POST api/customers
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = @"
                        INSERT INTO Customer (FirstName, LastName)
                        OUTPUT INSERTED.Id
                        VALUES (@Firstname, @LastName)
                    ";
                    cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@LastName", customer.LastName));

                    customer.Id = (int) await cmd.ExecuteScalarAsync();
                    var newId = (int)customer.Id;

                    return CreatedAtRoute("GetCustomer", new { id = customer.Id }, customer);
                }
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Customer customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                            UPDATE Customer
                            SET FirstName = @firstName
                            -- Set the remaining columns here
                            WHERE Id = @id
                        ";
                        cmd.Parameters.Add(new SqlParameter("@id", customer.Id));
                        cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));

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
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        private bool CustomerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // More string interpolation
                    cmd.CommandText = "SELECT Id FROM Customer WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    return reader.Read();
                }
            }
        }
    }
}
