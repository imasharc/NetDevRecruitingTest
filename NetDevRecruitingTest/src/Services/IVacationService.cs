using NetDevRecruitingTest.src.Domain;

namespace NetDevRecruitingTest.src.Services
{
    public interface IVacationService
    {
        IEnumerable<Employee> GetDotNetEmployeesWithVacationsIn2019();
        IEnumerable<(Employee Employee, int UsedDays)> GetEmployeesWithUsedDaysInCurrentYear();
        IEnumerable<Team> GetTeamsWithoutVacationsIn2019();
    }
}