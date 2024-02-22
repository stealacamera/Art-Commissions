namespace ArtCommissions.Common.DTOs;

public class EmailSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }

    // Information desk email
    public string InfoDeskName { get; set; } = null!;
    public string InfoDeskEmail { get; set; } = null!;
    public string InfoDeskPassword { get; set; } = null!;
}