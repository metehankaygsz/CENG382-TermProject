using System;

namespace ClassManagementSystem.Data.Models
{
    public class Holiday
    {
        public int      Id      { get; set; }
        public DateTime Date    { get; set; }
        public string   Name    { get; set; } = default!;
    }
}