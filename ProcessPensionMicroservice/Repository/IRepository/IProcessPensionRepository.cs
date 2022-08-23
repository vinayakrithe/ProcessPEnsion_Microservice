using ProcessPensionMicroservice.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessPensionMicroservice.Repository.IRepository
{
    public interface IProcessPensionRepository
    {
        Task<PensionerDetail> GetPensionerDetailAsync(string aadharNumber,string token);
        PensionerDetail CalculatePensionDetail(PensionerDetail pensionerDetail);
        Task SaveDataAsync(string token);
        Task Update(PensionerDetail item);
        PensionerDetail GetPensionerDetailFromDB(string aadharNumber);
    }
}
