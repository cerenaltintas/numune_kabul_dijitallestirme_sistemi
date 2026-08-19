using Microsoft.Extensions.Logging;
using NumuneKabul.Application.DTOs;
using NumuneKabul.Application.Interfaces;
using NumuneKabul.Domain.Entities;
using NumuneKabul.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace NumuneKabul.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        
        // Populate Institution Name (N+1 issue should be handled in repository ideally, but keeping it simple)
        var institutions = await _unitOfWork.Institutions.GetAllAsync();
        var instDict = institutions.ToDictionary(i => i.Id, i => i.Name);

        return users.Select(u => new UserDto 
        { 
            Id = u.Id, 
            Username = u.Username, 
            Name = u.Name, 
            Role = u.Role, 
            InstitutionId = u.InstitutionId,
            InstitutionName = u.InstitutionId.HasValue && instDict.ContainsKey(u.InstitutionId.Value) ? instDict[u.InstitutionId.Value] : null,
            IsActive = u.IsActive
        }).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return null;

        return new UserDto 
        { 
            Id = user.Id, 
            Username = user.Username, 
            Name = user.Name, 
            Role = user.Role, 
            InstitutionId = user.InstitutionId,
            IsActive = user.IsActive
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        // Check if username exists
        var existing = await _unitOfWork.Users.FindAsync(u => u.Username == dto.Username);
        if (existing.Any()) 
            throw new InvalidOperationException("Bu kullanıcı adı zaten kullanılıyor.");

        var user = new User 
        { 
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 11),
            Name = dto.Name,
            Role = dto.Role,
            InstitutionId = dto.InstitutionId,
            IsActive = dto.IsActive
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Yeni kullanıcı eklendi. ID: {Id}, Username: {Username}", user.Id, user.Username);

        return new UserDto { Id = user.Id, Username = user.Username };
    }

    public async Task UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) 
            throw new KeyNotFoundException($"Id={id} olan kullanıcı bulunamadı.");

        // Check if username exists (excluding self)
        var existing = await _unitOfWork.Users.FindAsync(u => u.Username == dto.Username && u.Id != id);
        if (existing.Any()) 
            throw new InvalidOperationException("Bu kullanıcı adı başka bir kullanıcı tarafından kullanılıyor.");

        user.Username = dto.Username;
        user.Name = dto.Name;
        user.Role = dto.Role;
        user.InstitutionId = dto.InstitutionId;
        user.IsActive = dto.IsActive;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 11);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Kullanıcı güncellendi. ID: {Id}", user.Id);
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) 
            throw new KeyNotFoundException($"Id={id} olan kullanıcı bulunamadı.");

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Kullanıcı silindi. ID: {Id}", id);
    }
}
