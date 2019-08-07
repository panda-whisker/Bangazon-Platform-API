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
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
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

        // GET api/customers
        [HttpGet]
        public async Task<IActionResult> Get(string q, string _include)
        {
            string SqlCommandText = @" 
                SELECT   c.Id as customerId, c.FirstName as First, c.LastName as Last,
                    p.[Name] as paymentName, p.AcctNumber, p.Id
               
                    FROM customer c
                     JOIN PaymentType p ON c.id = p.CustomerId
                        WHERE 1 = 1
                 ";
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    if (q != null)
                    {
                        SqlCommandText = $@"{SqlCommandText} AND (
                             c.FirstName LIKE @q OR c.LastName LIKE @q
                                )";
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}"));
                    }


                    cmd.CommandText = SqlCommandText;
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        if (customers == null)
                        {
                            Customer customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("customerId")),
                                FirstName = reader.GetString(reader.GetOrdinal("First")),
                                LastName = reader.GetString(reader.GetOrdinal("Last")),

                                // You might have more columns
                            };
                            PaymentType payment = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("paymentId")),
                                Name = reader.GetString(reader.GetOrdinal("paymentName")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("paymentName"))
                            };

                            if (customers.Any(z => z.Id == customer.Id))
                            {
                                Customer ExistingCustomer = customers.Find(z => z.Id == customer.Id);
                                ExistingCustomer.Payments.Add(payment);
                            }
                            else
                            {
                                customer.Payments.Add(payment);
                                customers.Add(customer);
                            }


                            customers.Add(customer);
                        }
                        reader.Close();
                        return Ok(customers);


                    }
                }
            }
        }


       [HttpGet("{id}", Name = "GetCustomer")]
        public IActionResult Get([FromRoute] int id, string _include)

        {
            if (!CustomerExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            string SqlCommandText;
            {
                if (_include == "payments")
                {
                    SqlCommandText = @"SELECT c.Id,
                                                  c.FirstName,
                                                  c.LastName,
                                                  p.Id AS PaymentTypeId,
                                                  p.AcctNumber,
                                                  p.Name
                                           FROM Customer c
                                           LEFT OUTER JOIN PaymentType p
                                               ON p.CustomerId = c.Id";
                }
                else
                {
                    SqlCommandText = @"
                        SELECT c.Id, c.FirstName, c.LastName
                            FROM customer c";
                }

                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())

                    {
                        cmd.CommandText = @"
                            SELECT 
                                Id, FirstName , LastName
                                  FROM customer
                                    WHERE Id = @customerId";
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

                                    // You might have more columns
                                };

                            }

                            if (_include == "Products" && _include == "payments")
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal("Id")))
                                {
                                    customer.Payments.Add(
                                        new PaymentType
                                        {
                                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                            Name = reader.GetString(reader.GetOrdinal("Name")),
                                            AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber")),
                                            CustomerId = reader.GetInt32(reader.GetOrdinal("customerId"))
                                        });

                                }
                            }
                        }

                        reader.Close();

                        return Ok(customer);
                    }
                }
            } }

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
