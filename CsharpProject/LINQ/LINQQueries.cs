using Microsoft.EntityFrameworkCore;
using CsharpProject.Entities;

namespace CsharpProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting program...\n");

            LINQQUeries queries = new LINQQUeries();

            queries.queryingAllRecords();

            Console.Write("\nThe program has finished running. Press any key to exit.");
            Console.ReadKey();
        }
    }

    public class LINQQUeries {
        // Simple queries to retrieve all records from the tables
        public void queryingAllRecords()
        {
            using (var db = new MyDbContext())
            {
                var allCategories = db.Categories.ToList();
                var allOrders = db.Orders.ToList();
                var allProducts = db.Products.ToList();

                Console.WriteLine("***** All Categories *****");
                foreach (var c in allCategories)
                    Console.WriteLine($"Name={c.Name}");

                Console.WriteLine("\n***** All Orders *****");
                foreach (var o in allOrders)
                    Console.WriteLine($"OrderId={o.ID}  Customer={o.Product.Name}");

                Console.WriteLine("\n***** All Products *****");
                foreach (var p in allProducts)
                    Console.WriteLine($"Name={p.ID}  Price={p.Price}");
            } 
        }
    }
}