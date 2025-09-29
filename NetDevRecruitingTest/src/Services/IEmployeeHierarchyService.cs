using NetDevRecruitingTest.src.Domain;

namespace NetDevRecruitingTest.src.Services
{
    public interface IEmployeeHierarchyService
    {
        public List<EmployeesStructure> FillEmployeesStructures(List<Employee> employees);
        public int? GetSuperiorRowOfEmployee(int employeeId, int superiorId);
    }
}
