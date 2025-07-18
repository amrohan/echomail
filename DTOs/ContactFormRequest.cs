namespace EmailRelayServer.DTOs;

public class ContactFormRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Message { get; set; }

    public string? Subject { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
}
