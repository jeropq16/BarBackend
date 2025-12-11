using Barber.Application.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Barber.Api.Controllers;


[ApiController]
[Route("haircuts")]
public class HairCutsController : ControllerBase
{
    private readonly HairCutService _service;

    public HairCutsController(HairCutService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var h = await _service.GetAllAsync();
        return Ok(h);
    }
    
    
    
}