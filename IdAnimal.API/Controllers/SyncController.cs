using System.Security.Claims;
using Dotmim.Sync;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.Web.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // 👈 Important: Add this for .Any()

namespace IdAnimal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly WebServerAgent _webServerAgent;

    public SyncController(WebServerAgent webServerAgent)
    {
        _webServerAgent = webServerAgent;
    }

    [HttpPost]
    public async Task HandleAsync()
    {
        // 1. Get User ID from JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out int authenticatedUserId))
        {
            Response.StatusCode = 401;
            return;
        }

        // --- FIX FOR READ (Fetching Data) ---
        _webServerAgent.RemoteOrchestrator.OnTableChangesSelecting(args =>
        {
            var syncParams = args.Context.Parameters;

            // Remove existing parameter if present to avoid duplication/errors
            if (syncParams.Contains("UserId"))
            {
                syncParams.Remove("UserId");
            }

            // Add the secure parameter
            syncParams.Add("UserId", authenticatedUserId);
        });

        // --- FIX FOR WRITE (Uploading Data) ---
        _webServerAgent.RemoteOrchestrator.OnRowsChangesApplying(args =>
        {
            // FIX: Use LINQ to check for the column name string
            var hasUserId = args.SchemaTable.Columns.Any(c => c.ColumnName.Equals("UserId", StringComparison.OrdinalIgnoreCase));

            if (hasUserId)
            {
                foreach (var row in args.SyncRows)
                {
                    // FORCE the UserId to match the JWT
                    row["UserId"] = authenticatedUserId;
                }
            }
        });

        // 4. Run the sync
        await _webServerAgent.HandleRequestAsync(HttpContext);
    }
}