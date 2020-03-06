using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ConcurrencyPoC
{
    public interface ITestCase
    {
        void Run();
        void Assert();
    }
}