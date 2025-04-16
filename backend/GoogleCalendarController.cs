using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(DateTime dateTime)
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        
        if (string.IsNullOrEmpty(accessToken))
            return Unauthorized("Google access token not found");

        var service = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken),
            ApplicationName = "aioniq"
        });

        var request = service.Events.List("primary");
        request.TimeMin = DateTime.Now;
        request.MaxResults = 10;
        request.SingleEvents = true;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        var events = await request.ExecuteAsync();
        
        return Ok(events.Items);
    }
}

