using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPlusAPI.WorkPlus.Data;
using WorkPlusAPI.WorkPlus.DTOs;
using WorkPlusAPI.WorkPlus.Model;

namespace WorkPlusAPI.WorkPlus.Service
{
    public class MasterDataService : IMasterDataService
    {
        private readonly WorkPlusContext _context;
        private readonly ILogger<MasterDataService> _logger;

        public MasterDataService(WorkPlusContext context, ILogger<MasterDataService> logger)
        {
            _context = context;
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
                _logger.LogError(ex, "Error getting worker {Id}", id);
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

                // Map back the created worker to DTO
                workerDto.WorkerId = worker.WorkerId;
                workerDto.IsActive = worker.IsActive;
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
                if (worker == null)
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
                _logger.LogError(ex, "Error updating worker {Id}", workerDto.WorkerId);
                throw;
            }
        }

        public async Task<bool> DeleteWorkerAsync(int id)
        {
            try
            {
                var worker = await _context.Workers.FindAsync(id);
                if (worker == null)
                    return false;

                worker.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting worker {Id}", id);
                throw;
            }
        }
        #endregion
    }
} 