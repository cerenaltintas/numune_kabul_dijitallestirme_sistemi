using System.ComponentModel.DataAnnotations;

namespace NumuneKabul.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? InstitutionId { get; set; }
    public string? InstitutionName { get; set; }
    public bool IsActive { get; set; }
}

public class CreateUserDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty; // "Admin", "Numune Kabul Personeli" vs.

    public int? InstitutionId { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    public string? Password { get; set; } // Null ise güncelleme

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public int? InstitutionId { get; set; }
    
    public bool IsActive { get; set; }
}

