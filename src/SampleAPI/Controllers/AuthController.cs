using Microsoft.AspNetCore.Mvc;
using SampleAPI.Features.GetVersao2;

namespace SampleAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("versao-app")]
    public async Task<IActionResult> VersaoApp([FromQuery] string versaoAtual = "1.0.0")
    {
        try
        {
            var response = await _mediator.Send(new GetVersao.Query(versaoAtual));
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(e);
        }
    }
}