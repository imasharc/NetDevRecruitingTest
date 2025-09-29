namespace NetDevRecruitingTest.src.Domain
{
    public class EmployeesStructure
    {
        public int EmployeeId { get; }
        public int SuperiorId { get; }
        public int Row { get; }

        public EmployeesStructure(int employeeId, int superiorId, int row)
        {
            if (row < 1) throw new ArgumentOutOfRangeException(nameof(row), "Row musi być co najmniej 1.");
            EmployeeId = employeeId;
            SuperiorId = superiorId;
            Row = row;
        }
    }
}
