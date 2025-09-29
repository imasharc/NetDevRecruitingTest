using NetDevRecruitingTest.src.Data;
using NetDevRecruitingTest.src.Domain;
using Microsoft.EntityFrameworkCore;

namespace NetDevRecruitingTest.src.Services
{
    public class VacationService : IVacationService
    {
        private readonly AppDbContext _context;
        private const int HoursPerDay = 8;  // Konfigurowalny przelicznik

        public VacationService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<Employee> GetDotNetEmployeesWithVacationsIn2019()
        {
            return _context.Employees
                .Include(e => e.Team)
                .Where(e => e.Team.Name == ".NET" &&
                            e.Vacations.Any(v => v.DateSince.Year == 2019))
                .ToList();
        }

        public IEnumerable<(Employee Employee, int UsedDays)> GetEmployeesWithUsedDaysInCurrentYear()
        {
            var currentYear = DateTime.Now.Year;
            var now = DateTime.Now;

            return _context.Employees
                .Include(e => e.Vacations)
                .Select(e => new
                {
                    Employee = e,
                    UsedDays = e.Vacations
                        .Where(v => v.DateSince.Year == currentYear && v.DateUntil < now)
                        .Sum(v => v.IsPartialVacation
                            ? (int)Math.Ceiling((double)v.NumberOfHours / HoursPerDay)
                            : (v.DateUntil - v.DateSince).Days + 1)
                })
                .Where(x => x.UsedDays > 0)
                .AsEnumerable()
                .Select(x => (x.Employee, x.UsedDays));
        }

        public IEnumerable<Team> GetTeamsWithoutVacationsIn2019()
        {
            return _context.Teams
                .Include(t => t.Employees).ThenInclude(e => e.Vacations)
                .Where(t => !t.Employees.Any(e => e.Vacations.Any(v => v.DateSince.Year == 2019)))
                .ToList();
        }
    }
}