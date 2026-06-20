using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// 上传商品图片（仅支持 JPG）
    /// </summary>
    [HttpPost("image")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 最大 10MB
    public async Task<ActionResult<UploadResult>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "请选择要上传的图片" });

        // 只允许 JPG
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".jpg" && ext != ".jpeg")
            return BadRequest(new { error = "仅支持 JPG 格式的图片" });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "图片大小不能超过 10MB" });

        // 保存到 wwwroot/uploads/ 目录
        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        // 生成唯一文件名
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 返回可访问的 URL
        var url = $"/uploads/{fileName}";

        return Ok(new UploadResult
        {
            Url = url,
            FileName = fileName
        });
    }
}

public class UploadResult
{
    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
