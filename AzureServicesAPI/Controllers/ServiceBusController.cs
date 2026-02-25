using AzureServicesAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Shared.Models.AzureServices;

namespace AzureServicesAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceBusController : ControllerBase
    {
        private readonly IServiceBusManager _serviceBusManager;

        public ServiceBusController(IServiceBusManager serviceBusManager)
        {
            _serviceBusManager = serviceBusManager;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessageAsync([FromBody] ServiceBus request)
        {
            if (string.IsNullOrWhiteSpace(request.Body))
                return BadRequest("Message body cannot be empty.");

            var result = await _serviceBusManager.SendMessageAsync(request);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("peek")]
        public async Task<IActionResult> PeekMessagesAsync([FromQuery] int count = 1)
        {
            if (count < 1 || count > 100)
                return BadRequest("Count must be between 1 and 100.");

            var result = await _serviceBusManager.PeekMessagesAsync(count);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("receive")]
        public async Task<IActionResult> ReceiveAndCompleteAsync()
        {
            var result = await _serviceBusManager.ReceiveAndCompleteAsync();
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("deadletter")]
        public async Task<IActionResult> PeekDeadLettersAsync()
        {
            var result = await _serviceBusManager.PeekDeadLettersAsync();
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("deadletter/resend/{sequenceNumber}")]
        public async Task<IActionResult> ResendDeadLetterSingleAsync(long sequenceNumber)
        {
            var result = await _serviceBusManager.ResendDeadLetterAsync(sequenceNumber);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}