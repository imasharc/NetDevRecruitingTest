using Microsoft.EntityFrameworkCore;
using NetDevRecruitingTest.src.Data;
using NetDevRecruitingTest.src.Domain;
using NetDevRecruitingTest.src.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public void employee_can_request_vacation()
    {
        var jan = _context.Employees.First(e => e.Id == 1);
        var janVacations = _context.Vacations.Where(v => v.EmployeeId == 1).ToList();
        var package = _context.VacationPackages.First(p => p.Id == 1);

        var canRequest = _service.IfEmployeeCanRequestVacation(jan, janVacations, package);
        Assert.That(canRequest, Is.True);  // Jan ma 15 wolnych dni (20 - 5), więc może złożyć wniosek
    }

    [Test]
    public void employee_cant_request_vacation()
    {
        // Dodaj dodatkowy urlop dla Jana, by wykorzystać wszystkie 20 dni
        var extraVacation = new Vacation 
        { 
            Id = 4, 
            EmployeeId = 1, 
            DateSince = new DateTime(2025, 3, 1), 
            DateUntil = new DateTime(2025, 3, 15), 
            IsPartialVacation = false, 
            NumberOfHours = 0 
        }; // 15 dni, razem z 5 dniami = 20 dni
        _context.Vacations.Add(extraVacation);
        _context.SaveChanges();

        var jan = _context.Employees.First(e => e.Id == 1);
        var janVacations = _context.Vacations.Where(v => v.EmployeeId == 1).ToList();
        var package = _context.VacationPackages.First(p => p.Id == 1);

        var canRequest = _service.IfEmployeeCanRequestVacation(jan, janVacations, package);
        Assert.That(canRequest, Is.False);  // Jan wykorzystał 20 dni, więc nie może złożyć wniosku
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
        // Mock: Dodaj extra vacation dla Jana, by przekroczyć 20
        var extraVacation = new Vacation { Id = 5, EmployeeId = 1, DateSince = new DateTime(2025, 3, 1), DateUntil = new DateTime(2025, 3, 20), IsPartialVacation = false, NumberOfHours = 0 };  // 20 dni
        _context.Vacations.Add(extraVacation);
        _context.SaveChanges();

        var jan = _context.Employees.First(e => e.Id == 1);
        var janVacations = _context.Vacations.Where(v => v.EmployeeId == 1).ToList();
        var package = _context.VacationPackages.First(p => p.Id == 1);

        var canRequest = _service.IfEmployeeCanRequestVacation(jan, janVacations, package);
        Assert.That(canRequest, Is.False);  // 20 - (5 + 20) = 0 dni
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