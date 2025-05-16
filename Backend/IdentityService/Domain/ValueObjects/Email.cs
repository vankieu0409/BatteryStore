using IdentityService.Domain.Common;

namespace IdentityService.Domain.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; private set; } = string.Empty;
    
    // Constructor cho EF Core
    private Email() { }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty");
            
        if (!IsValidEmail(email))
            throw new DomainException("Invalid email format");
            
        return new Email(email);
    }
    
    private static bool IsValidEmail(string email)
    {
        try {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch {
            return false;
        }
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }
    
    public static implicit operator string(Email email) => email.Value;
}
