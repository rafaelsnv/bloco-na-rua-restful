using BlocoNaRua.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BlocoNaRua.Tests;

public class RepositoryBaseTest
{
    protected static Mock<DbSet<TestEntity>> CreateMockDbSet<TestEntity>(List<TestEntity> data) where TestEntity : EntityBase
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<TestEntity>>();

        mockSet.As<IQueryable<TestEntity>>()
               .Setup(m => m.Provider)
               .Returns(queryable.Provider);

        mockSet.As<IQueryable<TestEntity>>()
               .Setup(m => m.Expression)
               .Returns(queryable.Expression);

        mockSet.As<IQueryable<TestEntity>>()
               .Setup(m => m.ElementType)
               .Returns(queryable.ElementType);

        mockSet.As<IQueryable<TestEntity>>()
               .Setup(m => m.GetEnumerator())
               .Returns(queryable.GetEnumerator());

        mockSet.Setup(d => d.AddAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity entity, CancellationToken _) =>
            {
                data.Add(entity);
                return new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TestEntity>(null);
            });

        mockSet.Setup(d => d.Remove(It.IsAny<TestEntity>()))
               .Callback<TestEntity>(entity => data.Remove(entity));

        mockSet.Setup(d => d.Update(It.IsAny<TestEntity>()))
            .Callback<TestEntity>(entity =>
            {
                var idx = data.FindIndex(e => e.Id == entity.Id);
                if (idx >= 0) data[idx] = entity;
            });

        mockSet.Setup(d => d.FindAsync(It.IsAny<object[]>()))
               .ReturnsAsync((object[] ids) =>
               {
                   return data.FirstOrDefault(e => e.Id == (int)ids[0]);
               });

        return mockSet;
    }

}
