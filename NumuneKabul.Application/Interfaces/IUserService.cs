using NumuneKabul.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NumuneKabul.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task UpdateUserAsync(int id, UpdateUserDto dto);
    Task DeleteUserAsync(int id);
}
