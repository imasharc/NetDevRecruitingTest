using NetDevRecruitingTest.src.Domain;
using NetDevRecruitingTest.src.Services;

namespace NetDevRecruitingTest.Services;

public class EmployeeHierarchyService : IEmployeeHierarchyService
{
    private readonly Dictionary<int, Dictionary<int, int>> _hierarchyCache = new();

    public List<EmployeesStructure> FillEmployeesStructures(List<Employee> employees)
    {
        ArgumentNullException.ThrowIfNull(employees);

        var employeeMap = employees.ToDictionary(e => e.Id);
        var structures = new List<EmployeesStructure>();

        foreach (var employee in employees)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<(int currentId, int row)>();
            queue.Enqueue((employee.Id, 0));

            while (queue.Count > 0)
            {
                var (currentId, row) = queue.Dequeue();

                if (visited.Contains(currentId))
                    throw new InvalidOperationException("Wykryto cykl w hierarchii.");

                visited.Add(currentId);

                if (!employeeMap.TryGetValue(currentId, out var currentEmployee) ||
                    !currentEmployee.SuperiorId.HasValue)
                    continue;

                var superiorId = currentEmployee.SuperiorId.Value;
                var newRow = row + 1;

                structures.Add(new EmployeesStructure(employee.Id, superiorId, newRow));

                if (!_hierarchyCache.TryGetValue(employee.Id, out var cacheEntry) || cacheEntry == null)
                    _hierarchyCache[employee.Id] = new Dictionary<int, int>();

                _hierarchyCache[employee.Id][superiorId] = newRow;

                queue.Enqueue((superiorId, newRow));
            }
        }

        return structures;
    }

    public int? GetSuperiorRowOfEmployee(int employeeId, int superiorId)
    {
        if (_hierarchyCache.TryGetValue(employeeId, out var superiors) &&
            superiors.TryGetValue(superiorId, out var row))
            return row;

        return null;
    }
}