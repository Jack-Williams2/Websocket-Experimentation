using Microsoft.AspNetCore.Mvc;
using WebsocketApi.Websockets;

namespace WebsocketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleController : Controller
    {
        [HttpPost("notify")]
        public async Task<IActionResult> NotifyAll([FromBody] string message)
        {
            await NotifactionWebsocketHandler.BroadcastAsync(message);
            return Ok();
        }
    }
}
