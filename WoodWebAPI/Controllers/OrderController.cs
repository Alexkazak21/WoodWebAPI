using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WoodWebAPI.Data.Models.Order;
using WoodWebAPI.Services;

namespace WoodWebAPI.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IOrderManage _entityService;

        public OrderController(IOrderManage entity)
        {
            _entityService = entity;
        }

        [HttpPost]
        public async Task<OrderModel[]?> GetOrdersOfCustomer(GetOrdersDTO model)
        {
            var data = await _entityService.GetOrdersOfCustomerAsync(model);

            if (data != null)
            {
                return data;
            }

            return data;
        }

        [HttpPost]
        public async Task<OrderModel[]?> GetFullOrdersList()
        {

            var data = await _entityService.GetFullOrdersArrayAsync();

            if (data != null)
            {
                return data;
            }

            return data;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(GetOrdersDTO model)
        {
            var data = await _entityService.CreateAsync(model);

            if (data.Success)
            {
                return Ok(data);
            }

            return BadRequest(data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(ArchiveOrderDTO model)
        {
            var data = await _entityService.ArchiveAsync(model);

            if (data.Success)
            {
                return Ok(data);
            }

            return BadRequest(data);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteOrderByAdmin(ArchiveOrderDTO model)
        {
            var data = await _entityService.ArchiveByAdminAsync(model);

            if (data.Success)
            {
                return Ok(data);
            }

            return BadRequest(data);
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> VerifyOrderByAdmin(VerifyOrderDTO model)
        {
            var data = await _entityService.VerifyOrderByAdminAsync(model);

            if (data.Success)
            {
                return Ok(data);
            }

            return BadRequest(data);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrderByAdmin(VerifyOrderDTO model)
        {
            var data = await _entityService.CompleteOrderByAdminAsync(model);

            if (data.Success)
            {
                return Ok(data);
            }

            return BadRequest(data);
        }

        [HttpPost]
        public async Task<IActionResult> PaidSuccessfull(VerifyOrderDTO model)
        {
            var data = await _entityService.PaidSuccessfullyAsync(model);

            if (data.Success)
            {
                return Ok(data);
            }

            return BadRequest(data);
        }
    }
}
