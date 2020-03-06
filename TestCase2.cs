using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyPoC
{
    /// <summary>
    /// This test case demonstrate how the DatabaseValue
    /// Does not get reset to the InitialValue when Under a Transaction (in Postgres)
    /// </summary>
    public class TestCase2 : ITestCase
    {
        private readonly BaseContext _dataContext;
        private readonly string _dbType;

        public TestCase2(string dbType) {
            _dbType = dbType;
            _dataContext = ContextFactory.CreateContext(dbType);
        }

        public void Run()
        {
            using var transaction = _dataContext.Database.BeginTransaction(IsolationLevel.ReadUncommitted);

            var blog1 = _dataContext.Blogs.Single(p => p.BlogId == 1);
            var blog2 = _dataContext.Blogs.Single(p => p.BlogId == 2);
            blog1.Url = "blog1_updated";
            blog2.Url = "blog2_updated";

            // Simulate an external update to force throwing a DbUpdateConcurrencyException
            ExternalUpdate();

            try
            {
                // Attempt to save changes to the database
                _dataContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                var entry1 = _dataContext.Entry(blog1);
                var entry2 = _dataContext.Entry(blog2);
                var original1 = entry1.OriginalValues.GetValue<string>("Url");
                var current1 = entry1.CurrentValues.GetValue<string>("Url");
                var database1 = entry1.GetDatabaseValues().GetValue<string>("Url");
                var original2 = entry2.OriginalValues.GetValue<string>("Url");
                var current2 = entry2.CurrentValues.GetValue<string>("Url");
                var database2 = entry2.GetDatabaseValues().GetValue<string>("Url");

                Console.WriteLine($"Original1 {original1}");
                Console.WriteLine($"Current1 {current1}");
                Console.WriteLine($"Database1 {database1}");
                Console.WriteLine($"Original2 {original2}");
                Console.WriteLine($"Current2 {current2}");
                Console.WriteLine($"Database2 {database2}");

                Debug.Assert(original1 == "blog1_initial");
                Debug.Assert(current1 == "blog1_updated");
                Debug.Assert(database1 == "blog1_updatedElsewhere");
                Debug.Assert(original2 == "blog2_initial");
                Debug.Assert(current2 == "blog2_updated");
                Debug.Assert(database2 == "blog2_initial");
            }
            transaction.Commit();
        }

        public void Assert()
        {
        }

        public void ExternalUpdate() {
            // Change the entry in the database to simulate a concurrency conflict
            using var context = ContextFactory.CreateContext(_dbType);
            context.Database.ExecuteSqlRaw("UPDATE blogs SET url = 'blog1_updatedElsewhere' WHERE blog_id = 1");
        }
    }
}