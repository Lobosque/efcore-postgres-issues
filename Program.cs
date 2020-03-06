using System;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyPoC
{
    internal static class Program
    {
        private static void Main()
        {
            ITestCase testCase;
            string[] arguments = Environment.GetCommandLineArgs();
            var testCaseType = arguments[1];
            var dbType = arguments[2];

            ResetValues(dbType);
            if(testCaseType == "case1") {
                testCase = new TestCase1(dbType);
            } else if (testCaseType == "case2") {
                testCase = new TestCase2(dbType);
            } else {
                Console.WriteLine($"Unknown Test Case {testCaseType}");
                return;
            }

            testCase.Run();
            testCase.Assert();
        }

        private static void ResetValues(string dbType) {
            using var context = ContextFactory.CreateContext(dbType);
            context.Database.ExecuteSqlRaw("DELETE FROM posts WHERE 1 = 1");
            context.Database.ExecuteSqlRaw("DELETE FROM blogs WHERE 1 = 1");
            context.Database.ExecuteSqlRaw("INSERT INTO blogs (blog_id, url)  VALUES(1, 'blog1_initial')");
            context.Database.ExecuteSqlRaw("INSERT INTO blogs (blog_id, url)  VALUES(2, 'blog2_initial')");
        }
    }
}