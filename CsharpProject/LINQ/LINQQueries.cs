using CsharpProject.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using static CsharpProject.LINQQUeries;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CsharpProject
{
    class Program
    {
        static void Main(string[] args)
        {
            LINQQUeries queries = new LINQQUeries();
            queriesCUD cudQ = new queriesCUD();
            queriesREAD readQ = new queriesREAD();

            cudQ.AddProductWithNewCategory();
            cudQ.CreateBulkOrders();
            cudQ.IncreaseElectronicsPrices();
        }
    }

    public class LINQQUeries {
        // I. Data modification (CUD Operations)
        public class queriesCUD
        {
            // 1.Adding a New Product and Category (INSERT)
            public void AddProductWithNewCategory()
            {
                Console.WriteLine("\n--- AddProductWithNewCategory() was called! ---");
                using (var db = new MyDbContext())
                {
                    var existingCategory = db.Categories.FirstOrDefault(c => c.Name == "Gamer Accessories");
                    if (existingCategory != null)
                    {
                        Console.WriteLine("!!! Category 'Gamer Accessories' already exists !!!");
                        return;
                    }

                    var newCategory = new Category { Name = "Gamer Accessories" };

                    var existingProduct = db.Products.FirstOrDefault(p => p.Name == "RGB Mousepad XXL");
                    if (existingProduct != null)
                    {
                        Console.WriteLine("!!! Product 'RGB Mousepad XXL' already exists !!!");
                        return;
                    }

                    var newProduct = new Product {
                        Name = "RGB Mousepad XXL",
                        Price = 15000m,
                        Stock = 50,
                        Vatpercentage = 27,
                        Description = "<item><desc>Illuminated mouse pad</desc></item>",
                        Category = newCategory,
                    };

                    db.Products.Add(newProduct);
                    db.SaveChanges();

                    Console.WriteLine($"Success! New category ID: {newCategory.ID}, New product ID: {newProduct.ID}");
                }
            }

            // 2. Bulk Order Entry (INSERT)
            public void CreateBulkOrders()
            {
                Console.WriteLine("\n--- CreateBulkOrders() was called! ---");
                using (var db = new MyDbContext())
                {
                    // Lekérjük a legdrágább terméket
                    var mostValuableProduct = db.Products
                        .OrderByDescending(p => p.Price)
                        .FirstOrDefault();

                    if (mostValuableProduct == null)
                    {
                        Console.WriteLine("!!! No Product in the database to assign orders (mostValuableProduct = null) !!!");
                        return;
                    }

                    // --- CHECK (DUPLICATION FILTERING) ---
                    // We check whether there is already an order for this product that is typical for bulk insert.
                    // It is sufficient to check one specific order (e.g., "New" + "Credit Card")..
                    bool alreadyExists = db.Orders.Any(o =>
                        o.ProductID == mostValuableProduct.ID &&
                        o.Status == "New" &&
                        o.PaymentMethod == "Credit Card");

                    if (alreadyExists) {
                        Console.WriteLine("!!! A bulk order has already been placed for this product !!!");
                    } else {
                        var newOrders = new List<Order>
                        {
                            new Order { Date = DateTime.Now, Deadline = DateTime.Now.AddDays(3), Status = "New", PaymentMethod = "Credit Card", ProductID = mostValuableProduct.ID },
                            new Order { Date = DateTime.Now, Deadline = DateTime.Now.AddDays(5), Status = "Processing", PaymentMethod = "Bank Transfer", ProductID = mostValuableProduct.ID },
                            new Order { Date = DateTime.Now.AddDays(-1), Deadline = DateTime.Now.AddDays(2), Status = "Shipped", PaymentMethod = "Credit Card", ProductID = mostValuableProduct.ID }
                        };

                        db.Orders.AddRange(newOrders);
                        db.SaveChanges();
                    }

                    var allOrdersForProduct = db.Orders
                        .Include(o => o.Product)
                        .Where(x => x.ProductID == mostValuableProduct.ID)
                        .ToList();

                    if (allOrdersForProduct.Any()) {
                        Console.WriteLine($"\nCurrent orders for the product {mostValuableProduct.Name}:");
                        foreach (var a in allOrdersForProduct)
                        {
                            Console.WriteLine($" - Order ID: {a.ID}, Status: {a.Status}, Payment: {a.PaymentMethod}");
                        }
                    } else {
                        Console.WriteLine("!!! No orders for this product !!!");
                    }
                }
            }

            // 3. Price increase (UPDATE - Batch Update)
            public void IncreaseElectronicsPrices()
            {
                Console.WriteLine("\n--- IncreaseElectronicsPrices() was called! ---");
                using (var db = new MyDbContext())
                {    
                    var potentialProducts = db.Products
                        .Include(p => p.Category)
                        .Where(x => (x.Category.Name == "Smartphones and Tablets" || x.Category.Name == "TV and Entertainment") && x.Stock < 25).ToList();

                    // 1. Query the products, BUT add a filter:
                    // Only request those that do NOT YET have our mark ("[RAISED]") in their description.
                    var productsToUpdate = potentialProducts
                         .Where(x => x.Description != null && !x.Description.Contains("[RAISED]"))
                         .ToList();

                    if (!productsToUpdate.Any())
                    {
                        Console.WriteLine("!!! No products to update (or we have already raised the price of all of them earlier) !!!");
                        return;
                    }

                    foreach (var p in productsToUpdate)
                    {
                        p.Price = p.Price * 1.25m;

                        // 2. Mark the product! 
                        // Add to the description that it is now complete.
                        // This way, it will not be included in the list during the next run due to the above !Contains(...).
                        p.Description += " [RAISED]";
                    }

                    db.SaveChanges();
                    Console.WriteLine($"{productsToUpdate.Count} db electronic product prices increased and marked with '[RAISED]' tag.");
                }
            }

            // 4. Status Update (UPDATE - Single)
            public void UpdateOrderStatus()
            {
                Console.WriteLine("\n--- UpdateOrderStatus() was called! ---");
                using (var db = new MyDbContext())
                {

                }
            }

            // 5. Deleting Discontinued Products (DELETE - Conditional)
            public void DeleteDiscontinuedProducts()
            {
                Console.WriteLine("\n--- DeleteDiscontinuedProducts() was called! ---");
                using (var db = new MyDbContext())
                {

                }
            }

            // 6. Deleting an Incorrect Order (DELETE - Specific)
            public void DeleteCancelledOrder()
            {
                Console.WriteLine("\n--- DeleteCancelledOrder() was called! ---");
                using (var db = new MyDbContext())
                {

                }
            }
        }
        

        // II.Queries (READ)
        public class queriesREAD
        {
            // 7. "Bestseller" Categories (GroupBy + Aggregation)

            // 8. "Forgotten" Orders (Filter + Date)

            // 9. Expensive Categories (Any + All)

            // 10. Payment Method Statistics (GroupBy + Count)

            // 11. The "Most Valuable" Current Order (OrderBy + Include + FirstOrDefault)

            // 12. Projection to Anonymous Type (Select new { ... })
            
        }
    }
}