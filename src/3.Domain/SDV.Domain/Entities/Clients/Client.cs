using SDV.Domain.Entities.Clients.ValueObjects;
using SDV.Domain.Entities.Commons;
using SDV.Domain.Enums;
using SDV.Domain.Enums.Clients;

namespace SDV.Domain.Entities.Clients;

public class Client : BaseEntity
{
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool EmailVerified { get; private set; }
    public string? VerificationToken { get; private set; }
    public DateTime? TokenExpirationDate { get; private set; }
    public string? JobTitle { get; private set; }
    public string? Bio { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    public Client(string name, Email email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password cannot be empty.", nameof(passwordHash));
        
        Id = Guid.NewGuid(); 
        Name = name;
        Email = email;
        PasswordHash = passwordHash;

        IsActive = true;
        EmailVerified = false;
    }

    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty.", nameof(newName));
        if (newName == Name) return;

        Name = newName;
        MarkAsUpdated();
    }

    public void UpdateProfile(string? jobTitle, string? bio, string? profileImageUrl, string? phoneNumber)
    {
        JobTitle = jobTitle;
        Bio = bio;
        ProfileImageUrl = profileImageUrl;
        PhoneNumber = phoneNumber;
        MarkAsUpdated();
    }

    public void UpdateEmail(Email newEmail)
    {
        if (newEmail.Equals(Email)) return;

        Email = newEmail;
        EmailVerified = false;
        GenerateEmailVerificationToken();
        MarkAsUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    public void ResetPassword(string tempPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(tempPasswordHash))
            throw new ArgumentException("Temporary password cannot be empty.", nameof(tempPasswordHash));

        PasswordHash = tempPasswordHash;
        MarkAsUpdated();
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        MarkAsUpdated();
    }

    public void GenerateEmailVerificationToken()
    {
        VerificationToken = Guid.NewGuid().ToString("N");
        TokenExpirationDate = DateTime.UtcNow.AddHours(24);
        EmailVerified = false;
        MarkAsUpdated();
    }

    public EmailVerificationResult VerifyEmail(string token)
    {
        if (EmailVerified) return EmailVerificationResult.AlreadyVerified;
        if (string.IsNullOrWhiteSpace(token) || token != VerificationToken)
            return EmailVerificationResult.InvalidToken;
        if (TokenExpirationDate < DateTime.UtcNow)
        {
            ClearToken();
            return EmailVerificationResult.Expired;
        }

        EmailVerified = true;
        ClearToken();
        return EmailVerificationResult.Success;
    }

    private void ClearToken()
    {
        VerificationToken = null;
        TokenExpirationDate = null;
        MarkAsUpdated();
    }
}
