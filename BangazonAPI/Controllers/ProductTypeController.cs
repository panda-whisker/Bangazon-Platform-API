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
    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
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

        // GET: api/productType
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, Name  
                                        FROM ProductType";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<ProductType> productTypes = new List<ProductType>();

                    while (reader.Read())
                    {
                        ProductType productType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),                          
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            
                        };

                        productTypes.Add(productType);
                    }
                    reader.Close();

                    return Ok(productTypes);
                }
            }
        }

        // GET: api/productType/2
        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, [Name]
                                        FROM ProductType
                                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    ProductType productType = null;

                    if (reader.Read())
                    {
                        productType = new ProductType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),                            
                            Name = reader.GetString(reader.GetOrdinal("Name"))                           
                        };
                    }
                    reader.Close();

                    return Ok(productType);
                }
            }
        }

        // POST: api/paymentType
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType productType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO productType ([Name])
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";                   
                    cmd.Parameters.Add(new SqlParameter("@name", productType.Name));
          
                    int newId = (int)await cmd.ExecuteScalarAsync();
                    productType.Id = newId;
                    return CreatedAtRoute("GetProductType", new { id = newId }, productType);
                }
            }
        }

        // PUT: api/paymentType/5

    }
}