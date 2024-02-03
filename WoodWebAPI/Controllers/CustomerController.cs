using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WoodWebAPI.Auth;
using WoodWebAPI.Data.Models;
using WoodWebAPI.Data.Models.Customer;
using WoodWebAPI.Services;

namespace WoodWebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerManage _entityService;

        public CustomerController(ICustomerManage entity)
        {
            _entityService = entity;
        }

        [HttpPost]
        public async Task<IActionResult> GetCustomerByAdminAsync()
        {
            var result = await _entityService.GetCustomerByAdminAsync();

            if (result == null)
            {
                return BadRequest("Customers list is empty"); 
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetCustomers()
        {
            var result = await _entityService.GetAsync();

            if (result == null)
            {
                return BadRequest("Customers list is empty"); 
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomers(CreateCustomerDTO model)
        {
            var result = await _entityService.CreateAsync(model);

            if (!result.Success)
            {
                return BadRequest(result.Message); 
            }

            return Ok(result);
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost]
        public async Task<IActionResult> DeleteCustomers(DeleteCustomerDTO model)
        {
            var result = await _entityService.DeleteAsync(model);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(UpdateCustomerDTO model)
        {
            var result = await _entityService.UpdateAsync(model);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(GetAdminDTO model)
        {
            var result = await _entityService.AddAdminAsync(model);

            if ( !result.Success )
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
        [HttpGet]
        public async Task<IActionResult> GetAdminList()
        {
            var result = await _entityService.GetAdminListAsync();

            if(result == null)
            {
                return BadRequest("Admin List is empty");
            }

            return Ok(result);
        }
    }
}
