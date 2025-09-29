using Microsoft.EntityFrameworkCore;
using NetDevRecruitingTest.Services;
using NetDevRecruitingTest.src.Data;
using NetDevRecruitingTest.src.Domain;
using NetDevRecruitingTest.src.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDevRecruitingTest;

internal class Program
{
    static void Main(string[] args)
    {
        // Demo Zad.1 (poprzednie)
        var hierarchyService = new EmployeeHierarchyService();

        var employeesSample = new List<Employee>
        {
            new Employee { Id = 1, Name = "Jan Kowalski" },
            new Employee { Id = 2, Name = "Kamil Nowak", SuperiorId = 1 },
            new Employee { Id = 3, Name = "Anna Mariacka", SuperiorId = 1 },
            new Employee { Id = 4, Name = "Andrzej Abacki", SuperiorId = 2 }
        };

        hierarchyService.FillEmployeesStructures(employeesSample);

        Console.WriteLine("Zad.1 przykłady:");
        Console.WriteLine(hierarchyService.GetSuperiorRowOfEmployee(2, 1)); // 1
        Console.WriteLine(hierarchyService.GetSuperiorRowOfEmployee(4, 3)); // null
        Console.WriteLine(hierarchyService.GetSuperiorRowOfEmployee(4, 1)); // 2

        // Demo Zad.2
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        using var context = new AppDbContext(options);
        SeedData(context);

        var vacationService = new VacationService(context);

        Console.WriteLine("\nZad.2a: .NET employees with 2019 vacations");
        foreach (var emp in vacationService.GetDotNetEmployeesWithVacationsIn2019())
            Console.WriteLine(emp.Name);  // Jan Kowalski

        Console.WriteLine("\nZad.2b: Employees with used days in current year");
        foreach (var (emp, usedDays) in vacationService.GetEmployeesWithUsedDaysInCurrentYear())
            Console.WriteLine($"{emp.Name}: {usedDays} dni");  // Jan: 5, Kamil: 1

        Console.WriteLine("\nZad.2c: Teams without vacations in 2019");
        foreach (var team in vacationService.GetTeamsWithoutVacationsIn2019())
            Console.WriteLine(team.Name);  // Java
    }

    private static void SeedData(AppDbContext context)
    {
        // Sample data: Teams, Employees, Packages, Vacations (dostosowane do a/b/c)
        var netTeam = new Team { Id = 1, Name = ".NET" };
        var javaTeam = new Team { Id = 2, Name = "Java" };
        context.Teams.AddRange(netTeam, javaTeam);

        var package = new VacationPackage { Id = 1, Name = "Standard", GrantedDays = 20, Year = 2025 };

        var jan = new Employee { Id = 1, Name = "Jan Kowalski", TeamId = 1, VacationPackageId = 1, PositionId = 1 };
        var kamil = new Employee { Id = 2, Name = "Kamil Nowak", TeamId = 1, VacationPackageId = 1, PositionId = 1 };
        var anna = new Employee { Id = 3, Name = "Anna Mariacka", TeamId = 2, VacationPackageId = 1, PositionId = 1 };
        context.Employees.AddRange(jan, kamil, anna);

        // Urlopy: Dla 2019 (a,c), 2025 (b, zakończone/przyszłe, partial/full)
        var vacation2019 = new Vacation { Id = 1, EmployeeId = 1, DateSince = new DateTime(2019, 1, 1), DateUntil = new DateTime(2019, 1, 5), IsPartialVacation = false, NumberOfHours = 0 };
        var vacation2025Full = new Vacation { Id = 2, EmployeeId = 1, DateSince = new DateTime(2025, 1, 1), DateUntil = new DateTime(2025, 1, 5), IsPartialVacation = false, NumberOfHours = 0 };  // Zakończony, 5 dni
        var vacation2025Partial = new Vacation { Id = 3, EmployeeId = 2, DateSince = new DateTime(2025, 2, 1), DateUntil = new DateTime(2025, 2, 1), IsPartialVacation = true, NumberOfHours = 4 };  // Zakończony, 4h = 1 dzień
        var vacationFuture = new Vacation { Id = 4, EmployeeId = 1, DateSince = new DateTime(2025, 10, 1), DateUntil = new DateTime(2025, 10, 5), IsPartialVacation = false, NumberOfHours = 0 };  // Przyszły, ignorowany

        context.Vacations.AddRange(vacation2019, vacation2025Full, vacation2025Partial, vacationFuture);
        context.VacationPackages.Add(package);

        context.SaveChanges();
    }
}