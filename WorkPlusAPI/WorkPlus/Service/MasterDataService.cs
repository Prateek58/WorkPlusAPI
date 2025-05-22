using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WorkPlusAPI.Archive.Data.Workplus;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Model;

namespace WorkPlusAPI.WorkPlus.Service
{
    public class MasterDataService : IMasterDataService
    {
        private readonly WorkPlusContext _context;
        private readonly LoginWorkPlusContext _loginContext;
        private readonly ILogger<MasterDataService> _logger;

        public MasterDataService(WorkPlusContext context, LoginWorkPlusContext loginContext, ILogger<MasterDataService> logger)
        {
            _context = context;
            _loginContext = loginContext;
            _logger = logger;
        }

        #region Workers
        public async Task<IEnumerable<WorkerDTO>> GetWorkersAsync()
        {
            try
            {
                var workers = await _context.Workers
                    .Where(w => w.IsActive == true)
                    .OrderByDescending(w => w.WorkerId)
                    .Select(w => new WorkerDTO
                    {
                        WorkerId = w.WorkerId,
                        FullName = w.FullName,
                        UserId = w.UserId,
                        TypeId = w.TypeId,
                        FirstName = w.FirstName,
                        LastName = w.LastName,
                        FatherName = w.FatherName,
                        MotherName = w.MotherName,
                        Gender = w.Gender,
                        BirthPlace = w.BirthPlace,
                        BirthCity = w.BirthCity,
                        BloodGroup = w.BloodGroup,
                        AgeAtJoining = w.AgeAtJoining,
                        Phone = w.Phone,
                        Email = w.Email,
                        PresentAddress1 = w.PresentAddress1,
                        PresentAddress2 = w.PresentAddress2,
                        PresentAddress3 = w.PresentAddress3,
                        PresentCity = w.PresentCity,
                        PresentState = w.PresentState,
                        PermanentAddress1 = w.PermanentAddress1,
                        PermanentAddress2 = w.PermanentAddress2,
                        PermanentAddress3 = w.PermanentAddress3,
                        PermanentCity = w.PermanentCity,
                        PermanentState = w.PermanentState,
                        DateOfJoining = w.DateOfJoining,
                        DateOfLeaving = w.DateOfLeaving,
                        ReferenceName = w.ReferenceName,
                        Remarks = w.Remarks,
                        EsiApplicable = w.EsiApplicable,
                        EsiLocation = w.EsiLocation,
                        PfNo = w.PfNo,
                        NomineeName = w.NomineeName,
                        NomineeRelation = w.NomineeRelation,
                        NomineeAge = w.NomineeAge,
                        Pan = w.Pan,
                        BankAccountNo = w.BankAccountNo,
                        BankName = w.BankName,
                        BankLocation = w.BankLocation,
                        BankRtgsCode = w.BankRtgsCode,
                        IsActive = w.IsActive
                    })
                    .ToListAsync();

                return workers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting workers");
                throw;
            }
        }

        public async Task<WorkerDTO> GetWorkerAsync(int id)
        {
            try
            {
                var worker = await _context.Workers
                    .Where(w => w.WorkerId == id && w.IsActive == true)
                    .Select(w => new WorkerDTO
                    {
                        WorkerId = w.WorkerId,
                        FullName = w.FullName,
                        UserId = w.UserId,
                        TypeId = w.TypeId,
                        FirstName = w.FirstName,
                        LastName = w.LastName,
                        FatherName = w.FatherName,
                        MotherName = w.MotherName,
                        Gender = w.Gender,
                        BirthPlace = w.BirthPlace,
                        BirthCity = w.BirthCity,
                        BloodGroup = w.BloodGroup,
                        AgeAtJoining = w.AgeAtJoining,
                        Phone = w.Phone,
                        Email = w.Email,
                        PresentAddress1 = w.PresentAddress1,
                        PresentAddress2 = w.PresentAddress2,
                        PresentAddress3 = w.PresentAddress3,
                        PresentCity = w.PresentCity,
                        PresentState = w.PresentState,
                        PermanentAddress1 = w.PermanentAddress1,
                        PermanentAddress2 = w.PermanentAddress2,
                        PermanentAddress3 = w.PermanentAddress3,
                        PermanentCity = w.PermanentCity,
                        PermanentState = w.PermanentState,
                        DateOfJoining = w.DateOfJoining,
                        DateOfLeaving = w.DateOfLeaving,
                        ReferenceName = w.ReferenceName,
                        Remarks = w.Remarks,
                        EsiApplicable = w.EsiApplicable,
                        EsiLocation = w.EsiLocation,
                        PfNo = w.PfNo,
                        NomineeName = w.NomineeName,
                        NomineeRelation = w.NomineeRelation,
                        NomineeAge = w.NomineeAge,
                        Pan = w.Pan,
                        BankAccountNo = w.BankAccountNo,
                        BankName = w.BankName,
                        BankLocation = w.BankLocation,
                        BankRtgsCode = w.BankRtgsCode,
                        IsActive = w.IsActive
                    })
                    .FirstOrDefaultAsync();

                return worker;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting worker with ID {WorkerId}", id);
                throw;
            }
        }

        public async Task<WorkerDTO> CreateWorkerAsync(WorkerDTO workerDto)
        {
            try
            {
                var worker = new Worker
                {
                    FullName = workerDto.FullName,
                    UserId = workerDto.UserId,
                    TypeId = workerDto.TypeId,
                    FirstName = workerDto.FirstName,
                    LastName = workerDto.LastName,
                    FatherName = workerDto.FatherName,
                    MotherName = workerDto.MotherName,
                    Gender = workerDto.Gender,
                    BirthPlace = workerDto.BirthPlace,
                    BirthCity = workerDto.BirthCity,
                    BloodGroup = workerDto.BloodGroup,
                    AgeAtJoining = workerDto.AgeAtJoining,
                    Phone = workerDto.Phone,
                    Email = workerDto.Email,
                    PresentAddress1 = workerDto.PresentAddress1,
                    PresentAddress2 = workerDto.PresentAddress2,
                    PresentAddress3 = workerDto.PresentAddress3,
                    PresentCity = workerDto.PresentCity,
                    PresentState = workerDto.PresentState,
                    PermanentAddress1 = workerDto.PermanentAddress1,
                    PermanentAddress2 = workerDto.PermanentAddress2,
                    PermanentAddress3 = workerDto.PermanentAddress3,
                    PermanentCity = workerDto.PermanentCity,
                    PermanentState = workerDto.PermanentState,
                    DateOfJoining = workerDto.DateOfJoining,
                    DateOfLeaving = workerDto.DateOfLeaving,
                    ReferenceName = workerDto.ReferenceName,
                    Remarks = workerDto.Remarks,
                    EsiApplicable = workerDto.EsiApplicable,
                    EsiLocation = workerDto.EsiLocation,
                    PfNo = workerDto.PfNo,
                    NomineeName = workerDto.NomineeName,
                    NomineeRelation = workerDto.NomineeRelation,
                    NomineeAge = workerDto.NomineeAge,
                    Pan = workerDto.Pan,
                    BankAccountNo = workerDto.BankAccountNo,
                    BankName = workerDto.BankName,
                    BankLocation = workerDto.BankLocation,
                    BankRtgsCode = workerDto.BankRtgsCode,
                    IsActive = true
                };

                _context.Workers.Add(worker);
                await _context.SaveChangesAsync();

                workerDto.WorkerId = worker.WorkerId;
                return workerDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating worker");
                throw;
            }
        }

        public async Task<bool> UpdateWorkerAsync(WorkerDTO workerDto)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(workerDto.WorkerId);
                if (worker == null || worker.IsActive != true)
                    return false;

                worker.FullName = workerDto.FullName;
                worker.UserId = workerDto.UserId;
                worker.TypeId = workerDto.TypeId;
                worker.FirstName = workerDto.FirstName;
                worker.LastName = workerDto.LastName;
                worker.FatherName = workerDto.FatherName;
                worker.MotherName = workerDto.MotherName;
                worker.Gender = workerDto.Gender;
                worker.BirthPlace = workerDto.BirthPlace;
                worker.BirthCity = workerDto.BirthCity;
                worker.BloodGroup = workerDto.BloodGroup;
                worker.AgeAtJoining = workerDto.AgeAtJoining;
                worker.Phone = workerDto.Phone;
                worker.Email = workerDto.Email;
                worker.PresentAddress1 = workerDto.PresentAddress1;
                worker.PresentAddress2 = workerDto.PresentAddress2;
                worker.PresentAddress3 = workerDto.PresentAddress3;
                worker.PresentCity = workerDto.PresentCity;
                worker.PresentState = workerDto.PresentState;
                worker.PermanentAddress1 = workerDto.PermanentAddress1;
                worker.PermanentAddress2 = workerDto.PermanentAddress2;
                worker.PermanentAddress3 = workerDto.PermanentAddress3;
                worker.PermanentCity = workerDto.PermanentCity;
                worker.PermanentState = workerDto.PermanentState;
                worker.DateOfJoining = workerDto.DateOfJoining;
                worker.DateOfLeaving = workerDto.DateOfLeaving;
                worker.ReferenceName = workerDto.ReferenceName;
                worker.Remarks = workerDto.Remarks;
                worker.EsiApplicable = workerDto.EsiApplicable;
                worker.EsiLocation = workerDto.EsiLocation;
                worker.PfNo = workerDto.PfNo;
                worker.NomineeName = workerDto.NomineeName;
                worker.NomineeRelation = workerDto.NomineeRelation;
                worker.NomineeAge = workerDto.NomineeAge;
                worker.Pan = workerDto.Pan;
                worker.BankAccountNo = workerDto.BankAccountNo;
                worker.BankName = workerDto.BankName;
                worker.BankLocation = workerDto.BankLocation;
                worker.BankRtgsCode = workerDto.BankRtgsCode;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating worker with ID {WorkerId}", workerDto.WorkerId);
                throw;
            }
        }

        public async Task<bool> DeleteWorkerAsync(int id)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(id);
                if (worker == null || worker.IsActive != true)
                    return false;

                worker.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting worker with ID {WorkerId}", id);
                throw;
            }
        }
        #endregion

        #region Users
        public async Task<IEnumerable<UserDTO>> GetUsersAsync()
        {
            try
            {
                var users = await _loginContext.Users
                    .OrderByDescending(u => u.Id)
                    .Select(u => new UserDTO
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt ?? DateTime.Now,
                        UpdatedAt = u.UpdatedAt ?? DateTime.Now,
                        Password = null,
                        PasswordHash = null
                    })
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                throw;
            }
        }

        public async Task<UserDTO> GetUserAsync(int id)
        {
            try
            {
                var user = await _loginContext.Users
                    .Where(u => u.Id == id)
                    .Select(u => new UserDTO
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt ?? DateTime.Now,
                        UpdatedAt = u.UpdatedAt ?? DateTime.Now,
                        Password = null,
                        PasswordHash = null
                    })
                    .FirstOrDefaultAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with ID {UserId}", id);
                throw;
            }
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO userDto)
        {
            try
            {
                // Check if username or email already exists
                if (await _loginContext.Users.AnyAsync(u => u.Username == userDto.Username || u.Email == userDto.Email))
                {
                    throw new InvalidOperationException("Username or email already exists");
                }

                var user = new Archive.Models.WorkPlus.User
                {
                    Username = userDto.Username,
                    Email = userDto.Email,
                    PasswordHash = HashPassword(userDto.Password),
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    IsActive = userDto.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Assign default user role
                var userRole = await _loginContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole != null)
                {
                    user.Roles.Add(userRole);
                }

                _loginContext.Users.Add(user);
                await _loginContext.SaveChangesAsync();

                userDto.Id = user.Id;
                userDto.CreatedAt = user.CreatedAt ?? DateTime.Now;
                userDto.UpdatedAt = user.UpdatedAt ?? DateTime.Now;
                
                // Don't return the password or hash
                userDto.Password = null;
                userDto.PasswordHash = null;
                
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(UserDTO userDto)
        {
            try
            {
                var user = await _loginContext.Users.FindAsync(userDto.Id);
                if (user == null)
                    return false;

                // Update user properties
                user.Email = userDto.Email;
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.IsActive = userDto.IsActive;
                user.UpdatedAt = DateTime.Now;

                // If password is provided, update it
                if (!string.IsNullOrEmpty(userDto.Password))
                {
                    user.PasswordHash = HashPassword(userDto.Password);
                }

                await _loginContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {UserId}", userDto.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _loginContext.Users.FindAsync(id);
                if (user == null)
                    return false;

                // Instead of actually deleting, just set IsActive to false
                user.IsActive = false;
                user.UpdatedAt = DateTime.Now;

                await _loginContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user with ID {UserId}", id);
                throw;
            }
        }

        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            byte[] salt = Encoding.UTF8.GetBytes("WorkPlusStaticSalt123!@#");
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(hash);
        }
        #endregion

        #region Roles
        public async Task<IEnumerable<RoleDTO>> GetRolesAsync()
        {
            try
            {
                var roles = await _loginContext.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => new RoleDTO
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description
                    })
                    .ToListAsync();

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                throw;
            }
        }

        public async Task<UserRoleDTO> GetUserRolesAsync(int userId)
        {
            try
            {
                // Check if user exists
                var user = await _loginContext.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    throw new InvalidOperationException($"User with ID {userId} not found");
                }

                // Get all roles and separate into assigned and available
                var allRoles = await _loginContext.Roles.ToListAsync();
                var userRoleIds = user.Roles.Select(r => r.Id).ToHashSet();

                var result = new UserRoleDTO
                {
                    UserId = user.Id,
                    Username = user.Username,
                    AssignedRoles = allRoles
                        .Where(r => userRoleIds.Contains(r.Id))
                        .Select(r => new RoleDTO
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Description = r.Description
                        })
                        .ToList(),
                    AvailableRoles = allRoles
                        .Where(r => !userRoleIds.Contains(r.Id))
                        .Select(r => new RoleDTO
                        {
                            Id = r.Id,
                            Name = r.Name,
                            Description = r.Description
                        })
                        .ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user with ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> AssignUserRolesAsync(UserRoleAssignmentDTO assignmentDto)
        {
            try
            {
                // Get user with current roles
                var user = await _loginContext.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == assignmentDto.UserId);

                if (user == null)
                {
                    return false;
                }

                // Get all roles that should be assigned
                var rolesToAssign = await _loginContext.Roles
                    .Where(r => assignmentDto.RoleIds.Contains(r.Id))
                    .ToListAsync();

                // Clear current roles and assign new ones
                user.Roles.Clear();
                foreach (var role in rolesToAssign)
                {
                    user.Roles.Add(role);
                }

                await _loginContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user with ID {UserId}", assignmentDto.UserId);
                throw;
            }
        }
        #endregion
    }
} 