using System;
using System.Collections.Generic;

using Microsoft.Data.SqlClient; 
using CsharpProject.Entities;

namespace CsharpProject.Repositories
{
    public class MyAdoNetRepository {
        // The connection string used by EF Core (hard-coded for the sake of example)
        private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CsharpProject;Integrated Security=True";


        // (READ) - Querying all products together with their categories
        public List<Product> GetAllProductsWithCategory()
        {
            var products = new List<Product>();

            // Establishing contact
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Writing an SQL command
                string sql = @"
                    SELECT p.ID, p.Name, p.Price, p.Stock, p.VATPercentage, p.Description, p.CategoryID, c.ID AS CatID, c.Name AS CatName
                    FROM Product p
                    INNER JOIN Category c ON p.CategoryID = c.ID";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Establishing contact
                    connection.Open();

                    // Executing queries and reading (DataReader)
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Manual Mapping - SQL Columns -> C# Object
                            var product = new Product
                            {
                                ID = (int)reader["ID"],
                                Name = reader["Name"].ToString(),
                                Price = (decimal)reader["Price"],
                                Stock = (int)reader["Stock"],
                                Vatpercentage = (int)reader["VATPercentage"],
                                Description = reader["Description"].ToString(),
                                CategoryID = (int)reader["CategoryID"],
                                
                                Category = new Category 
                                {
                                    ID = (int)reader["CatID"], // We use alias names from SQL
                                    Name = reader["CatName"].ToString()
                                }
                            };
                            
                            products.Add(product);
                        }
                    }
                }
            } // At the end of the using block, connection.Close() is automatically called.

            return products;
        }


        // (FIND) - Retrieving an order by ID
        public Order GetOrderById(int orderId)
        {
            Order order = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Parameterized SQL for security! (@Id)
                string sql = "SELECT * FROM [Order] WHERE ID = @Id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Adding a parameter (SQL injection protection)
                    command.Parameters.AddWithValue("@Id", orderId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = new Order
                            {
                                ID = (int)reader["ID"],
                                Date = (DateTime)reader["Date"],
                                Deadline = (DateTime)reader["Deadline"],
                                Status = reader["Status"].ToString(),
                                PaymentMethod = reader["PaymentMethod"].ToString(),
                                ProductID = (int)reader["ProductID"]
                            };
                        }
                    }
                }
            }
            return order;
        }


        // (CREATE) - Insert new category
        public void AddCategory(Category newCategory)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql = "INSERT INTO Category (Name) VALUES (@Name)";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Name", newCategory.Name);

                    connection.Open();
                    
                    command.ExecuteNonQuery(); 
                }
            }
        }

        public void AddOrder(Order newOrder)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO [Order] ([Date], Deadline, [Status], PaymentMethod, ProductID)
                    VALUES (@Date, @Deadline, @Status, @PaymentMethod, @ProductID);
                    
                    -- Optional: Return the generated ID
                    SELECT CAST(scope_identity() AS int);";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    // Adding parameters
                    command.Parameters.AddWithValue("@Date", newOrder.Date);
                    command.Parameters.AddWithValue("@Deadline", newOrder.Deadline);
                    command.Parameters.AddWithValue("@Status", newOrder.Status);
                    command.Parameters.AddWithValue("@PaymentMethod", newOrder.PaymentMethod);
                    command.Parameters.AddWithValue("@ProductID", newOrder.ProductID);

                    connection.Open();

                    // ExecuteScalar: Waiting for the ID to be returned (due to the SELECT after INSERT)
                    int newId = (int)command.ExecuteScalar();
                    newOrder.ID = newId; // Update the C# object
                }
            }
        }


        // (JOIN) - Order inquiry with product data
        public Order GetOrderWithProductDetails(int orderId)
        {
            Order order = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT o.ID AS OrderID, o.Date, o.Deadline, o.Status, o.PaymentMethod, p.ID AS ProdID, p.Name AS ProdName, p.Price, p.Stock
                    FROM [Order] o
                    INNER JOIN Product p ON o.ProductID = p.ID
                    WHERE o.ID = @Id";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", orderId);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Instantiation of the main entity (Order)
                            order = new Order
                            {
                                ID = (int)reader["OrderID"],
                                Date = (DateTime)reader["Date"],
                                Deadline = (DateTime)reader["Deadline"],
                                Status = reader["Status"].ToString(),
                                PaymentMethod = reader["PaymentMethod"].ToString(),
                                ProductID = (int)reader["ProdID"],
                                
                                Product = new Product
                                {
                                    ID = (int)reader["ProdID"],
                                    Name = reader["ProdName"].ToString(),
                                    Price = (decimal)reader["Price"],
                                    Stock = (int)reader["Stock"]
                                }
                            };
                        }
                    }
                }
            }
            return order;
        }
        

        // (TRANSACTION HANDLING) - Stock reduction
        public void PlaceOrderWithTransaction(Order newOrder)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Start transaction
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Insert order (the transaction must be passed to the command!)
                    string insertSql = @"INSERT INTO [Order] (Date, Deadline, Status, PaymentMethod, ProductID)
                                         VALUES (@Date, @Deadline, @Status, @PaymentMethod, @ProductID);";
                    
                    using (SqlCommand cmdOrder = new SqlCommand(insertSql, connection, transaction))
                    {
                        cmdOrder.Parameters.AddWithValue("@Date", newOrder.Date);
                        cmdOrder.Parameters.AddWithValue("@Deadline", newOrder.Deadline);
                        cmdOrder.Parameters.AddWithValue("@Status", newOrder.Status);
                        cmdOrder.Parameters.AddWithValue("@PaymentMethod", newOrder.PaymentMethod);
                        cmdOrder.Parameters.AddWithValue("@ProductID", newOrder.ProductID);
                        cmdOrder.ExecuteNonQuery();
                    }

                    // Reduce inventory (UPDATE)
                    string updateSql = "UPDATE Product SET Stock = Stock - 1 WHERE ID = @Pid";
                    
                    using (SqlCommand cmdUpdate = new SqlCommand(updateSql, connection, transaction))
                    {
                        cmdUpdate.Parameters.AddWithValue("@Pid", newOrder.ProductID);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    // Finalize it.
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // If there was an error, reverse everything (no order, no stock reduction).
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}