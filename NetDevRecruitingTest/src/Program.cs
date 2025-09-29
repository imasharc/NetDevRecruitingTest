using NetDevRecruitingTest.Services;
using NetDevRecruitingTest.src.Domain;
using System;
using System.Collections.Generic;

namespace NetDevRecruitingTest;

internal class Program
{
    static void Main(string[] args)
    {
        var service = new EmployeeHierarchyService();

        var employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "Jan Kowalski" },
            new Employee { Id = 2, Name = "Kamil Nowak", SuperiorId = 1 },
            new Employee { Id = 3, Name = "Anna Mariacka", SuperiorId = 1 },
            new Employee { Id = 4, Name = "Andrzej Abacki", SuperiorId = 2 }
        };

        service.FillEmployeesStructures(employees);

        Console.WriteLine(service.GetSuperiorRowOfEmployee(2, 1)); // 1
        Console.WriteLine(service.GetSuperiorRowOfEmployee(4, 3)); // null
        Console.WriteLine(service.GetSuperiorRowOfEmployee(4, 1)); // 2
    }
}