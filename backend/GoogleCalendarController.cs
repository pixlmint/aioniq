using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public CalendarController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("testEvents")]
    [GoogleScopedAuthorize(CalendarService.ScopeConstants.CalendarReadonly)]
    public async Task<IActionResult> TestGetEvents([FromServices] IGoogleAuthProvider auth)
    {
        GoogleCredential cred = await auth.GetCredentialAsync();
        var service = new CalendarService(
            new BaseClientService.Initializer { HttpClientInitializer = cred }
        );

        var calendars = await service.CalendarList.List().ExecuteAsync();
        var calendarNames = calendars.Items.Select(x => x.Id).ToList();
        return Ok(calendarNames);
    }
}
