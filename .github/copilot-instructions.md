# Repository-Level Copilot Instructions

## Project Context & Architecture

### Technology Stack
- **Primary Language**: C# (.NET 8+)
- **Architecture Pattern**: Clean Architecture / Onion Architecture
- **Key Frameworks**: ASP.NET Core, Entity Framework Core
- **Design Principles**: SOLID, DRY, YAGNI

### Code Style & Conventions

#### Naming Conventions
- Use PascalCase for public members, classes, and methods
- Use camelCase for private fields with `_` prefix
- Async methods must have `Async` suffix
- Interface names must start with `I`

#### Language Features
- Prefer **file-scoped namespaces** for cleaner structure
- Use **primary constructors** where appropriate (C# 12+)
- Leverage **collection expressions** `[]` over `new List<T>()`
- Prefer **pattern matching** and `switch` expressions over conditional chains
- Use **nullable reference types** consistently - all nullability must be explicit

#### Async/Await Patterns
- Always use `ConfigureAwait(false)` in library code
- Prefer `ValueTask<T>` for frequently-called, often-synchronous methods
- Never use `.Result` or `.Wait()` - always await properly
- Use `IAsyncEnumerable<T>` for streaming scenarios

### Architecture Guidelines

#### Dependency Injection
- Constructor injection is the default pattern
- Avoid service locator anti-pattern
- Register services with appropriate lifetime (Transient, Scoped, Singleton)
- Use `IOptions<T>` pattern for configuration binding

#### Domain Logic
- Keep domain entities free of infrastructure concerns
- Use **Value Objects** for domain concepts without identity
- Implement **Domain Events** for cross-aggregate communication
- Validation belongs in domain layer, not controllers

#### Data Access
- Repository pattern for complex queries and abstraction
- Specification pattern for reusable query logic
- Always use parameterized queries - never string concatenation
- Leverage `AsNoTracking()` for read-only queries

#### Error Handling
- Use **Result pattern** over exceptions for expected failures
- Create custom exceptions for domain-specific errors
- Implement global exception handling middleware
- Log exceptions with structured logging (Serilog preferred)

### Testing Standards

- **Unit tests**: xUnit with FluentAssertions
- **Mocking**: NSubstitute or Moq
- **Test naming**: `MethodName_Scenario_ExpectedBehavior`
- Aim for >80% code coverage on business logic
- Use `Bogus` for test data generation

### Code Generation Preferences

#### When suggesting code:
1. Include appropriate null checks and validation
2. Add XML documentation for public APIs
3. Consider performance implications (allocations, boxing, collection sizes)
4. Include relevant using statements
5. Suggest corresponding unit tests when implementing new features

#### Prefer:
- LINQ method syntax over query syntax
- Expression-bodied members for simple implementations
- Records for immutable DTOs
- Minimal APIs for lightweight endpoints
- Source generators over reflection where applicable

### Common Patterns

**Repository Interface Example:**
```csharp
public interface IRepository<T> where T : class, IAggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
}
```

**Result Pattern:**
```csharp
public readonly record struct Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;
}
```

### Anti-Patterns to Avoid
- God objects / classes with too many responsibilities
- Anemic domain models (all logic in services)
- Static dependencies and singletons (except truly stateless)
- Primitive obsession - use Value Objects
- Repository methods returning `IQueryable<T>`

### Documentation
- Use triple-slash XML comments for all public APIs
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags
- Document complex algorithms with inline comments
- Keep README.md updated with architecture decisions