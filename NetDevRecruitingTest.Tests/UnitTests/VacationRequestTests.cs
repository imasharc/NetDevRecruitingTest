using Microsoft.EntityFrameworkCore;
using NetDevRecruitingTest.src.Data;
using NetDevRecruitingTest.src.Domain;
using NetDevRecruitingTest.src.Services;

namespace NetDevRecruitingTest.Tests.UnitTests;

[TestFixture]
public class VacationRequestTests
{
    private AppDbContext _context;
    private IVacationService _service;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        SeedData(_context);
        _service = new VacationService(_context);
    }

    [Test]
    public void IfEmployeeCanRequestVacation_WithFreeDays_ReturnsTrue()
    {
        var jan = _context.Employees.First(e => e.Id == 1);
        var janVacations = _context.Vacations.Where(v => v.EmployeeId == 1).ToList();
        var package = _context.VacationPackages.First(p => p.Id == 1);

        var canRequest = _service.IfEmployeeCanRequestVacation(jan, janVacations, package);
        Assert.That(canRequest, Is.True);  // 15 wolnych dni > 0
    }

    [Test]
    public void IfEmployeeCanRequestVacation_NoFreeDays_ReturnsFalse()
    {
        // Mock: Dodaj extra vacation dla Jana, by wykorzystać wszystkie 20 dni
        var extraVacation = new Vacation { Id = 5, EmployeeId = 1, DateSince = new DateTime(2025, 3, 1), DateUntil = new DateTime(2025, 3, 20), IsPartialVacation = false, NumberOfHours = 0 };
        _context.Vacations.Add(extraVacation);
        _context.SaveChanges();

        var jan = _context.Employees.First(e => e.Id == 1);
        var janVacations = _context.Vacations.Where(v => v.EmployeeId == 1).ToList();
        var package = _context.VacationPackages.First(p => p.Id == 1);

        var canRequest = _service.IfEmployeeCanRequestVacation(jan, janVacations, package);
        Assert.That(canRequest, Is.False);  // 0 wolnych dni
    }

    [Test]
    public void IfEmployeeCanRequestVacation_NoVacations_ReturnsTrue()
    {
        var anna = _context.Employees.First(e => e.Id == 3);
        var annaVacations = _context.Vacations.Where(v => v.EmployeeId == 3).ToList();  // Pusta
        var package = _context.VacationPackages.First(p => p.Id == 1);

        var canRequest = _service.IfEmployeeCanRequestVacation(anna, annaVacations, package);
        Assert.That(canRequest, Is.True);  // 20 wolnych dni > 0
    }

    [Test]
    public void IfEmployeeCanRequestVacation_NullEmployee_ThrowsException()
    {
        var vacations = new List<Vacation>();
        var package = new VacationPackage { Id = 1, Name = "Standard", GrantedDays = 20, Year = 2025 };

        Assert.Throws<ArgumentNullException>(() => _service.IfEmployeeCanRequestVacation(null, vacations, package));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static void SeedData(AppDbContext context)
    {
        var netTeam = new Team { Id = 1, Name = ".NET" };
        var javaTeam = new Team { Id = 2, Name = "Java" };
        context.Teams.AddRange(netTeam, javaTeam);

        var package = new VacationPackage { Id = 1, Name = "Standard", GrantedDays = 20, Year = 2025 };

        var jan = new Employee { Id = 1, Name = "Jan Kowalski", TeamId = 1, VacationPackageId = 1, PositionId = 1 };
        var kamil = new Employee { Id = 2, Name = "Kamil Nowak", TeamId = 1, VacationPackageId = 1, PositionId = 1 };
        var anna = new Employee { Id = 3, Name = "Anna Mariacka", TeamId = 2, VacationPackageId = 1, PositionId = 1 };
        context.Employees.AddRange(jan, kamil, anna);

        var vacation2025Full = new Vacation { Id = 1, EmployeeId = 1, DateSince = new DateTime(2025, 1, 1), DateUntil = new DateTime(2025, 1, 5), IsPartialVacation = false, NumberOfHours = 0 };
        var vacation2025Partial = new Vacation { Id = 2, EmployeeId = 2, DateSince = new DateTime(2025, 2, 1), DateUntil = new DateTime(2025, 2, 1), IsPartialVacation = true, NumberOfHours = 4 };
        var vacationFuture = new Vacation { Id = 3, EmployeeId = 1, DateSince = new DateTime(2025, 10, 1), DateUntil = new DateTime(2025, 10, 5), IsPartialVacation = false, NumberOfHours = 0 };

        context.Vacations.AddRange(vacation2025Full, vacation2025Partial, vacationFuture);
        context.VacationPackages.Add(package);

        context.SaveChanges();
    }
}