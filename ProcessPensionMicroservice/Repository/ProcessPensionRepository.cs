
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ProcessPensionMicroservice.Data;
using ProcessPensionMicroservice.Models;
using ProcessPensionMicroservice.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProcessPensionMicroservice.Repository
{
    public class ProcessPensionRepository : IProcessPensionRepository
    {
        private readonly ApplicationDbContext _db;
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(ProcessPensionRepository));

        public ProcessPensionRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        List<string> publicBanks = new List<string>(){
            "SBI","PNB","MB","AB"
        };
        List<string> privateBanks = new List<string>(){
            "HDFC","HFBC","ICICI"
        };
        public async Task<PensionerDetail> GetPensionerDetailAsync(string aadharNumber,string token)
        {
            PensionerDetail pensionerDetail = new PensionerDetail();
            using (var httpClient = new HttpClient())
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44356/api/PensionerDetails/GetPensionerDetail/" + aadharNumber))
                {
                    requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.SendAsync(requestMessage);
                    _log4net.Info("Get request send to Prnsioner Detail microservice from GetPensionerDetailAsync method of: " + nameof(ProcessPensionRepository));
                   
                    int sc = (int)response.StatusCode;
                    if(sc == 200)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        pensionerDetail = JsonConvert.DeserializeObject<PensionerDetail>(apiResponse);
                    }
                    else
                    {
                        _log4net.Error("Error while sending Get request to Prnsioner Detail microservice from GetPensionerDetailAsync method of: " + nameof(ProcessPensionRepository));
                        return null;
                    }
                }
            }
            _log4net.Info("Prnsioner Details returned from GetPensionerDetailAsync method of: " + nameof(ProcessPensionRepository));
            return pensionerDetail;
        }

        public async Task SaveDataAsync(string token)
        {
            List<PensionerDetail> pensionerDetails = new List<PensionerDetail>();
            using (var httpClient = new HttpClient())
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44356/api/PensionerDetails/GetDetails"))
                {
                    requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                    var response = await httpClient.SendAsync(requestMessage);

                    int sc = (int)response.StatusCode;
                    if (sc == 200)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        pensionerDetails = JsonConvert.DeserializeObject<List<PensionerDetail>>(apiResponse);
                    }
                }
            }

            foreach (var entry in pensionerDetails)
            {
                var penDetail = _db.PensionerDetails.FirstOrDefault(item => item.AadharNumber == entry.AadharNumber);
                if (penDetail == null)
                {
                    _db.PensionerDetails.Add(entry);
                }
            }
            _log4net.Info("data saved in database from SaveDataAsync method of: " + nameof(ProcessPensionRepository));
            _db.SaveChanges();
        }

        public async Task Update(PensionerDetail pensionerDetail)
        {
            var penDetail = _db.PensionerDetails.FirstOrDefault(item => item.AadharNumber == pensionerDetail.AadharNumber);
            penDetail.PensionAmount = pensionerDetail.PensionAmount;
            penDetail.BankServiceCharge = pensionerDetail.BankServiceCharge;
            penDetail.AadharNumber = pensionerDetail.AadharNumber;
            penDetail.Name = pensionerDetail.Name;
            penDetail.DateOfBirth = pensionerDetail.DateOfBirth;
            penDetail.PAN = pensionerDetail.PAN;
            penDetail.SalaryEarned = pensionerDetail.SalaryEarned;
            penDetail.Allowances = pensionerDetail.Allowances;
            penDetail.PensionType = pensionerDetail.PensionType;
            penDetail.BankName = pensionerDetail.BankName;
            penDetail.AccountNumber = pensionerDetail.AccountNumber;
            penDetail.BankType = pensionerDetail.BankType;
            _log4net.Info("data updated in database from Upade method of: " + nameof(ProcessPensionRepository));
            await _db.SaveChangesAsync();
        }

        public PensionerDetail GetPensionerDetailFromDB(string aadharNumber)
        {
            return   _db.PensionerDetails.FirstOrDefault(item => item.AadharNumber == aadharNumber);
        }

        public PensionerDetail CalculatePensionDetail(PensionerDetail pensionerDetail)
        {
            double amount = 0;
            int bankServiceCharge = 0;

            if (string.Equals(pensionerDetail.PensionType, "self", StringComparison.OrdinalIgnoreCase))
            {
                amount = ((pensionerDetail.SalaryEarned)*0.8) + pensionerDetail.Allowances;
            }
            else if (string.Equals(pensionerDetail.PensionType, "family", StringComparison.OrdinalIgnoreCase))
            {
                amount = ((pensionerDetail.SalaryEarned) * 0.5) + pensionerDetail.Allowances;
            }
           
            if(publicBanks.Contains(pensionerDetail.BankName.ToUpper()))
            {
                bankServiceCharge = 500;
            }
            else if(privateBanks.Contains(pensionerDetail.BankName.ToUpper()))
            {
                bankServiceCharge = 550;
            }
            pensionerDetail.PensionAmount = amount;
            pensionerDetail.BankServiceCharge = bankServiceCharge;
            return pensionerDetail;
        }
    }
}
