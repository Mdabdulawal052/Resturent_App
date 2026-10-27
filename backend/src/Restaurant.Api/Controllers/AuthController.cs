using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;

namespace Restaurant.Api.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest req)
    {
        // Simple demo token generation for active cashier/admin
        return Ok(new AuthResponse(
            Token: "demo_jwt_token_" + Guid.NewGuid().ToString("N"),
            UserId: req.UserName == "admin" ? 1 : 2,
            UserName: req.UserName,
            DisplayName: req.UserName == "admin" ? "System Administrator" : "Kamrul Hasan (Cashier)",
            Role: req.UserName == "admin" ? "Admin" : "Cashier",
            RestaurantId: 1,
            BranchId: 1
        ));
    }

    [HttpPost("pin-login")]
    public ActionResult<AuthResponse> PinLogin([FromBody] PinLoginRequest req)
    {
        var role = req.Pin == "1234" ? "Admin" : (req.Pin == "1111" ? "Waiter" : "Cashier");
        return Ok(new AuthResponse(
            Token: "pin_session_token_" + Guid.NewGuid().ToString("N"),
            UserId: 2,
            UserName: role.ToLower(),
            DisplayName: $"{role} Terminal Session",
            Role: role,
            RestaurantId: 1,
            BranchId: req.BranchId ?? 1
        ));
    }
}
