
using BlocoNaRua.Restful.Models.CarnivalBlock;

namespace BlocoNaRua.Services.Interfaces;

public interface ICarnivalBlockService
{
    IEnumerable<CarnivalBlockDTO> GetAll();
    CarnivalBlockDTO GetById(int id);
    void Create(CarnivalBlockCreate model);
    void Update(int id, CarnivalBlockUpdate model);
    void Delete(int id);
}
