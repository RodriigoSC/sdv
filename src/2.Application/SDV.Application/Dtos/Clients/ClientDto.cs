
using SDV.Application.Dtos.Commons;

namespace SDV.Application.Dtos.Clients;

public class ClientDto : ICommonDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailVerified { get; set; }
    public UserProfileDto Profile { get; set; } = new();    
}
