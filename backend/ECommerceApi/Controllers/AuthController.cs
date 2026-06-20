using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // 硬编码账号密码：12345 / 12345
    private const string ValidUsername = "12345";
    private const string ValidPassword = "12345";

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "请输入账号和密码" });
        }

        if (request.Username != ValidUsername || request.Password != ValidPassword)
        {
            return Unauthorized(new { error = "账号或密码错误" });
        }

        return Ok(new LoginResponse
        {
            Token = "admin-token-2024",
            Username = request.Username,
            Message = "登录成功"
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
