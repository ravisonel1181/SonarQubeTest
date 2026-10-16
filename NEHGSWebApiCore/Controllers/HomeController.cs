using Microsoft.AspNetCore.Mvc;
using NEHGSWebApiCore.Application.Common.Responses;
using NEHGSWebApiCore.Application.DTOs;
using NEHGSWebApiCore.Application.Interfaces.Services;
using System.Globalization;
using System.Net;

namespace NEHGSWebApiCore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IFileStorageService _fileStorage;

        public HomeController(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        //[HttpGet]
        //public IActionResult Index()
        //{
        //    return Ok(ApiResponse<string>.Ok("NEHGS Collections API"));
        //}

        //[HttpGet("about")]
        //public IActionResult About()
        //{
        //    return Ok(ApiResponse<string>.Ok("Your application description page."));
        //}

        //[HttpGet("contact")]
        //public IActionResult Contact()
        //{
        //    return Ok(ApiResponse<string>.Ok("Your contact page."));
        //}

        [HttpPost("upload")]
        [RequestSizeLimit(524288000)] // 500MB
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.Fail("Please select a file to upload.", HttpStatusCode.BadRequest));          

            var uploadResponse = await _fileStorage.SaveCsvAsync(file);
           
            return Ok(uploadResponse);
        }       
    }
}
