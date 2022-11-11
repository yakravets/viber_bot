using Microsoft.AspNetCore.Mvc;
using Viber.Bot.NetCore.Infrastructure;
using Viber.Bot.NetCore.Models;
using Viber.Bot.NetCore.RestApi;

namespace ViberSender.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ViberController : ControllerBase
    {
        private readonly IViberBotApi _viberBotApi;
        private readonly IConfiguration _configuration;

        public ViberController(IViberBotApi viberBotApi, IConfiguration configuration)
        {
            _viberBotApi = viberBotApi;
            _configuration = configuration;
        }

        // The service sets a webhook automatically, but if you want sets him manually then use this
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var webhookUrl = _configuration.GetSection("ViberBot")["Webhook"];
            var response = await _viberBotApi.SetWebHookAsync(new ViberWebHook.WebHookRequest(webhookUrl));

            if (response.Content?.Status == ViberErrorCode.Ok)
            {
                return Ok("Viber-bot is active");
            }
            else
            {
                return BadRequest(response.Content?.StatusMessage);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ViberCallbackData update)
        {
            var str = String.Empty;

            switch (update.Message.Type)
            {
                case ViberMessageType.Text:
                    {
                        var mess = update.Message as ViberMessage.TextMessage;

                        str = mess.Text;

                        break;
                    }

                default: break;
            }

            // you should to control required fields
            var message = new ViberMessage.TextMessage()
            {
                //required
                Receiver = update.Sender.Id,
                Sender = new ViberUser.User()
                {
                    //required
                    Name = "Our bot",
                    Avatar = "http://dl-media.viber.com/1/share/2/long/bots/generic-avatar%402x.png"
                },
                //required
                Text = str
            };

            // our bot returns incoming text
            var response = await _viberBotApi.SendMessageAsync<ViberResponse.SendMessageResponse>(message);

            return Ok();
        }
    }
}
