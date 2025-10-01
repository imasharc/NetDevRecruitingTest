# NetDevRecruitingTest

Rozwiązanie zadania rekrutacyjnego w .NET 8.
Projekt skupia się na:
- clean code,
- SOLID (SRP dla separacji logiki, DIP dla interfejsów),
- testach jednostkowych (NUnit)
- obsłudze błędów (walidacje, exceptions).

Struktura:
- src/Domain dla modeli,
- src/Services dla biznesu,
- Tests dla testów.

Użyto BFS dla efektywności, cache dla O(1) lookup.

## 📑 Spis treści

- [🏗️ Zadanie 1: Optymalizacja struktury hierarchii pracowników](#zadanie-1-optymalizacja-struktury-hierarchii-pracowników)
- [🗄️ Zadanie 2: Zapytania do bazy danych](#zadanie-2-zapytania-do-bazy-danych)
- [📅 Zadanie 3: Obliczanie pozostałych dni urlopowych](#zadanie-3-obliczanie-pozostałych-dni-urlopowych)
- [✅ Zadanie 4: Sprawdzanie możliwości zgłoszenia wniosku urlopowego](#zadanie-4-sprawdzanie-możliwości-zgłoszenia-wniosku-urlopowego)
- [🧪 Zadanie 5: Testy funkcjonalności wniosku urlopowego](#zadanie-5-testy-funkcjonalności-wniosku-urlopowego)
- [⚡ Zadanie 6: Optymalizacja liczby zapytań SQL](#zadanie-6-optymalizacja-liczby-zapytań-sql)

---

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

## Zadanie 2: Zapytania do bazy danych

### Opis rozwiązania
Zadanie wymaga 3 zapytań LINQ do schematu bazy (Employees, Teams, VacationPackages, Vacations). Zaimplementowano w VacationService (SRP: tylko logika zapytań, DIP via interfejs IVacationService). Użyto EF Core z fluent config dla relacji (unidirectional dla minimalizmu). Dla b: Liczenie dni – pełne: (Until - Since).Days +1, partial: Ceiling(Hours / 8), tylko zakończone (< Now, rok 2025). "W całości datą przeszłą" interpretowane jako DateUntil < Now (wyklucza dzisiejszy dzień, bo nie jest w pełni przeszły). Eager loading (Include) optymalizuje. Sample data w seed dla demo/testów.

Kod w:
- src/Domain/Team.cs, VacationPackage.cs, Vacation.cs (modele z relacjami).
- src/Data/AppDbContext.cs (DbContext z fluent API).
- src/Services/IVacationService.cs i VacationService.cs (LINQ queries).
- Tests/UnitTests/VacationTests.cs (testy z in-memory DB, edge cases: partial, future, 0 vacations).

### Zauważone błędy w zadaniu
- Hardcoded rok 2019 w a/c – brak parametru, ale zachowałem dla zgodności.
- Schemat ma PositionId – nieużywane, ale dodane dla kompletności.
- Partial vacations: Brak specyfikacji przelicznika godzin – założyłem 8h/dzień (konfigurowalny).
- "Dni zużyte" w b: Tylko zakończone (< Now), ignorując przyszłe/trwające; "w całości datą przeszłą" – interpretacja wyklucza dzisiejszy dzień.

### Poprawki względem oryginalnego zadania
- Ujednolicono nazwy: Vacation (singular) dla klasy, Vacations dla DbSet – konwencja EF.
- Dodano walidacje (ArgumentNull w serwisie) i exceptions – brak w oryginale.
- Unidirectional relations (brak WithMany prop dla VacationPackage/Superior) – minimalizm, unika niepotrzebnych kolekcji (SRP).
- Przelicznik partial: Math.Ceiling dla dni – domenowa decyzja, konfigurowalna.
- Filtr w b: .Where(x => x.UsedDays > 0) – zwraca tylko pracowników z zużytymi dniami przeszłymi, rozumianymi jako najpóźniej dzień wczorajszy
- Eager loading – optymalizacja, zapobiega N+1 queries.
- Testy/testowalność: In-memory DB dla izolacji, pokrycie edge (partial, 0 dni).

### Uruchomienie i testowanie Zadania 2
- **Demo:** W VS: F5 na NetDevRecruitingTest. Z terminala: cd NetDevRecruitingTest > dotnet run. Wyświetla wyniki a/b/c z seed data.
- **Testy:** W VS: Test Explorer > Run All. Z terminala: dotnet test. Coverage: dotnet test --collect:"XPlat Code Coverage".

## Zadanie 3: Obliczanie pozostałych dni urlopowych

### Opis rozwiązania
Zadanie wymaga uzupełnienia metody obliczającej liczbę pozostałych dni urlopowych dla pracownika w bieżącym roku, na podstawie puli z pakietu (GrantedDays) minus zużyte dni z zakończonych urlopów. Zaimplementowano w warstwie Services (VacationService, rozszerzając IVacationService – DIP dla wstrzykiwania zależności i testowalności). Logika używa LINQ do filtrowania i sumowania (efektywne O(n) dla listy vacations, czysta funkcja bez side-effects). Bieżąca data hardcoded jako 30 września 2025 r. (zgodnie z kontekstem), ale w produkcji rekomenduję wstrzykiwanie IDateTimeProvider (abstrakcja dla testów). Obliczenia dni: pełne urlopy – (DateUntil - DateSince).Days + 1; częściowe – Math.Ceiling(NumberOfHours / 8.0) z konfigurowalnym przelicznikiem (8h/dzień). Tylko zakończone urlopy (< bieżąca data, w bieżącym roku). Sample data w Program.cs i seed dla demo/testów.

Kod w:
- src/Domain/Employee.cs, Vacation.cs i VacationPackage.cs (modele z relacjami, bez zmian).
- src/Services/IVacationService.cs i VacationService.cs (metoda CountFreeDaysForEmployee z LINQ i walidacją).
- Tests/UnitTests/FreeDaysTests.cs (testy z in-memory DB, edge cases: pełne/częściowe, brak urlopów, przyszłe, przekroczenie puli).

### Zauważone błędy w zadaniu
- Brak specyfikacji przelicznika godzin dla częściowych urlopów (IsPartialVacation) – założyłem 8h/dzień, ale mogłoby być konfigurowalne (np. per firma).
- "Dni urlopowych ma do wykorzystania" – niejasne, czy uwzględniać urlopy trwające (DateSince < Now < DateUntil); interpretacja "w całości datą przeszłą" sugeruje tylko zakończone (< Now).
- Bieżący rok: Nie określony (założyłem z VacationPackage.Year, walidacja matches Now.Year).
- Brak obsługi ujemnych wyników (jeśli used > granted) – mogłoby prowadzić do błędów; brak walidacji wejścia (nulls, mismatched IDs).
- Schemat: PositionId nieużywane, ale obecne; Vacations mnoga w nazwie, ale singular w klasie.

### Poprawki względem oryginalnego zadania
- Dodano walidacje: ArgumentNullException dla nulls, ArgumentException dla mismatched year/package ID – brak w oryginale, dla robustness.
- Przelicznik partial: Math.Ceiling(Hours / 8.0) – domenowa decyzja, konfigurowalna stała.
- Filtr: Tylko zakończone (< Now) i bieżący rok (DateSince.Year == Now.Year) – spójne z zadaniem 2b, ignoruje przyszłe/trwające.
- Granica ustawiona na 0: Math.Max(0, granted - used) – zapobiega ujemnym, co mogłoby crashować UI.
- Testy: Oddzielny plik FreeDaysTests.cs – pokrycie edge, w tym overused = 0.

### Uruchomienie i testowanie Zadania 3
- **Demo**: W VS: F5 na NetDevRecruitingTest (rozszerzone o Zad.3). Z terminala: cd NetDevRecruitingTest > dotnet run. Wyświetla free days dla sample employees (Jan:15, Kamil:19, Anna:20) z seed data.
- **Testy**: W VS: Test Explorer > Run All (4 testy powinny przejść). Z terminala: dotnet test. Coverage: dotnet test --collect:"XPlat Code Coverage" (cel: >90%).

## Zadanie 4: Sprawdzanie możliwości zgłoszenia wniosku urlopowego

### Opis rozwiązania
Zadanie wymaga zaimplementowania metody `IfEmployeeCanRequestVacation` w klasie `VacationService`, która sprawdza, czy pracownik może złożyć wniosek urlopowy, opierając się na dostępności wolnych dni urlopowych. Rozwiązanie wykorzystuje logikę z zadania 3 (`CountFreeDaysForEmployee`) dla ponownego użycia kodu (DRY), co zapewnia spójność obliczeń. Metoda waliduje wejście (null, zgodność roku i pakietu), filtruje zakończone urlopy w bieżącym roku (30 września 2025 r.) i zwraca `true`, jeśli wolne dni przekraczają 0, w przeciwnym razie `false`. Zgodność z SOLID: SRP (tylko decyzja), OCP (gotowa na rozszerzenie kryteriów). Demo w Program.cs i testy weryfikują różne przypadki.

Kod w:
- src/Services/IVacationService.cs i VacationService.cs (metoda z LINQ i walidacją).
- Tests/UnitTests/VacationRequestTests.cs (testy z in-memory DB, edge cases: wolne dni, brak dni, null).

### Zauważone błędy w zadaniu
- Brak specyfikacji minimalnej liczby dni potrzebnych do wniosku (założono ≥1).
- Niejasne, czy uwzględniać urlopy trwające (DateSince < Now < DateUntil) – interpretacja "zakończone" wyklucza takie przypadki.
- Brak definicji bieżącej daty – przyjęto 30 września 2025 r. z kontekstu.
- Brak obsługi błędów wejścia (null, niezgodność danych) – potencjalne wyjątki runtime.

### Poprawki względem oryginalnego zadania
- Dodano walidacje: ArgumentNullException dla null, ArgumentException dla mismatched year/package ID – zwiększa robustness.
- Reuse CountFreeDaysForEmployee – eliminuje duplikację, spójne z zadaniem 3.
- Filtr: Tylko zakończone urlopy (< Now) i bieżący rok – zgodne z logiką zadania 2b/3.
- Czysta funkcja: Bez stanu, gotowa na DI dla daty (np. IDateTimeProvider).
- Testy: Pokrycie scenariuszy (wolne dni, brak dni, null, przyszłe) – brak w oryginale.

### Uruchomienie i testowanie Zadania 4
- **Demo:** W VS: F5 na NetDevRecruitingTest (rozszerzone o Zad.4). Z terminala: cd NetDevRecruitingTest > dotnet run. Wyświetla wyniki z sample data (np. Jan: true, po wykorzystaniu 20 dni: false).
- **Testy:** W VS: Test Explorer > Run All (4 testy powinny przejść). Z terminala: dotnet test. Coverage: dotnet test --collect:"XPlat Code Coverage" (cel: >90%).

## Zadanie 5: Testy funkcjonalności wniosku urlopowego

### Opis rozwiązania
Zadanie wymaga zaimplementowania testów jednostkowych weryfikujących działanie metody `IfEmployeeCanRequestVacation` z zadania 4. Testy zostały dodane w pliku `VacationRequestTests.cs` i wykorzystują in-memory bazę danych do izolacji. Dwa proste testy sprawdzają przypadki: gdy pracownik może złożyć wniosek (wolne dni > 0) oraz gdy nie może (brak wolnych dni). Logika opiera się na istniejącym seed data, z dodatkowym mockiem urlopu dla scenariusza negatywnego. Podejście zapewnia zgodność z TDD (Test-Driven Development) i wysoką testowalność, zgodnie z zasadami SOLID (SRP dla izolacji testów).

Kod w:
- Tests/UnitTests/VacationRequestTests.cs (dwa nowe testy: employee_can_request_vacation, employee_cant_request_vacation).

### Zauważone błędy w zadaniu
- Brak specyfikacji szczegółowych kryteriów testów (np. minimalna liczba dni, edge cases jak null).

### Poprawki względem oryginalnego zadania
- Dodano dwa testy pokrywające podstawowe scenariusze (wolne dni i brak dni) – brak w oryginale.
- Użyto mocków (dodatkowy urlop) dla pełnego pokrycia – zwiększa robustność.
- Zachowano istniejącą strukturę testów (SetUp, TearDown) – spójność z zadaniami 2-4.
- Testy są proste i skupione na funkcjonalności, bez zbędnej złożoności.

### Uruchomienie i testowanie Zadania 5
- **Testy:** W VS: Test Explorer > Run All (2 nowe testy powinny przejść). Z terminala: cd NetDevRecruitingTest.Tests > dotnet test. Coverage: dotnet test --collect:"XPlat Code Coverage" (cel: >90%).
- Wymagania: .NET 8 SDK, NuGet: NUnit, NUnit3TestAdapter, coverlet.collector.

## Zadanie 6: Optymalizacja liczby zapytań SQL

### Opis rozwiązania
Zadanie wymaga analizy i opisania sposobów optymalizacji liczby zapytań SQL w scenariuszu, gdzie parametry metody `CountFreeDaysForEmployee` z zadania 3 (np. `Employee`, `List<Vacation>`, `VacationPackage`) są pobierane bezpośrednio z bazy danych. Rozwiązanie koncentruje się na zminimalizowaniu problemu N+1 i poprawie wydajności dla schematu bazy danych z relacjami (Employees, Vacations, VacationPackages, Teams). Zaproponowano sześć metod optymalizacji, z uwzględnieniem kontekstu hierarchii pracowników i urlopów. Rekomendacja obejmuje kombinację Eager Loading i widoku SQL dla najlepszego balansu między prostotą a skalowalnością. Analiza obejmuje zalety, wady i przykłady implementacji, zgodne z best practices .NET Core i EF Core.

Kod w:
- Brak nowego kodu (zadanie opisowe), ale dotyczy src/Services/VacationService.cs (metoda CountFreeDaysForEmployee z potencjalną optymalizacją).

### Zauważone błędy w zadaniu
- Brak specyfikacji wielkości datasetu – optymalizacja zależy od liczby pracowników/urlopów.
- Niejasne, czy uwzględniać przyszłe urlopy w agregacji – interpretacja zgodna z zadaniem 3 (ignorowane).
- Brak wskazania, czy baza jest relacyjna (zakładam SQL Server z EF Core).
- Nie określono, czy hierarchia (z zadania 1) wpływa na zapytania – skupiono się na urlopach.

### Poprawki względem oryginalnego zadania
- Dodano szczegółowy opis sześciu metod optymalizacji – brak w oryginale, zwiększa zrozumienie.
- Uwzględniono kontekst hierarchii (choć nieistotny dla urlopów) dla spójności z zadaniami 1-5.
- Przykłady SQL (widoki, stored procedures) są kompletne i gotowe do użycia.
- Analiza zalet/wad każdej metody – ułatwia wybór w zależności od scenariusza.

### Sposoby optymalizacji liczby zapytań SQL
1. **Eager Loading (Załaduj wszystko naraz)**
   - **Opis**: Użycie `Include` w EF Core do załadowania powiązanych encji (np. `Vacations`, `VacationPackage`) w jednym zapytaniu z JOIN. Przykład: `context.Employees.Include(e => e.Vacations).Include(e => e.VacationPackage).First(e => e.Id == employeeId)`.
   - **Zaleta**: Eliminuje N+1 (jedno zapytanie).
   - **Wada**: Większe zużycie pamięci przy dużych relacjach.

2. **Lazy Loading (Opóźnione ładowanie z kontrolą)**
   - **Opis**: Explicit ładowanie z `Load` (np. `context.Entry(employee).Collection(e => e.Vacations).Load()`) dla kontrolowanego dostępu.
   - **Zaleta**: Ładuje dane na żądanie, oszczędza zasoby.
   - **Wada**: Ryzyko N+1 bez optymalizacji.

3. **Projektowanie widoku (View) lub materializowanej tabeli**
   - **Opis**: Tworzenie widoku SQL agregującego dane, np. `CREATE VIEW vw_EmployeeVacationSummary AS SELECT e.Id, vp.GrantedDays - COALESCE(SUM(CASE WHEN v.IsPartialVacation THEN CEILING(v.NumberOfHours / 8.0) ELSE DATEDIFF(day, v.DateSince, v.DateUntil) + 1 END), 0) as FreeDays FROM Employees e JOIN VacationPackages vp ON e.VacationPackageId = vp.Id LEFT JOIN Vacations v ON e.Id = v.EmployeeId AND v.DateUntil < GETDATE() GROUP BY e.Id, vp.GrantedDays`.
   - **Zaleta**: Przesuwa obliczenia na serwer, szybkie zapytania.
   - **Wada**: Wymaga utrzymania (aktualizacja, indeksy).

4. **Użycie Stored Procedures**
   - **Opis**: Jedna procedura składowana, np. `CREATE PROCEDURE sp_GetEmployeeFreeDays @EmployeeId INT AS SELECT e.Id, vp.GrantedDays - COALESCE(SUM(CASE WHEN v.IsPartialVacation THEN CEILING(v.NumberOfHours / 8.0) ELSE DATEDIFF(day, v.DateSince, v.DateUntil) + 1 END), 0) as FreeDays FROM Employees e JOIN VacationPackages vp ON e.VacationPackageId = vp.Id LEFT JOIN Vacations v ON e.Id = v.EmployeeId AND v.DateUntil < GETDATE() WHERE e.Id = @EmployeeId GROUP BY e.Id, vp.GrantedDays`.
   - **Zaleta**: Optymalizacja na serwerze, pojedyncze wywołanie.
   - **Wada**: Mniejsza elastyczność.

5. **Batching i Paging**
   - **Opis**: Pobieranie danych partiami, np. `context.Employees.Where(e => e.TeamId == teamId).Skip(0).Take(100).Include(e => e.Vacations).ToList()`.
   - **Zaleta**: Redukuje pamięć i czas dla dużych datasetów.
   - **Wada**: Wymaga logiki pobierania kolejnych partii.

6. **Caching wyników**
   - **Opis**: Użycie MemoryCache, np. `memoryCache.GetOrCreate("EmployeeFreeDays_" + employeeId, entry => context.Employees.Include(e => e.Vacations).First(e => e.Id == employeeId))`.
   - **Zaleta**: Zmniejsza zapytania przy powtarzalnych wywołaniach.
   - **Wada**: Ryzyko nieaktualnych danych.

### Rekomendacja
Najlepsze podejście to kombinacja **Eager Loading** (proste wdrożenie w EF Core) i **widoku SQL** (agregacja na serwerze). Eager Loading eliminuje N+1, a widok optymalizuje obliczenia dla skalowalności. Kombinacja eager loading i widoku SQL jest jednym z najlepszych podejść w kontekście optymalizacji zapytań EF Core, szczególnie dla scenariuszy z relacjami i agregacjami jak w naszym przypadku (obliczanie wolnych dni urlopowych na podstawie pracowników, urlopów i pakietów). Jednak "najlepsze" zależy od specyfiki systemu – skali danych, częstotliwości zapytań, złożoności obliczeń i wymagań utrzymaniowych.

### Uruchomienie i testowanie Zadania 6
- **Analiza:** Zadanie opisowe, nie wymaga kodu. Testy istniejących metod (np. VacationService) z optymalizacjami można uruchomić w VS (Test Explorer > Run All) lub z terminala: cd NetDevRecruitingTest.Tests > dotnet test. Coverage: dotnet test --collect:"XPlat Code Coverage" (cel: >90%).
- Wymagania: .NET 8 SDK, EF Core, SQL Server (dla widoku/procedur).