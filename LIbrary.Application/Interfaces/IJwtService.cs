using Library.Domain.Entities;

namespace Library.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}