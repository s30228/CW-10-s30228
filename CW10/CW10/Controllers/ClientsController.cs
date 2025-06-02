using CW10.Exceptions;
using CW10.Services;
using Microsoft.AspNetCore.Mvc;

namespace CW10.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientsController(IDbService service) : ControllerBase
{
    [HttpDelete("{idClient}")] // http://localhost:port/clients/{idClient}
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        try
        {
            await service.DeleteClientAsync(idClient);
            return NoContent();
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