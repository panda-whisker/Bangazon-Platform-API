﻿using System;
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
        public async Task<IActionResult> Get([FromRoute]string q, string _includes)
        {
            string sql = @"SELECT c.Id, c.FirstName, 
                                    c.LastName, 
                                    pt.Id AS PaymentTypeId, 
                                    pt.AcctNumber, 
                                    pt.CustomerId, 
                                    pt.Name,
                                    p.Id AS ProductId,
                                    p.ProductTypeId,
                                    p.CustomerId,
                                    p.Price,
                                    p.Title,
                                    p.Description,
                                    p.Quantity
                                    FROM Customer c
                                    LEFT JOIN PaymentType pt ON pt.CustomerId = c.Id
                                    LEFT JOIN Product p ON p.CustomerId = c.Id
                                    WHERE 2=2";

            if (q != null)
            {
                sql = $@"{sql} AND (
                    c.LastName LIKE @q
                    OR c.FirstName LIKE @q
                    )
                    ";

            }

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (q != null)
                    {
                        cmd.Parameters.Add(new SqlParameter("@q", $"%{q}%"));

                    }

                    SqlDataReader reader = await cmd.ExecuteReaderAsync();

                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {

                        Customer customer;
                        List<Product> products = new List<Product>();
                        List<PaymentType> paymentTypes = new List<PaymentType>();

                        if (!customers.Exists(a => a.Id == reader.GetInt32(reader.GetOrdinal("Id"))))
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                PaymentTypes = paymentTypes,
                                Products = products,
                                Orders = new List<Order>()
                            };
                            customers.Add(customer);
                        }
                        Customer customer1 = customers.Find(a => a.Id == reader.GetInt32(reader.GetOrdinal("Id")));

                        if (_includes == "payments" && !reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            PaymentType paymentType = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber"))
                            };

                            if (customer1.Id == paymentType.CustomerId && !customer1.Products.Exists(a => a.Id == reader.GetInt32(reader.GetOrdinal("PaymentTypeId"))))
                            {
                                customer1.PaymentTypes.Add(paymentType);
                            }
                        }
                        if (_includes == "products" && !reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            Product product = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("Id")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                            };
                            if (customer1.Id == product.CustomerId && !customer1.Products.Exists(a => a.Id == reader.GetInt32(reader.GetOrdinal("ProductId"))))
                            {
                                customer1.Products.Add(product);
                            }
                        }


                    }

                    reader.Close();

                    return Ok(customers);
                }
            }
        }

        // GET api/values/5
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get([FromRoute]int id, string _includes)
        {
            if (!CustomerExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }

            string sql = @"SELECT c.Id, c.FirstName, 
                                    c.LastName, 
                                    pt.Id AS PaymentTypeId, 
                                    pt.AcctNumber, 
                                    pt.CustomerId, 
                                    pt.Name,
                                    p.Id AS ProductId,
                                    p.ProductTypeId,
                                    p.CustomerId,
                                    p.Price,
                                    p.Title,
                                    p.Description,
                                    p.Quantity
                                    FROM Customer c
                                    LEFT JOIN PaymentType pt ON pt.CustomerId = c.Id
                                    LEFT JOIN Product p ON p.CustomerId = c.Id
                                    WHERE c.Id = @id";

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();


                    Customer customer = null;

                    List<Product> products = new List<Product>();
                    List<PaymentType> paymentTypes = new List<PaymentType>();

                    if (reader.Read())
                    {

                        customer = new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            PaymentTypes = paymentTypes,
                            Products = products,
                            Orders = new List<Order>()
                        };
                        if (_includes == "payments" && !reader.IsDBNull(reader.GetOrdinal("PaymentTypeId")))
                        {
                            PaymentType paymentType = new PaymentType
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("PaymentTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                AcctNumber = reader.GetInt32(reader.GetOrdinal("AcctNumber"))
                            };

                            if (customer.Id == paymentType.CustomerId && !customer.Products.Exists(a => a.Id == reader.GetInt32(reader.GetOrdinal("PaymentTypeId"))))
                            {
                                customer.PaymentTypes.Add(paymentType);
                            }
                        }
                        if (_includes == "products" && !reader.IsDBNull(reader.GetOrdinal("ProductId")))
                        {
                            Product product = new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("Id")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                            };
                            if (customer.Id == product.CustomerId && !customer.Products.Exists(a => a.Id == reader.GetInt32(reader.GetOrdinal("ProductId"))))
                            {
                                customer.Products.Add(product);
                            }
                        }
                    }
                    reader.Close();

                    return Ok(customer);
                }
            }
        }

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
                        VALUES (@firstName, @lastName)
                    ";
                    cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));
                    int newId = (int)await cmd.ExecuteScalarAsync();
                    customer.Id = newId;

                    return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
                }
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer customer)
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
                            SET FirstName = @firstName,
                            LastName = @lastName
                            WHERE Id = @id
                        ";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));

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

        //DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Customer WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

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