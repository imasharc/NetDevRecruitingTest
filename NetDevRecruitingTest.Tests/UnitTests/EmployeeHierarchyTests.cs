using NetDevRecruitingTest.Services;
using NetDevRecruitingTest.src.Domain;
using NetDevRecruitingTest.src.Services;

namespace NetDevRecruitingTest.Tests.UnitTests;

[TestFixture]
public class EmployeeHierarchyTests
{
    private IEmployeeHierarchyService _service;

    [SetUp]
    public void Setup()
    {
        _service = new EmployeeHierarchyService();
    }

    [Test]
    public void FillEmployeesStructures_TworzyPoprawneRelacje()
    {
        var employees = PobierzPrzykładowychPracowników();
        var structures = _service.FillEmployeesStructures(employees);
        Assert.That(structures.Count, Is.EqualTo(4));
    }

    [Test]
    public void GetSuperiorRowOfEmployee_ZwracaPoprawnyRząd()
    {
        var employees = PobierzPrzykładowychPracowników();
        _service.FillEmployeesStructures(employees);

        Assert.That(_service.GetSuperiorRowOfEmployee(2, 1), Is.EqualTo(1));
        Assert.That(_service.GetSuperiorRowOfEmployee(4, 1), Is.EqualTo(2));
        Assert.IsNull(_service.GetSuperiorRowOfEmployee(4, 3));
    }

    [Test]
    public void FillEmployeesStructures_RzucaWyjątekNaCykl()
    {
        var employees = new List<Employee>
        {
            new Employee { Id = 1, SuperiorId = 2 },
            new Employee { Id = 2, SuperiorId = 1 }
        };

        Assert.Throws<InvalidOperationException>(() => _service.FillEmployeesStructures(employees));
    }

    private static List<Employee> PobierzPrzykładowychPracowników()
    {
        return new List<Employee>
        {
            new Employee { Id = 1, Name = "Jan Kowalski" },
            new Employee { Id = 2, Name = "Kamil Nowak", SuperiorId = 1 },
            new Employee { Id = 3, Name = "Anna Mariacka", SuperiorId = 1 },
            new Employee { Id = 4, Name = "Andrzej Abacki", SuperiorId = 2 }
        };
    }
}