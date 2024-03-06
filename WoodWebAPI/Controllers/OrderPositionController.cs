using Microsoft.AspNetCore.Mvc;
using WoodWebAPI.Data.Models.OrderPosition;
using WoodWebAPI.Services;

namespace WoodWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderPositionController : ControllerBase
    {
        private readonly IOrderPositionManage _entityService;

        public OrderPositionController(IOrderPositionManage entity)
        {
            _entityService = entity;
        }

        [HttpPost]
        public async Task<IActionResult> GetOrderPositionsOfOrder(GetOrderPositionsByOrderIdDTO model)
        {
            var data = await _entityService.GetOrderPositionsOfOrderAsync(model);

            if (data != null)
            {
                return Ok(data);
            }

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrderPositionToOrderAsync(AddOrderPositionDTO model)
        {
            var data = await _entityService.AddOrderPositionToOrderAsync(model);

            if (data != null)
            {
                return Ok(data);
            }

            return Ok(data);
        }

        [HttpPost]
        public async Task<double> GetTotalVolumeOfOrderAsync(GetOrderPositionsByOrderIdDTO model)
        {
            var data = await _entityService.GetTotalVolumeOfOrderAsync(model);

            return data;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderPositionAsync(UpdateOrderPositionDTO model)
        {
            var data = await _entityService.UpdateOrderPositionAsync(model);

            if (data != null)
            {
                return Ok(data);
            }

            return Ok(data);
        }
    }
}
