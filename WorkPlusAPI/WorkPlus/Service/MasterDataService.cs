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

        public async Task<RoleDTO> GetRoleAsync(int id)
        {
            try
            {
                var role = await _loginContext.Roles
                    .Where(r => r.Id == id)
                    .Select(r => new RoleDTO
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description
                    })
                    .FirstOrDefaultAsync();

                return role;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role with ID {RoleId}", id);
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

        #region JobGroups
        public async Task<IEnumerable<JobGroupDTO>> GetJobGroupsAsync()
        {
            try
            {
                var jobGroups = await _context.JobGroups
                    .OrderBy(g => g.GroupName)
                    .Select(g => new JobGroupDTO
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        MinWorkers = g.MinWorkers,
                        MaxWorkers = g.MaxWorkers
                    })
                    .ToListAsync();

                return jobGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job groups");
                throw;
            }
        }

        public async Task<JobGroupDTO> GetJobGroupAsync(int id)
        {
            try
            {
                var jobGroup = await _context.JobGroups
                    .Where(g => g.GroupId == id)
                    .Select(g => new JobGroupDTO
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        MinWorkers = g.MinWorkers,
                        MaxWorkers = g.MaxWorkers
                    })
                    .FirstOrDefaultAsync();

                return jobGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job group with ID {GroupId}", id);
                throw;
            }
        }

        public async Task<JobGroupDTO> CreateJobGroupAsync(JobGroupDTO jobGroupDto)
        {
            try
            {
                var jobGroup = new JobGroup
                {
                    GroupName = jobGroupDto.GroupName,
                    MinWorkers = jobGroupDto.MinWorkers,
                    MaxWorkers = jobGroupDto.MaxWorkers
                };

                _context.JobGroups.Add(jobGroup);
                await _context.SaveChangesAsync();

                jobGroupDto.GroupId = jobGroup.GroupId;
                return jobGroupDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job group");
                throw;
            }
        }

        public async Task<bool> UpdateJobGroupAsync(JobGroupDTO jobGroupDto)
        {
            try
            {
                var jobGroup = await _context.JobGroups.FindAsync(jobGroupDto.GroupId);
                if (jobGroup == null)
                    return false;

                jobGroup.GroupName = jobGroupDto.GroupName;
                jobGroup.MinWorkers = jobGroupDto.MinWorkers;
                jobGroup.MaxWorkers = jobGroupDto.MaxWorkers;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job group with ID {GroupId}", jobGroupDto.GroupId);
                throw;
            }
        }

        public async Task<bool> DeleteJobGroupAsync(int id)
        {
            try
            {
                var jobGroup = await _context.JobGroups.FindAsync(id);
                if (jobGroup == null)
                    return false;

                // Check if the job group has associated group members
                var hasGroupMembers = await _context.GroupMembers.AnyAsync(gm => gm.GroupId == id);
                if (hasGroupMembers)
                {
                    throw new InvalidOperationException("Cannot delete job group that has associated group members");
                }

                // Check if the job group is used in any job entries
                var hasJobEntries = await _context.JobEntries.AnyAsync(je => je.GroupId == id);
                if (hasJobEntries)
                {
                    throw new InvalidOperationException("Cannot delete job group that is used in job entries");
                }

                _context.JobGroups.Remove(jobGroup);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException)
            {
                // Rethrow business rule violations
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job group with ID {GroupId}", id);
                throw;
            }
        }
        #endregion

        #region GroupMembers
        public async Task<IEnumerable<GroupMemberDTO>> GetGroupMembersAsync()
        {
            try
            {
                var groupMembers = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Include(gm => gm.Worker)
                    .OrderBy(gm => gm.Group.GroupName)
                    .ThenBy(gm => gm.Worker.FullName)
                    .Select(gm => new GroupMemberDTO
                    {
                        Id = gm.Id,
                        GroupId = gm.GroupId,
                        WorkerId = gm.WorkerId,
                        GroupName = gm.Group.GroupName,
                        WorkerName = gm.Worker.FullName
                    })
                    .ToListAsync();

                return groupMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group members");
                throw;
            }
        }

        public async Task<IEnumerable<GroupMemberDTO>> GetGroupMembersByGroupAsync(int groupId)
        {
            try
            {
                var groupMembers = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Include(gm => gm.Worker)
                    .Where(gm => gm.GroupId == groupId)
                    .OrderBy(gm => gm.Worker.FullName)
                    .Select(gm => new GroupMemberDTO
                    {
                        Id = gm.Id,
                        GroupId = gm.GroupId,
                        WorkerId = gm.WorkerId,
                        GroupName = gm.Group.GroupName,
                        WorkerName = gm.Worker.FullName
                    })
                    .ToListAsync();

                return groupMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group members for group ID {GroupId}", groupId);
                throw;
            }
        }

        public async Task<GroupMemberDTO> GetGroupMemberAsync(int id)
        {
            try
            {
                var groupMember = await _context.GroupMembers
                    .Include(gm => gm.Group)
                    .Include(gm => gm.Worker)
                    .Where(gm => gm.Id == id)
                    .Select(gm => new GroupMemberDTO
                    {
                        Id = gm.Id,
                        GroupId = gm.GroupId,
                        WorkerId = gm.WorkerId,
                        GroupName = gm.Group.GroupName,
                        WorkerName = gm.Worker.FullName
                    })
                    .FirstOrDefaultAsync();

                return groupMember;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group member with ID {Id}", id);
                throw;
            }
        }

        public async Task<GroupMemberDTO> CreateGroupMemberAsync(GroupMemberCreateDTO groupMemberDto)
        {
            try
            {
                // Check if the worker already exists in the group
                bool exists = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupId == groupMemberDto.GroupId && gm.WorkerId == groupMemberDto.WorkerId);
                
                if (exists)
                {
                    throw new InvalidOperationException("Worker is already a member of this group");
                }

                // Verify that group exists
                var group = await _context.JobGroups.FindAsync(groupMemberDto.GroupId);
                if (group == null)
                {
                    throw new InvalidOperationException("Group not found");
                }

                // Verify that worker exists and is active
                var worker = await _context.Workers.FindAsync(groupMemberDto.WorkerId);
                if (worker == null || worker.IsActive != true)
                {
                    throw new InvalidOperationException("Worker not found or inactive");
                }

                // Check if adding this worker would exceed the maximum workers for the group
                var currentMemberCount = await _context.GroupMembers
                    .CountAsync(gm => gm.GroupId == groupMemberDto.GroupId);
                
                if (currentMemberCount >= group.MaxWorkers)
                {
                    throw new InvalidOperationException($"Cannot add more workers. Group has reached its maximum capacity of {group.MaxWorkers} workers");
                }

                var groupMember = new GroupMember
                {
                    GroupId = groupMemberDto.GroupId,
                    WorkerId = groupMemberDto.WorkerId
                };

                _context.GroupMembers.Add(groupMember);
                await _context.SaveChangesAsync();

                // Fetch the complete group member with navigation properties
                var createdGroupMember = await GetGroupMemberAsync(groupMember.Id);
                return createdGroupMember;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group member");
                throw;
            }
        }

        public async Task<bool> DeleteGroupMemberAsync(int id)
        {
            try
            {
                var groupMember = await _context.GroupMembers.FindAsync(id);
                if (groupMember == null)
                    return false;

                // Check if there are any job entries that reference this group
                var hasJobEntries = await _context.JobEntries
                    .AnyAsync(je => je.GroupId == groupMember.GroupId);

                if (hasJobEntries)
                {
                    // Instead of preventing deletion, just log a warning
                    _logger.LogWarning("Deleting a member from group ID {GroupId} which has associated job entries", groupMember.GroupId);
                }

                _context.GroupMembers.Remove(groupMember);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group member with ID {Id}", id);
                throw;
            }
        }
        #endregion

        #region Jobs
        public async Task<IEnumerable<JobDTO>> GetJobsAsync()
        {
            try
            {
                var jobs = await _context.Jobs
                    .Include(j => j.JobType)
                    .Include(j => j.CreatedByNavigation)
                    .OrderByDescending(j => j.JobId)
                    .Select(j => new JobDTO
                    {
                        JobId = j.JobId,
                        JobName = j.JobName,
                        JobTypeId = j.JobTypeId,
                        RatePerItem = j.RatePerItem,
                        RatePerHour = j.RatePerHour,
                        ExpectedHours = j.ExpectedHours,
                        ExpectedItemsPerHour = j.ExpectedItemsPerHour,
                        IncentiveBonusRate = j.IncentiveBonusRate,
                        PenaltyRate = j.PenaltyRate,
                        IncentiveType = j.IncentiveType,
                        CreatedBy = j.CreatedBy,
                        JobTypeName = j.JobType.TypeName,
                        CreatedByName = j.CreatedByNavigation.FirstName + " " + j.CreatedByNavigation.LastName
                    })
                    .ToListAsync();

                return jobs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting jobs");
                throw;
            }
        }

        public async Task<JobDTO> GetJobAsync(int id)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.JobType)
                    .Include(j => j.CreatedByNavigation)
                    .Where(j => j.JobId == id)
                    .Select(j => new JobDTO
                    {
                        JobId = j.JobId,
                        JobName = j.JobName,
                        JobTypeId = j.JobTypeId,
                        RatePerItem = j.RatePerItem,
                        RatePerHour = j.RatePerHour,
                        ExpectedHours = j.ExpectedHours,
                        ExpectedItemsPerHour = j.ExpectedItemsPerHour,
                        IncentiveBonusRate = j.IncentiveBonusRate,
                        PenaltyRate = j.PenaltyRate,
                        IncentiveType = j.IncentiveType,
                        CreatedBy = j.CreatedBy,
                        JobTypeName = j.JobType.TypeName,
                        CreatedByName = j.CreatedByNavigation.FirstName + " " + j.CreatedByNavigation.LastName
                    })
                    .FirstOrDefaultAsync();

                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job with ID {JobId}", id);
                throw;
            }
        }

        public async Task<JobDTO> CreateJobAsync(JobCreateDTO jobDto)
        {
            try
            {
                // Verify that job type exists
                var jobType = await _context.JobTypes.FindAsync(jobDto.JobTypeId);
                if (jobType == null)
                {
                    throw new InvalidOperationException("Job type not found");
                }

                // Verify that user exists
                var user = await _loginContext.Users.FindAsync(jobDto.CreatedBy);
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                var job = new WorkPlus.Model.Job
                {
                    JobName = jobDto.JobName,
                    JobTypeId = jobDto.JobTypeId,
                    RatePerItem = jobDto.RatePerItem,
                    RatePerHour = jobDto.RatePerHour,
                    ExpectedHours = jobDto.ExpectedHours,
                    ExpectedItemsPerHour = jobDto.ExpectedItemsPerHour,
                    IncentiveBonusRate = jobDto.IncentiveBonusRate,
                    PenaltyRate = jobDto.PenaltyRate,
                    IncentiveType = jobDto.IncentiveType,
                    CreatedBy = jobDto.CreatedBy
                };

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                // Fetch the complete job with navigation properties
                var createdJob = await GetJobAsync(job.JobId);
                return createdJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating job");
                throw;
            }
        }

        public async Task<bool> UpdateJobAsync(JobDTO jobDto)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(jobDto.JobId);
                if (job == null)
                    return false;

                // Verify that job type exists
                var jobType = await _context.JobTypes.FindAsync(jobDto.JobTypeId);
                if (jobType == null)
                {
                    throw new InvalidOperationException("Job type not found");
                }

                job.JobName = jobDto.JobName;
                job.JobTypeId = jobDto.JobTypeId;
                job.RatePerItem = jobDto.RatePerItem;
                job.RatePerHour = jobDto.RatePerHour;
                job.ExpectedHours = jobDto.ExpectedHours;
                job.ExpectedItemsPerHour = jobDto.ExpectedItemsPerHour;
                job.IncentiveBonusRate = jobDto.IncentiveBonusRate;
                job.PenaltyRate = jobDto.PenaltyRate;
                job.IncentiveType = jobDto.IncentiveType;
                // We don't update CreatedBy since it represents who created the job originally

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job with ID {JobId}", jobDto.JobId);
                throw;
            }
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                    return false;

                // Check if the job is used in any job entries
                var hasJobEntries = await _context.JobEntries.AnyAsync(je => je.JobId == id);
                if (hasJobEntries)
                {
                    throw new InvalidOperationException("Cannot delete job that is used in job entries");
                }

                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (InvalidOperationException)
            {
                // Rethrow business rule violations
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job with ID {JobId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<dynamic>> GetJobTypesAsync()
        {
            try
            {
                var jobTypes = await _context.JobTypes
                    .OrderBy(jt => jt.TypeName)
                    .Select(jt => new
                    {
                        JobTypeId = jt.JobTypeId,
                        JobTypeName = jt.TypeName
                    })
                    .ToListAsync();

                return jobTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job types");
                throw;
            }
        }
        #endregion

        #region JobTypes
        public async Task<IEnumerable<JobTypeDTO>> GetAllJobTypesAsync()
        {
            try
            {
                var jobTypes = await _context.JobTypes
                    .OrderBy(jt => jt.TypeName)
                    .Select(jt => new JobTypeDTO
                    {
                        JobTypeId = jt.JobTypeId,
                        TypeName = jt.TypeName
                    })
                    .ToListAsync();

                return jobTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all job types");
                throw;
            }
        }

        public async Task<JobTypeDTO> GetJobTypeAsync(int id)
        {
            try
            {
                var jobType = await _context.JobTypes
                    .Where(jt => jt.JobTypeId == id)
                    .Select(jt => new JobTypeDTO
                    {
                        JobTypeId = jt.JobTypeId,
                        TypeName = jt.TypeName
                    })
                    .FirstOrDefaultAsync();

                return jobType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job type with ID {JobTypeId}", id);
                throw;
            }
        }
        #endregion

        #region EmployeeTypes
        public async Task<IEnumerable<EmployeeTypeDTO>> GetEmployeeTypesAsync()
        {
            try
            {
                var employeeTypes = await _context.EmployeeTypes
                    .OrderBy(et => et.TypeName)
                    .Select(et => new EmployeeTypeDTO
                    {
                        TypeId = et.TypeId,
                        TypeName = et.TypeName
                    })
                    .ToListAsync();

                return employeeTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee types");
                throw;
            }
        }

        public async Task<EmployeeTypeDTO> GetEmployeeTypeAsync(int id)
        {
            try
            {
                var employeeType = await _context.EmployeeTypes
                    .Where(et => et.TypeId == id)
                    .Select(et => new EmployeeTypeDTO
                    {
                        TypeId = et.TypeId,
                        TypeName = et.TypeName
                    })
                    .FirstOrDefaultAsync();

                return employeeType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting employee type with ID {TypeId}", id);
                throw;
            }
        }
        #endregion
    }
} 