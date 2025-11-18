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
            queriesCUD cudQ = new queriesCUD();
            cudQ.AddProductWithNewCategory();
            cudQ.CreateBulkOrders();
            cudQ.IncreaseElectronicsPrices();
            cudQ.UpdateOrderStatus();
            cudQ.DeleteDiscontinuedProducts();
            cudQ.DeleteCancelledOrder(20);

            queriesREAD readQ = new queriesREAD();
            readQ.GetBestsellerCategories();
            readQ.GetLateOrders();
            readQ.GetExpensiveCategories();
            readQ.GetPaymentStats();
            readQ.GetMostValuableActiveOrder();
            readQ.GetSimpleBookList();
        }
    }

    public class LINQQUeries
    {
        // I. Data modification (CUD Operations)
        public class queriesCUD
        {
            public queriesCUD()
            {
                Console.WriteLine("====<>====  \tqueriesCUD\t  ====<>==== ");
            }

            // 1.Adding a New Product and Category (INSERT)
            public void AddProductWithNewCategory()
            {
                Console.WriteLine("\n--- 1. AddProductWithNewCategory() was called! ---");
                using (var db = new MyDbContext())
                {
                    var existingCategory = db.Categories
                        .FirstOrDefault(c => c.Name == "Gamer Accessories");
                    if (existingCategory != null)
                    {
                        Console.WriteLine("!!! ERROR: Category 'Gamer Accessories' already exists !!!");
                        return;
                    }

                    var newCategory = new Category { Name = "Gamer Accessories" };

                    var existingProduct = db.Products
                        .FirstOrDefault(p => p.Name == "RGB Mousepad XXL");
                    if (existingProduct != null)
                    {
                        Console.WriteLine("!!! ERROR: Product 'RGB Mousepad XXL' already exists !!!");
                        return;
                    }

                    var newProduct = new Product
                    {
                        Name = "RGB Mousepad XXL",
                        Price = 15000m,
                        Stock = 50,
                        Vatpercentage = 27,
                        Description = "<item><desc>Illuminated mouse pad</desc></item>",
                        Category = newCategory,
                    };

                    db.Products.Add(newProduct);
                    db.SaveChanges();

                    Console.WriteLine($"New category ID: {newCategory.ID}, New product ID: {newProduct.ID}");
                }
            }

            // 2. Bulk Order Entry (INSERT)
            public void CreateBulkOrders()
            {
                Console.WriteLine("\n--- 2. CreateBulkOrders() was called! ---");
                using (var db = new MyDbContext())
                {
                    // Lekérjük a legdrágább terméket
                    var mostValuableProduct = db.Products
                        .OrderByDescending(p => p.Price)
                        .FirstOrDefault();

                    if (mostValuableProduct == null)
                    {
                        Console.WriteLine("!!! ERROR: No Product in the database to assign orders (mostValuableProduct = null) !!!");
                        return;
                    }

                    // --- CHECK (DUPLICATION FILTERING) ---
                    // We check whether there is already an order for this product that is typical for bulk insert.
                    // It is sufficient to check one specific order (e.g., "New" + "Credit Card")..
                    bool alreadyExists = db.Orders.Any(o =>
                        o.ProductID == mostValuableProduct.ID &&
                        o.Status == "New" &&
                        o.PaymentMethod == "Credit Card");

                    if (alreadyExists)
                    {
                        Console.WriteLine("!!! ERROR: A bulk order has already been placed for this product !!!");
                    }
                    else
                    {
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

                    if (allOrdersForProduct.Any())
                    {
                        Console.WriteLine($"\nCurrent orders for the product {mostValuableProduct.Name}:");
                        foreach (var a in allOrdersForProduct)
                        {
                            Console.WriteLine($" - Order ID: {a.ID}, Status: {a.Status}, Payment: {a.PaymentMethod}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("!!! ERROR: No orders for this product !!!");
                    }
                }
            }

            // 3. Price increase (UPDATE)
            public void IncreaseElectronicsPrices()
            {
                Console.WriteLine("\n--- 3. IncreaseElectronicsPrices() was called! ---");
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
                        Console.WriteLine("!!! ERROR: No products to update (or we have already raised the price of all of them earlier) !!!");
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

            // 4. Status Update (UPDATE)
            public void UpdateOrderStatus()
            {
                Console.WriteLine("\n--- 4. UpdateOrderStatus() was called! ---");
                using (var db = new MyDbContext())
                {
                    var oldestOrderDate = db.Orders
                            .Where(x => x.Status == "New")
                            .Select(x => (DateTime?)x.Deadline)
                            .Min();

                    if (oldestOrderDate == null)
                    {
                        Console.WriteLine("!!! ERROR: There are no orders with 'New' status !!!");
                        return;
                    }

                    Console.WriteLine($"Oldest due date: {oldestOrderDate.Value}");


                    var oldestNew = db.Orders
                        .Where(x => x.Deadline.Equals(oldestOrderDate))
                        .FirstOrDefault();

                    if (oldestNew != null)
                    {
                        oldestNew.Status = "Processing";
                        oldestNew.Deadline = oldestNew.Deadline.AddDays(3);

                        db.SaveChanges();
                        Console.WriteLine($"Order ({oldestNew.ID}) updated to Processing status.");
                    }
                    else
                    {
                        Console.WriteLine("!!! ERROR: No orders found to update !!!");

                    }
                }
            }

            // 5. Deleting Discontinued Products (DELETE)
            public void DeleteDiscontinuedProducts()
            {
                Console.WriteLine("\n--- 5. DeleteDiscontinuedProducts() was called! ---");
                using (var db = new MyDbContext())
                {
                    var deleteAble = db.Products
                        .Where(x => x.Stock == 0 && !x.Orders.Any())
                        .ToList();

                    if (deleteAble.Any())
                    {
                        db.Products.RemoveRange(deleteAble);
                        db.SaveChanges();
                        Console.WriteLine($"{deleteAble.Count} discontinued products deleted.");
                    }
                    else
                    {
                        Console.WriteLine("!!! ERROR: There are no expired products to delete !!!");
                    }
                }
            }

            // 6. Deleting an Incorrect Order (DELETE)
            public void DeleteCancelledOrder(int id)
            {
                Console.WriteLine("\n--- 6. DeleteCancelledOrder() was called! ---");
                using (var db = new MyDbContext())
                {
                    var searchedItem = db.Orders
                        .Where(x => x.ID == id)
                        .FirstOrDefault();

                    if (searchedItem == null)
                    {
                        Console.WriteLine("!!! ERROR: Order not found !!!");
                        return;
                    }

                    if (searchedItem.Status.Equals("New"))
                    {
                        db.Orders.Remove(searchedItem);
                        db.SaveChanges();
                        Console.WriteLine("Order successfully deleted.");
                    }
                    else
                    {
                        Console.WriteLine($"!!! ERROR: The order cannot be deleted because its status is: {searchedItem.Status} !!!");
                        Console.ResetColor();
                    }
                }
            }
        }


        // II.Queries (READ)
        public class queriesREAD
        {
            public queriesREAD()
            {
                Console.WriteLine("\n\n====<>====  \tqueriesREAD\t  ====<>==== ");
            }

            // 7. "Bestseller" Categories (GroupBy + Aggregation)
            public void GetBestsellerCategories()
            {
                Console.WriteLine("\n--- 7. GetBestsellerCategories() was called! ---");
                using (var db = new MyDbContext())
                {
                    var stats = db.Categories
                        .Select(c => new
                        {
                            CategoryName = c.Name,
                            TotalRevenue = c.Products.SelectMany(p => p.Orders).Sum(o => (decimal?)o.Product.Price) ?? 0,
                            TotalSales = c.Products.SelectMany(p => p.Orders).Count()
                        })
                        .OrderByDescending(x => x.TotalRevenue)
                        .ToList();

                    if (stats.Any())
                    {
                        foreach (var s in stats)
                        {
                            Console.WriteLine($"{s.CategoryName}: {s.TotalRevenue:N0} HUF revenue ({s.TotalSales} items sold)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("!!! ERROR: No sales data found for categories !!!");
                    }
                }
            }

            // 8. "Forgotten" Orders (Filter + Date)
            public void GetLateOrders()
            {
                Console.WriteLine("\n--- 8. GetLateOrders() was called! ---");
                using (var db = new MyDbContext())
                {
                    var lateOrders = db.Orders
                        .Include(o => o.Product)
                        .Where(o => o.Status != "Completed" && o.Deadline < DateTime.Now)
                        .ToList();

                    if (lateOrders.Any())
                    {
                        foreach (var o in lateOrders)
                        {
                            // Calculating TimeSpan
                            var delay = DateTime.Now - o.Deadline;
                            Console.WriteLine($"Order #{o.ID} ({o.Product.Name}) - Delay: {delay.Days} days. Payment: {o.PaymentMethod}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("--- ERROR: No late orders found. Good job! ---");
                    }
                }
            }

            // 9. Expensive Categories (Any + All)
            public void GetExpensiveCategories()
            {
                Console.WriteLine("\n--- 9. GetExpensiveCategories() was called! ---");
                using (var db = new MyDbContext())
                {
                    var categories = db.Categories
                        .Where(c => c.Products.Any(p => p.Price > 50000) && c.Products.All(p => p.Stock > 0))
                        .Select(c => c.Name)
                        .ToList();

                    if (categories.Any())
                    {
                        foreach (var name in categories)
                        {
                            Console.WriteLine($"Premium Category: {name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("!!! ERROR: No category meets the 'Premium' criteria (High price + Full stock) !!!");
                    }
                }
            }

            // 10. Payment Method Statistics (GroupBy + Count)
            public void GetPaymentStats()
            {
                Console.WriteLine("\n--- 10. GetPaymentStats() was called! ---");
                using (var db = new MyDbContext())
                {
                    var stats = db.Orders
                        .GroupBy(o => o.PaymentMethod)
                        .Select(g => new
                        {
                            Method = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToList();

                    if (stats.Any())
                    {
                        foreach (var s in stats)
                        {
                            Console.WriteLine($"{s.Method}: {s.Count} orders");
                        }
                    }
                    else
                    {
                        Console.WriteLine("!!! ERROR: No order statistics available !!!");
                    }
                }
            }

            // 11. The "Most Valuable" Current Order (OrderBy + Include + FirstOrDefault)
            public void GetMostValuableActiveOrder()
            {
                Console.WriteLine("\n--- 11. GetMostValuableActiveOrder() was called! ---");
                using (var db = new MyDbContext())
                {
                    var topOrder = db.Orders
                        .Include(o => o.Product)
                        .Where(o => o.Status == "Processing" || o.Status == "Shipped")
                        .OrderByDescending(o => o.Product.Price)
                        .FirstOrDefault();

                    if (topOrder != null)
                    {
                        Console.WriteLine($"TOP Order #{topOrder.ID}: {topOrder.Product.Name} - {topOrder.Product.Price:N0} HUF ({topOrder.Status})");
                    }
                    else
                    {
                        Console.WriteLine("!!! ERROR: No active orders (Processing/Shipped) found at the moment !!!");
                    }
                }
            }

            // 12. Projection to Anonymous Type (Select new { ... })
            public void GetSimpleBookList()
            {
                Console.WriteLine("\n--- 12. GetSimpleBookList() was called! ---");
                using (var db = new MyDbContext())
                {
                    var books = db.Products
                        .Where(p => p.Category.Name.Contains("Books"))
                        .Select(p => new
                        {
                            ProductName = p.Name,
                            NetPrice = p.Price / (1 + (decimal)p.Vatpercentage / 100),
                            StockStatus = p.Stock > 10 ? "Available" : "Low Stock"
                        })
                        .ToList();

                    if (books.Any())
                    {
                        foreach (var b in books)
                        {
                            Console.WriteLine($"Book: {b.ProductName} | Net Price: {b.NetPrice:N0} HUF | Stock: {b.StockStatus}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("!!! ERROR: No products found in category 'Books' !!!");
                    }
                }
            }
        }
    }
}