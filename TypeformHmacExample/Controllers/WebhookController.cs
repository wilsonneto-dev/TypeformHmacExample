using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace TypeformHmacExample.Controllers;
[ApiController]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    public static List<string> logs = new();

    [HttpGet()]
    public IActionResult Get()
    {
        return Ok(new { Success = true, logs });
    }

    [HttpPost()]
    public async Task<IActionResult> Receive(
        [FromServices] IConfiguration _config, 
        [FromServices] ILogger<WebhookController> _logger
    ) {
        using var reader = new StreamReader(Request.Body);
        string jsonRequest = await reader.ReadToEndAsync();
 
        var secret = _config.GetValue<string>("Typeform:WebhookSecret");
        string typeFormSig = Request.Headers["Typeform-Signature"];
        string generatedSig = $"sha256={CreateToken(jsonRequest, secret)}";
        
        _logger.LogInformation($"SIGNATURE STUFF: sec: {secret} typeform: {typeFormSig}  MyGen: {generatedSig}");
        logs.Add($"SIGNATURE STUFF: sec: {secret} typeform: {typeFormSig}  MyGen: {generatedSig}");

        // return (typeFormSig == generatedSig);

        return Ok(new { Success = true });
    }

    private static string CreateToken(string message, string secret)
    {
        var encoding = new System.Text.UTF8Encoding();
        byte[] keyByte = encoding.GetBytes(secret);
        byte[] messageBytes = encoding.GetBytes(message);
        using (var hmacsha256 = new HMACSHA256(keyByte))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashmessage);
        }
    }
}
