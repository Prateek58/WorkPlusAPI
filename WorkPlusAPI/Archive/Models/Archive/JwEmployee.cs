using System;
using System.Collections.Generic;

namespace WorkPlusAPI.Archive.Models.Archive;

public partial class JwEmployee
{
    public int EmployeeId { get; set; }

    public string NameDisplay { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? FatherName { get; set; }

    public string? MotherName { get; set; }

    public string? PhoneNo { get; set; }

    public string? Email { get; set; }

    public string? Gender { get; set; }

    public string? PresentAddress1 { get; set; }

    public string? PresentAddress2 { get; set; }

    public string? PresentAddress3 { get; set; }

    public string? PresentCity { get; set; }

    public string? PresentState { get; set; }

    public string? BirthPlace { get; set; }

    public string? BirthDistrict { get; set; }

    public string? BloodGroup { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? DateOfJoining { get; set; }

    public DateOnly? DateOfLeaving { get; set; }

    public sbyte? AgeAtJoining { get; set; }

    public string? ReferenceName { get; set; }

    public string? Remarks { get; set; }

    public bool? EsiApplicable { get; set; }

    public string? EsiNo { get; set; }

    public string? EsiDispensoryNo { get; set; }

    public string? EsiLocation { get; set; }

    public bool? PfApplicable { get; set; }

    public string? PfNo { get; set; }

    public string? NomineeName { get; set; }

    public string? NomineeRelation { get; set; }

    public sbyte? NomineeAge { get; set; }

    public string? Pan { get; set; }

    public string? BankAccountNo { get; set; }

    public string? BankName { get; set; }

    public string? BankBranch { get; set; }

    public string? BankLocation { get; set; }

    public string? BankRtgsCode { get; set; }

    public sbyte? EmployeeTypeId { get; set; }

    public decimal? RatePerHour { get; set; }

    public decimal? RatePerDay { get; set; }

    public DateOnly? SalaryIncrementDueDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual JwEmployeeType? EmployeeType { get; set; }
}
