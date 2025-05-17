using System;
using System.Collections.Generic;

namespace WorkPlusAPI.WorkPlus.Model;

public partial class Worker
{
    public int WorkerId { get; set; }

    public string FullName { get; set; } = null!;

    public int? UserId { get; set; }

    public int TypeId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? FatherName { get; set; }

    public string? MotherName { get; set; }

    public string? Gender { get; set; }

    public string? BirthPlace { get; set; }

    public string? BirthCity { get; set; }

    public string? BloodGroup { get; set; }

    public int? AgeAtJoining { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? PresentAddress1 { get; set; }

    public string? PresentAddress2 { get; set; }

    public string? PresentAddress3 { get; set; }

    public string? PresentCity { get; set; }

    public string? PresentState { get; set; }

    public string? PermanentAddress1 { get; set; }

    public string? PermanentAddress2 { get; set; }

    public string? PermanentAddress3 { get; set; }

    public string? PermanentCity { get; set; }

    public string? PermanentState { get; set; }

    public DateOnly? DateOfJoining { get; set; }

    public DateOnly? DateOfLeaving { get; set; }

    public string? ReferenceName { get; set; }

    public string? Remarks { get; set; }

    public bool? EsiApplicable { get; set; }

    public string? EsiLocation { get; set; }

    public string? PfNo { get; set; }

    public string? NomineeName { get; set; }

    public string? NomineeRelation { get; set; }

    public int? NomineeAge { get; set; }

    public string? Pan { get; set; }

    public string? BankAccountNo { get; set; }

    public string? BankName { get; set; }

    public string? BankLocation { get; set; }

    public string? BankRtgsCode { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<JobEntry> JobEntries { get; set; } = new List<JobEntry>();

    public virtual ICollection<JobEntryWorker> JobEntryWorkers { get; set; } = new List<JobEntryWorker>();

    public virtual EmployeeType Type { get; set; } = null!;

    public virtual User? User { get; set; }
}
