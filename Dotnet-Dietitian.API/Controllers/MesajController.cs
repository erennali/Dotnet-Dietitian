using Dotnet_Dietitian.API.Hubs;
using Dotnet_Dietitian.Application.Features.CQRS.Commands.MesajCommands;
using Dotnet_Dietitian.Application.Features.CQRS.Queries.MesajQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Dotnet_Dietitian.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MesajController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<MesajlasmaChatHub> _hubContext;

        public MesajController(IMediator mediator, IHubContext<MesajlasmaChatHub> hubContext)
        {
            _mediator = mediator;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateMesajCommand command)
        {
            try
            {
                var mesajId = await _mediator.Send(command);
                
                // SignalR ile anında alıcıya bildirim gönder
                // Bu kısım MassTransit consumer'ı tarafından da yapılabilir
                await _hubContext.Clients.User(command.AliciId.ToString())
                    .SendAsync("ReceiveMessage", new
                    {
                        Id = mesajId,
                        GonderenId = command.GonderenId,
                        GonderenTipi = command.GonderenTipi,
                        Icerik = command.Icerik,
                        GonderimZamani = DateTime.Now
                    });
                
                return Ok(new { mesajId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { hata = ex.Message });
            }
        }

        [HttpGet("conversation")]
        public async Task<IActionResult> GetConversation(
            [FromQuery] Guid user1Id, 
            [FromQuery] string user1Type,
            [FromQuery] Guid user2Id, 
            [FromQuery] string user2Type,
            [FromQuery] int count = 50)
        {
            var query = new GetConversationQuery(user1Id, user1Type, user2Id, user2Type, count);
            var messages = await _mediator.Send(query);
            return Ok(messages);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadMessages([FromQuery] Guid userId, [FromQuery] string userType)
        {
            var query = new GetUnreadMessagesQuery(userId, userType);
            var messages = await _mediator.Send(query);
            return Ok(messages);
        }

        [HttpPost("{mesajId}/read")]
        public async Task<IActionResult> MarkAsRead(
            Guid mesajId, 
            [FromQuery] Guid okuyanId,
            [FromQuery] string okuyanTipi)
        {
            var command = new MarkAsReadCommand(mesajId, okuyanId, okuyanTipi);
            await _mediator.Send(command);
            return Ok();
        }
    }
}