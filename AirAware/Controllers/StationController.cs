using AirAware.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirAware.Controllers;

[ApiController]
[Route("v1")]
public class StationController: ControllerBase
{
    [HttpGet]
    [Route("stations")]
    public List<Station> Get()
    {
        return new List<Station>();
    }
}