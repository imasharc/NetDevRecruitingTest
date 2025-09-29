namespace NetDevRecruitingTest.src.Domain
{
    public class Team
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}