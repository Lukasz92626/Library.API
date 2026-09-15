using MediatR;
using Library.Domain.Entities;
using Library.Domain.Exceptions;
using Library.Domain.Interfaces;
using Library.Application.DTOs.Auth;
using Library.Application.Interfaces;

namespace Library.Application.Auth.Commands;

public record RegisterCommand(string Email, string Password, string FullName) : IRequest<AuthResponse>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogger _activityLogger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IActivityLogger activityLogger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _activityLogger = activityLogger;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Chcek if the email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new InvalidOperationDomainException("Email already exists.");

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Points = 0,
            TotalFines = 0,
            JoinDate = DateTime.UtcNow,
            Role = "User"
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        await _activityLogger.LogAsync(user.Id, "Registered", new
        {
            Email = user.Email,
            FullName = user.FullName
        }, cancellationToken);

        // generate JWT token
        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            Points = user.Points
        };
    }
}