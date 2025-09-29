# NetDevRecruitingTest

Rozwiązanie zadania rekrutacyjnego w .NET 8. Projekt skupia się na clean code, SOLID (SRP dla separacji logiki, DIP dla interfejsów), testach jednostkowych (NUnit) i obsłudze błędów (walidacje, exceptions). Struktura: src/Domain dla modeli, src/Services dla biznesu, Tests dla testów. Użyto BFS dla efektywności, cache dla O(1) lookup.

## Zadanie 1: Optymalizacja struktury hierarchii pracowników

### Opis rozwiązania
Zadanie wymaga prekomputacji relacji przełożony-podwładny na wszystkich poziomach (rzędy). Zaimplementowano w warstwie Services (EmployeeHierarchyService z interfejsem IEmployeeHierarchyService – DIP dla testowalności). Logika używa BFS do traversowania w górę hierarchii (efektywne O(n) w drzewie, detekcja cykli via HashSet). Klasa EmployeesStructure jest immutable dla bezpieczeństwa. Cache (nested Dictionary) optymalizuje GetSuperiorRowOfEmployee. Sample data w Program.cs demonstruje użycie.

Kod w:
- src/Domain/Employee.cs i EmployeesStructure.cs (modele, immutable z walidacją).
- src/Services/IEmployeeHierarchyService.cs i EmployeeHierarchyService.cs (logika BFS, cache).
- Tests/UnitTests/EmployeeHierarchyTests.cs (testy pokrywające relacje, rzędy, cykle).

### Zauważone błędy w zadaniu
- Nazewnictwo: `String` zamiast `string` w Employee.Name (konwencja C# preferuje lowercase, użycie do dyskusji).
- Niekonsekwencja: Klasa `EmployeesStructure` (mnoga), ale lista zwrotna `List<EmployeeStructure >` (pojedyncza, spacja po List).
- Parametr Fill: `List <Employees>` (spacja, mnoga zamiast pojedyncza `Employee`).
- Literówka: `GetSupieriorRowOfEmployee` zamiast `GetSuperiorRowOfEmployee`.
- Składnia: Brak zamknięcia > w List w niektórych miejscach.

### Poprawki względem oryginalnego zadania
- Ujednolicono na `EmployeesStructure` (mnoga) we wszystkich miejscach, w tym liście zwrotnej.
- Poprawiono parametr na `List<Employee>` (pojedyncza, bez spacji) – spójne z klasą.
- Zmiana nazwy metody na `GetSuperiorRowOfEmployee` – poprawna ortografia.
- Dodano walidację (ArgumentOutOfRange w ctor) i exceptions (cykle) – dla robustness.
- Zmiana Fill na `FillEmployeesStructures` (mnoga) dla spójności.
- Dodano interfejs (DIP) – umożliwia mock w testach.
- Cache optymalizuje lookup – oryginał sugeruje liniowe wyszukiwanie.

### Uruchomienie i testowanie
- **Aplikacja konsolowa (demo):** W VS: Set as Startup Project > F5. Z terminala: `dotnet run` w folderze NetDevRecruitingTest. Output: Relacje rzędów z sample data (1, null, 2).
- **Testy jednostkowe:** W VS: View > Test Explorer > Run All (3 testy powinny przejść). Z terminala: `dotnet test` w solution. Coverage: `dotnet test --collect:"XPlat Code Coverage"` (cel: >90%).
- Wymagania: .NET 8 SDK, NuGet: NUnit, NUnit3TestAdapter, coverlet.collector.

Rozwiązanie jest maintainable – łatwe do rozszerzenia na kolejne zadania.