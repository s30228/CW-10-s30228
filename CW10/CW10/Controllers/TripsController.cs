using CW10.DTOs;
using CW10.Exceptions;
using CW10.Services;
using Microsoft.AspNetCore.Mvc;

namespace CW10.Controllers;

[ApiController]
[Route("[controller]")]
public class TripsController(IDbService service) : ControllerBase
{
    [HttpGet] // http://localhost:port/trips?page=1&pageSize=10
    public async Task<IActionResult> GetTripsInfo([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            return Ok(await service.GetTripsInfoAsunc(page, pageSize));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientCreateDTO dto)
    {
        try
        {
            var assignment = await service.AssignClientToTripAsync(idTrip, dto);
            return Created($"/clients/{assignment.IdClient}", assignment);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}