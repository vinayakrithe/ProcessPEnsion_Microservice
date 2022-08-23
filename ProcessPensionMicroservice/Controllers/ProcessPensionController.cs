using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using ProcessPensionMicroservice.Repository.IRepository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessPensionMicroservice.Controllers
{
    [Route("api/ProcessPension")]
    [ApiController]
    [Authorize]
    public class ProcessPensionController : ControllerBase
    {
        private readonly IProcessPensionRepository _callAPIREpository;
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(ProcessPensionController));

        public ProcessPensionController(IProcessPensionRepository callAPIREpository)
        {
            this._callAPIREpository = callAPIREpository;
        }

        //[HttpGet("[action]/{aadharNumber}")]
        //public async Task<IActionResult> GetPensionerDetails(string aadharNumber)
        //{
        //    var token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
        //    var result = await _callAPIREpository.GetPensionerDetailAsync(aadharNumber, token);

        //    if (result!=null)
        //    {
        //        return Ok(result);
        //    }
        //    return BadRequest();
        //}

        [HttpPost("[action]/{aadharNumber}")]
        public async Task<IActionResult> PensionDetail(string aadharNumber)
        {
            _log4net.Info(" Http Post Request From PensionDetail method of: " + nameof(ProcessPensionController));
            var token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            var result = await _callAPIREpository.GetPensionerDetailAsync(aadharNumber, token);
            _log4net.Info("Pensioner details fetched from PensionerDetails microservice in PensionDetail method of: " + nameof(ProcessPensionController));
            if (result==null)
            {
                _log4net.Warn(" Bad Request returned from PensionDetail method of: " + nameof(ProcessPensionController));
                return BadRequest(new { message = "Aaadhar number is invalid" }); 
            }
            var pensionerDetail = result;

            var res = _callAPIREpository.CalculatePensionDetail(pensionerDetail);

            await _callAPIREpository.SaveDataAsync(token);
            await _callAPIREpository.Update(res);
            _log4net.Info("Pensioner details returned From PensionDetail method of: " + nameof(ProcessPensionController));
            return Ok(res);
        }
    }
}
