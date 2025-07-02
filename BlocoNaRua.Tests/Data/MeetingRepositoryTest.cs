using System.Xml.XPath;
using BlocoNaRua.Data.Context;
using BlocoNaRua.Data.Repositories;
using BlocoNaRua.Data.Repositories.Interfaces;
using BlocoNaRua.Domain.Entities;

namespace BlocoNaRua.Tests.Data;

public class MeetingRepositoryTest
{
    private readonly AppDbContext _contextMock;
    private readonly IMeetingsRepository _meetingsRepository;
    public MeetingRepositoryTest()
    {
        _contextMock = AppDbContextMock.GetContext();
        _meetingsRepository = new MeetingsRepository(_contextMock);
    }

    private async Task AddData()
    {
        await _meetingsRepository.AddAsync
        (new
            (
                id: 1,
                name: "Meeting1",
                description: "meeting 1",
                location: "Location1",
                meetingCode: "CODE123",
                meetingDateTime: new DateTime
                (
                    2021, 1, 1,
                    10, 0, 0
                ),
                carnivalBlockId: 1
            )
        );
        await _meetingsRepository.AddAsync
        (new
            (
                id: 2,
                name: "Meeting2",
                description: "meeting 2",
                location: "Location2",
                meetingCode: "CODE456",
                meetingDateTime: new DateTime
                (
                    2022, 2, 2,
                    20, 0, 0
                ),
                carnivalBlockId: 1
            )
        );
    }

    [Fact]
    public async Task DeleteAsyncExists()
    {
        await AddData();
        var entityToDelete = await _meetingsRepository.GetByIdAsync(1);
        await _meetingsRepository.DeleteAsync(entityToDelete);

        var result = await _meetingsRepository.GetAllAsync();
        Assert.Single(result);
    }
}
