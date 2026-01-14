using ALS.Core.Constants;
using ALS.Core.Contracts;
using ALS.Core.Enum;
using ALS.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ALS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuditController : ControllerBase
    {
        private readonly ILogger<AuditController> _logger;
        private readonly IAuditQueryService _auditQueryService;
        private readonly IAuditIngestionService _auditIngestionService;

        public AuditController(
            ILogger<AuditController> logger,
            IAuditQueryService auditQueryService,
            IAuditIngestionService auditIngestionService)
        {
            _logger = logger;
            _auditQueryService = auditQueryService;
            _auditIngestionService = auditIngestionService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? userId,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] string? actionType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string sort = Constants.SortOptions.TimestampDesc,
            CancellationToken ct = default)
        {
            try
            {
                var req = new AuditSearchRequest(userId, from, to, actionType, sort, page, pageSize);
                var response = await _auditQueryService.SearchAsync(req, ct);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(Constants.ErrorMessages.GetErrorWithMessage, ex.Message);
                return Problem(detail: Constants.ErrorMessages.GetError, statusCode: 500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AuditEventDto dto, CancellationToken ct)
        {
            try
            {
                var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString() ?? HttpContext.TraceIdentifier;
                var eventId = await _auditIngestionService.IngestAsync(dto, AuditIngestSource.Http, traceId, ct);

                return Accepted(eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(Constants.ErrorMessages.PostErrorWithMessage, ex.Message);
                return Problem(detail: Constants.ErrorMessages.PostError, statusCode: 500);
            }
        }
    }
}
