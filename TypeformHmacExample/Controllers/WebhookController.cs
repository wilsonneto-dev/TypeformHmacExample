using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;

namespace TypeformHmacExample.Controllers;
[ApiController]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    public static List<string> logs = new();

    [HttpGet()]
    public IActionResult Get()
    {
        return Ok(new { 
            v = "1.2", 
            Success = true, 
            logs 
        });
    }

    [HttpPost()]
    public async Task<IActionResult> Receive(
        [FromServices] IConfiguration _config, 
        [FromServices] ILogger<WebhookController> _logger
    ) {
        try
        {
            logs.Add("Chegou evento...");
            using var reader = new StreamReader(Request.Body);
            string jsonRequest = await reader.ReadToEndAsync();

            var secret = _config.GetValue<string>("Typeform:WebhookSecret");
            string typeFormSig = Request.Headers["Typeform-Signature"];
            string generatedSig = $"sha256={CreateToken(jsonRequest, secret)}";

            // logs.Add($"#{@event.EventId}/{@event.EventType} - sec: {secret} typeform: {typeFormSig}  MyGen: {generatedSig} / eq:{generatedSig.Equals(typeFormSig)}");

            logs.Add($"sec: {secret} typeform: {typeFormSig}  MyGen: {generatedSig} / eq:{generatedSig.Equals(typeFormSig)}");
            logs.Add($"req body: {jsonRequest}");

            // return (typeFormSig == generatedSig);

            var obj = JsonSerializer.Deserialize<TypeformWebhookEvent>(jsonRequest, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy()
            });
            logs.Add($"obj: {obj} - {JsonSerializer.Serialize(obj)}");

            return Ok(new { Success = true, GeneratedSIgn = generatedSig });

        }
        catch (Exception ex)
        {
            logs.Add($"Xiii {ex.Message}");
            throw;
        }
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

public static class StringUtils
{
    public static string ToSnakeCase(this string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }
}

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();
    public override string ConvertName(string name) => name.ToSnakeCase();
}

[ExcludeFromCodeCoverage]
public class TypeformWebhookEvent
{
    public string EventId { get; set; }
    public string EventType { get; set; }
    public FormResponse FormResponse { get; set; }
}



[ExcludeFromCodeCoverage]
public class FormResponse
{
    public string FormId { get; set; }
    public string Token { get; set; }
    public DateTime SubmittedAt { get; set; }



    public List<Answer> Answers { get; set; }
    public Definition Definition { get; set; }
}



[ExcludeFromCodeCoverage]
public class Answer
{
    public string Type { get; set; }



    public string Text { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string FileUrl { get; set; }
    public long? Number { get; set; }
    public bool? Boolean { get; set; }



    public Field Field { get; set; }
    public Choice Choice { get; set; }
    public Choices Choices { get; set; }
}



[ExcludeFromCodeCoverage]
public class Field
{
    public string Id { get; set; }
    public string Type { get; set; }
}



[ExcludeFromCodeCoverage]
public class Choices
{
    public List<string> Labels { get; set; }
    public string Other { get; set; }
}



[ExcludeFromCodeCoverage]
public class Choice
{
    public string Label { get; set; }
}



[ExcludeFromCodeCoverage]
public class Definition
{
    public List<DefinitionField> Fields { get; set; }
}



[ExcludeFromCodeCoverage]
public class DefinitionField
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }



    public bool AllowMultipleSelections { get; set; }
    public bool AllowOtherChoice { get; set; }



    public List<DefinitionFieldChoice> Choices { get; set; }
}



[ExcludeFromCodeCoverage]
public class DefinitionFieldChoice
{
    public string Id { get; set; }
    public string Label { get; set; }
}
