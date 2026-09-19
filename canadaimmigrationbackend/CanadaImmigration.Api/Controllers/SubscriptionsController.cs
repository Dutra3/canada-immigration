using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CanadaImmigration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriberService _subscriberService;

    public SubscriptionsController(ISubscriberService subscriberService)
    {
        _subscriberService = subscriberService;
    }

    [HttpPost]
    [EnableRateLimiting("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _subscriberService.SubscribeAsync(request.Email, request.Categories, cancellationToken);

        return result switch
        {
            SubscribeResult.InvalidEmail => BadRequest("Informe um e-mail válido."),
            SubscribeResult.NoCategories => BadRequest("Selecione ao menos uma categoria (ou \"Todas/Qualquer\")."),
            SubscribeResult.Created => Ok(new { message = "Inscrição criada com sucesso." }),
            SubscribeResult.Updated => Ok(new { message = "Suas preferências de categoria foram atualizadas." }),
            _ => StatusCode(500)
        };
    }

    [HttpGet("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromQuery] string token, CancellationToken cancellationToken)
    {
        var removed = await _subscriberService.UnsubscribeAsync(token, cancellationToken);

        return removed ? Ok(new { message = "Você foi descadastrado com sucesso." }) : NotFound();
    }
}
