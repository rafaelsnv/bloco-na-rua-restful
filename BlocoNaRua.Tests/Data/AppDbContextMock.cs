using BlocoNaRua.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Tests.Data;

public static class AppDbContextMock
{
    public static AppDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb").Options;
        return new AppDbContext(options);
    }
}
