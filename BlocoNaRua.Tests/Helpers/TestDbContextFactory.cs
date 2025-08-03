using BlocoNaRua.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext GetContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }
}
