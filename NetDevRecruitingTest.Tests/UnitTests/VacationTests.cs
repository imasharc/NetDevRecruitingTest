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
public class VacationTests
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
    public void GetDotNetEmployeesWithVacationsIn2019_ReturnsCorrectEmployees()
    {
        var result = _service.GetDotNetEmployeesWithVacationsIn2019();
        Assert.That(result.Count(), Is.EqualTo(1));  // Tylko Jan z .NET i urlopem w 2019
        Assert.That(result.First().Name, Is.EqualTo("Jan Kowalski"));
    }

    [Test]
    public void GetEmployeesWithUsedDaysInCurrentYear_ReturnsCorrectUsedDays()
    {
        var result = _service.GetEmployeesWithUsedDaysInCurrentYear().ToList();
        Assert.That(result.Count, Is.EqualTo(2));  // Dwóch pracowników z urlopami w 2025
        Assert.That(result[0].UsedDays, Is.EqualTo(5));  // Jan: Pełny urlop 5 dni (zakończony)
        Assert.That(result[1].UsedDays, Is.EqualTo(1));  // Kamil: Partial 4h = 1 dzień (Ceiling)
    }

    [Test]
    public void GetTeamsWithoutVacationsIn2019_ReturnsTeamsWithoutVacations()
    {
        var result = _service.GetTeamsWithoutVacationsIn2019();
        Assert.That(result.Count(), Is.EqualTo(1));  // Tylko Java bez urlopów w 2019
        Assert.That(result.First().Name, Is.EqualTo("Java"));
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static void SeedData(AppDbContext context)
    {
        // Sample data dla testów: Zespoły, Pracownicy, Pakiety, Urlopy (2019/2025, partial/full)
        var netTeam = new Team { Id = 1, Name = ".NET" };
        var javaTeam = new Team { Id = 2, Name = "Java" };
        context.Teams.AddRange(netTeam, javaTeam);

        var package = new VacationPackage { Id = 1, Name = "Standard", GrantedDays = 20, Year = 2025 };

        var jan = new Employee { Id = 1, Name = "Jan Kowalski", TeamId = 1, VacationPackageId = 1, PositionId = 1 };
        var kamil = new Employee { Id = 2, Name = "Kamil Nowak", TeamId = 1, VacationPackageId = 1, PositionId = 1 };
        var anna = new Employee { Id = 3, Name = "Anna Mariacka", TeamId = 2, VacationPackageId = 1, PositionId = 1 };
        context.Employees.AddRange(jan, kamil, anna);

        // Urlopy: 2019 (dla a i c), 2025 bieżący (dla b, zakończone < 29.09.2025)
        var vacation2019 = new Vacation { Id = 1, EmployeeId = 1, DateSince = new DateTime(2019, 1, 1), DateUntil = new DateTime(2019, 1, 5), IsPartialVacation = false, NumberOfHours = 0 };
        var vacation2025Full = new Vacation { Id = 2, EmployeeId = 1, DateSince = new DateTime(2025, 1, 1), DateUntil = new DateTime(2025, 1, 5), IsPartialVacation = false, NumberOfHours = 0 };  // 5 dni, zakończony
        var vacation2025Partial = new Vacation { Id = 3, EmployeeId = 2, DateSince = new DateTime(2025, 2, 1), DateUntil = new DateTime(2025, 2, 1), IsPartialVacation = true, NumberOfHours = 4 };  // 4h = 1 dzień, zakończony
        var vacationFuture = new Vacation { Id = 4, EmployeeId = 1, DateSince = new DateTime(2025, 10, 1), DateUntil = new DateTime(2025, 10, 5), IsPartialVacation = false, NumberOfHours = 0 };  // Przyszły, ignorowany w b

        context.Vacations.AddRange(vacation2019, vacation2025Full, vacation2025Partial, vacationFuture);
        context.VacationPackages.Add(package);

        context.SaveChanges();
    }
}