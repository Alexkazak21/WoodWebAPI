using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WoodWebAPI.Data.Entities;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Timber;
using WoodWebAPI.Services;

namespace WoodWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TimberController : ControllerBase
    {
        private readonly ITimberManage _entityService;

        public TimberController(ITimberManage entity)
        {
            _entityService = entity;
        }

        [HttpPost]
        public async Task<GetTimber> GetTimbersOfOrder(GetTimberDTO model)
        {
            var data = await _entityService.GetFullOrderTimbersArrayAsync(model);

            if (data != null)
            {
                return data;
            }

            return null;
        }

        [HttpPost]
        public async Task<ExecResultModel> AddTimberToOrderAsync(AddTimberDTO model)
        {
            var data = await _entityService.AddTimberToOrderAsync(model);

            if (data != null)
            {
                return data;
            }

            return null;
        }

        [HttpPost]
        public async Task<ExecResultModel> GetTotalVolumeOfOrderAsync(GetTimberDTO model)
        {
            var data = await _entityService.GetTotalVolumeOfOrderAsync(model);

            if (data != null) 
            {
                return data;
            }

            return null;
        }

        [HttpPost]
        public async Task<IActionResult> GetOrderTimberIndexAsync(GetTimberByOrderDTO model)
        {
            var data = await _entityService.GetTimberArrayAsync(model);

            if (data == null)
            {
                return BadRequest(data);
            }

            return Ok(data);
        }
    }
}
