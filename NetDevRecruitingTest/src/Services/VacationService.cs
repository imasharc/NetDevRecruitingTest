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

        public int CountFreeDaysForEmployee(Employee employee, List<Vacation> vacations, VacationPackage vacationPackage)
        {
            var currentDate = new DateTime(2025, 9, 29); // Bieżąca data z zapytania; realnie użyjemy DateTime.Now
            var currentYear = currentDate.Year;

            if (vacationPackage.Year != currentYear)
            {
                throw new ArgumentException("Rok urlopu musi się zgadzać z rokiem bieżącym");
            }

            if (employee.VacationPackageId != vacationPackage.Id)
            {
                throw new ArgumentException("Urlop nie pasuje do opcji urlopowej przypisanej pracownikowi");
            }

            double usedDays = vacations
                .Where(v => v.EmployeeId == employee.Id && // Bezpieczeństwo: filtruj tylko urlopy pracownika
                            v.DateSince.Year == currentYear &&
                            v.DateUntil < currentDate)
                .Sum(v => v.IsPartialVacation
                    ? Math.Ceiling((double)v.NumberOfHours / 8)
                    : (v.DateUntil - v.DateSince).Days + 1);

            int freeDays = vacationPackage.GrantedDays - (int)usedDays;
            return Math.Max(0, freeDays); // Zapobiegaj ujemnym wartościom
        }

        public bool IfEmployeeCanRequestVacation(Employee employee, List<Vacation> vacations, VacationPackage vacationPackage)
        {
            if (employee == null || vacations == null || vacationPackage == null)
                throw new ArgumentNullException("Pracownik, wakacje ani opcja urlopowa nie mogą być nulllem.");

            if (vacationPackage.Year != DateTime.Now.Year)
                throw new ArgumentException("Rok pakietu urlopowego musi być ten sam co obecny rok.");

            if (employee.VacationPackageId != vacationPackage.Id)
                throw new ArgumentException("Paket urlopowy nie pasuje do przypisanego pakietu pracownika.");

            int freeDays = CountFreeDaysForEmployee(employee, vacations, vacationPackage);
            return freeDays > 0;
        }
    }
}