using Ayura.API.Features.EmailVerification.DTOs;
using Ayura.API.Features.EmailVerification.Services;
using Ayura.API.Features.Profile.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.EmailVerification.Controllers;

[ApiController]
[Route("api/evc")]
public class EmailVerificationController : Controller
{
    private readonly IEmailVerificationService _emailVerificationService;

    public EmailVerificationController(IEmailVerificationService emailVerificationService)
    {
        _emailVerificationService = emailVerificationService;
    }

    [HttpPost("generate")]
    public IActionResult GenerateEmailVerification([FromBody] EVCRequestDTO evcodeRequest)
    {
        var userId = ResolveJWT.ResolveIdFromJWT(Request);
        // if user is not logged in, return 401
        if (userId == null) return Unauthorized();
        Console.Write($"ID is {userId}\n");
        var result = _emailVerificationService.GenerateEmailVerificationCode(evcodeRequest, userId);
        return Ok(result);
    }


    [HttpPost("verify")]
    public IActionResult VerifyEmail([FromBody] EVCVerifyDTO evcodeVerify)
    {
        var userId = ResolveJWT.ResolveIdFromJWT(Request);
        var result = _emailVerificationService.VerifyEmail(evcodeVerify, userId);
        return Ok(result);
    }
}