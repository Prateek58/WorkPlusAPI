namespace WorkPlusAPI.DTOs;

public class JobWorkResponse
{
    public IEnumerable<dynamic> Data { get; set; } = new List<dynamic>();
    public int Total { get; set; }
} 