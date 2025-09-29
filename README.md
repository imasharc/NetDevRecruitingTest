# NetDevRecruitingTest

Rozwiązanie zadania rekrutacyjnego w .NET 8. Projekt skupia się na clean code, SOLID (SRP dla separacji logiki, DIP dla interfejsów), testach jednostkowych (NUnit) i obsłudze błędów (walidacje, exceptions). Struktura: src/Domain dla modeli, src/Services dla biznesu, Tests dla testów. Użyto BFS dla efektywności, cache dla O(1) lookup.

## Zadanie 1: Optymalizacja struktury hierarchii pracowników

### Opis rozwiązania
Zadanie wymaga prekomputacji relacji przełożony-podwładny na wszystkich poziomach (rzędy). Zaimplementowano w warstwie Services (EmployeeHierarchyService z interfejsem IEmployeeHierarchyService – DIP dla testowalności). Logika używa BFS do traversowania w górę hierarchii (efektywne O(n) w drzewie, detekcja cykli via HashSet). Klasa EmployeesStructure jest immutable dla bezpieczeństwa. Cache (nested Dictionary) optymalizuje GetSuperiorRowOfEmployee. Sample data w Program.cs demonstruje użycie.

Kod w:
- src/Domain/Employee.cs i EmployeesStructure.cs (modele, immutable z walidacją).
- src/Services/IEmployeeHierarchyService.cs i EmployeeHierarchyService.cs (logika BFS, cache).
- Tests/UnitTests/EmployeeHierarchyTests.cs (testy pokrywające relacje, rzędy, cykle).

### Wybór algorytmu traversowania: BFS vs DFS vs Prosty while loop
W implementacji użyto BFS (z kolejką) do traversowania hierarchii w górę od każdego pracownika. Poniżej wyjaśnienie zalet i wad tego podejścia w porównaniu do alternatyw (DFS rekurencyjny lub iteracyjny ze stosem, oraz prosty while loop śledzący superiorów).

#### Zalety BFS:
- Naturalne obliczanie poziomów (rows): BFS przetwarza strukturę poziom po poziomie, co idealnie pasuje do inkrementalnego wyliczania rzędów (row = previous_row + 1). Zapewnia spójne i intuicyjne liczenie odległości w hierarchii.
- Iteracyjne podejście: Unika rekurencji, co jest bezpieczniejsze w głębokich hierarchiach (brak ryzyka StackOverflowException w .NET). Idealne dla dużych organizacji z długimi łańcuchami przełożonych.
- Detekcja cykli: Łatwa integracja z HashSet visited – jeśli węzeł jest odwiedzony ponownie, rzucamy wyjątek. BFS zapewnia, że cykle są wykrywane w trakcie przetwarzania poziomów.
- Elastyczność: Chociaż ta przykładowa hierarchia jest liniowa (każdy pracownik ma co najwyżej jednego superiora), BFS traktuje ją jak graf, co ułatwia przyszłe rozszerzenia (np. na wielopoziomowe struktury z branchami, jeśli model ewoluuje).
- Zgodność z SOLID: Promuje SRP (oddzielna logika traversowania) i jest łatwe do testowania (mock queue i visited).

#### Wady BFS:
- Overkill dla prostej struktury: W tej prostej, przykładowej hierarchii (drzewo skierowane w górę, bez branchy), BFS może być zbyt wyinżynierowany – używa kolejki, co dodaje overhead kodu i pamięci (O(n) dla queue w najgorszym przypadku). To jest temat dysusyjny - czy decydujemy się na prostote dla specyficznego przypadku ryzykując długiem technicznym czy od razu implementujemy porządny, bardziej zaawansowany kod.
- Większa złożoność kodu: Wymaga zarządzania kolejką i tuplami (currentId, row), co czyni metodę dłuższa i mniej intuicyjną w porównaniu do prostego loopa.
- Wydajność w liniowych ścieżkach: Dla czysto liniowych łańcuchów (np. A -> B -> C), BFS wykonuje te same operacje co prostszy algorytm, ale z dodatkowym kosztem enqueue/dequeue.

#### Porównanie z DFS:
- Zalety DFS (rekurencyjny lub ze stosem): Prostszy do implementacji dla ścieżek w dół/górę – rekurencja naturalnie śledzi głębokość (row). Mniej kodu: rekurencyjna funkcja z parametrem row. W iteracyjnej wersji (stos) podobna do BFS, ale eksploruje głębiej najpierw.
- Wady DFS: Ryzyko przepełnienia stosu w bardzo głębokich hierarchiach (np. 1000+ poziomów, choć rzadkie). Detekcja cykli wymaga dodatkowego trackingu (visited lub recursion depth limit). W rekurencji trudniej zarządzać stanem w .NET.
- Kiedy DFS byłby lepszy?: Jeśli hierarchia była głęboka, ale nie szeroka, i potrzebowalibyśmy post-order processing (tu niepotrzebne).

#### Porównanie z prostym while loop:
- Zalety while loop: Najprostszy i najbardziej efektywny dla tej struktury – start od employee, while (superior != null), increment row, add structure/cache, check visited for cycles. Brak queue/stosu, O(1) pamięci dodatkowej per traversal. Kod krótszy, łatwiejszy do zrozumienia i maintain.
- Wady while loop: Mniej ogólny – nie radzi sobie naturalnie z branchami lub grafami (tu niepotrzebne, ale ogranicza skalowalność). Detekcja cykli wymaga ręcznego trackingu (HashSet visited w loopie). Brak level-order guarantee, choć w liniowej ścieżce to nie problem.
- Kiedy while loop byłby lepszy?: Dla tej specyficznej prostej hierarchii (liniowa w górę) – redukuje boilerplate, adheres do KISS principle, i jest wystarczający bez utraty funkcjonalności.

Wybór BFS w tym rozwiązaniu: Wybrano BFS ze względu na dobre praktyki utrzymania kodu i potencjał dla spotykanych w życiu struktur (łatwe rozszerzenie na złożone grafy), przy zachowaniu O(n) czasu. W produkcji, dla milionów pracowników, kwestią dyskusyjną jest zmiana na while loop dla prostoty, tylko jeśli hierarchia pozostaje prosta i liniowa. Testy jednostkowe potwierdzają poprawność niezależnie od algorytmu.

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