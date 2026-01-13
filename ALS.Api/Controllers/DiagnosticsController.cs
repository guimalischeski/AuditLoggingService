using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ALS.Api.Controllers
{
    [ApiController]
    public sealed class DiagnosticsController : ControllerBase
    {
        [HttpGet("/health")]
        [Produces("application/json")]
        public async Task<IActionResult> Health([FromServices] HealthCheckService hc, CancellationToken ct)
        {
            var report = await hc.CheckHealthAsync(ct);

            return Ok(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    error = e.Value.Exception?.Message,
                    durationMs = e.Value.Duration.TotalMilliseconds
                })
            });
        }
    }
}
