# Contributing to Tafsilk Platform

First off, thank you for considering contributing to Tafsilk Platform! 🎉

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates. When creating a bug report, include:

- **Clear title and description**
- **Steps to reproduce**
- **Expected vs actual behavior**
- **Screenshots** (if applicable)
- **Environment details** (OS, .NET version, browser)

**Bug Report Template:**
```markdown
**Description:**
A clear description of the bug.

**Steps to Reproduce:**
1. Go to '...'
2. Click on '...'
3. Scroll down to '...'
4. See error

**Expected Behavior:**
What should happen.

**Actual Behavior:**
What actually happens.

**Screenshots:**
If applicable.

**Environment:**
- OS: [e.g., Windows 11]
- .NET Version: [e.g., 9.0]
- Browser: [e.g., Chrome 120]
```

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful**
- **List any alternatives** you've considered

### Pull Requests

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Make your changes**
4. **Write or update tests**
5. **Ensure all tests pass** (`dotnet test`)
6. **Commit your changes** (`git commit -m 'Add some AmazingFeature'`)
7. **Push to the branch** (`git push origin feature/AmazingFeature`)
8. **Open a Pull Request**

## Development Setup

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server) or LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Setup Steps

1. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR-USERNAME/Tafsilk.git
   cd Tafsilk
   ```

2. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/AhmedTyson/Tafsilk.git
   ```

3. **Install dependencies**
   ```bash
   cd TafsilkPlatform.Web
   dotnet restore
   ```

4. **Configure user secrets**
   ```bash
   dotnet user-secrets set "Jwt:Key" "YOUR_DEV_SECRET_KEY_HERE"
   dotnet user-secrets set "Google:client_id" "YOUR_GOOGLE_CLIENT_ID"
   dotnet user-secrets set "Google:client_secret" "YOUR_GOOGLE_CLIENT_SECRET"
   ```

5. **Update database**
   ```bash
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

## Coding Standards

### C# Style Guide

Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

```csharp
// ✅ Good
public class TailorService : ITailorService
{
    private readonly ITailorRepository _tailorRepository;
    private readonly ILogger<TailorService> _logger;

    public TailorService(
        ITailorRepository tailorRepository,
   ILogger<TailorService> logger)
    {
        _tailorRepository = tailorRepository;
 _logger = logger;
    }

    public async Task<TailorProfile?> GetTailorByIdAsync(Guid id)
    {
        try
        {
            return await _tailorRepository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
      _logger.LogError(ex, "Error retrieving tailor {TailorId}", id);
            throw;
   }
    }
}

// ❌ Bad
public class tailorservice
{
    private ITailorRepository repo;
    
 public tailorservice(ITailorRepository r)
    {
        repo = r;
    }

    public TailorProfile GetTailor(Guid id)
    {
    return repo.GetByIdAsync(id).Result; // Don't block async!
    }
}
```

### Key Principles

1. **Use meaningful names**
   ```csharp
   // ✅ Good
   var activeVerifiedTailors = await _tailorRepository.GetActiveVerifiedTailorsAsync();
   
   // ❌ Bad
   var list = await _tailorRepository.GetAsync();
   ```

2. **Keep methods small and focused**
   - One responsibility per method
   - Maximum ~50 lines per method
   - Extract complex logic into helper methods

3. **Use async/await properly**
   ```csharp
   // ✅ Good
   public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
   {
       var order = MapToOrder(dto);
   await _orderRepository.AddAsync(order);
       await _unitOfWork.SaveChangesAsync();
       return order;
   }
 
   // ❌ Bad
   public Order CreateOrder(CreateOrderDto dto)
   {
       return CreateOrderAsync(dto).Result; // Deadlock risk!
   }
   ```

4. **Handle exceptions appropriately**
   ```csharp
   // ✅ Good
   try
   {
       await _emailService.SendAsync(email);
   }
   catch (SmtpException ex)
   {
       _logger.LogError(ex, "Failed to send email to {Email}", email.To);
       // Don't rethrow if it's not critical
 }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Unexpected error sending email");
throw; // Rethrow unexpected exceptions
   }
   ```

5. **Use dependency injection**
   - Register services in `Program.cs`
   - Inject via constructor
   - Prefer interfaces over concrete types

6. **Follow SOLID principles**
   - Single Responsibility
   - Open/Closed
   - Liskov Substitution
   - Interface Segregation
   - Dependency Inversion

### Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Class | PascalCase | `TailorProfile` |
| Interface | IPascalCase | `ITailorRepository` |
| Method | PascalCase | `GetTailorByIdAsync` |
| Parameter | camelCase | `tailorId` |
| Private field | _camelCase | `_tailorRepository` |
| Constant | PascalCase | `MaxFileSize` |
| Async method | PascalCaseAsync | `SaveChangesAsync` |

### Project Structure

```
TafsilkPlatform.Web/
├── Controllers/      # HTTP controllers
├── Data/        # DbContext, migrations
├── Extensions/      # Extension methods
├── Interfaces/   # Service interfaces
├── Middleware/      # Custom middleware
├── Models/          # Domain models
├── Repositories/    # Data access
├── Security/      # Auth/authz
├── Services/        # Business logic
├── ViewModels/      # DTOs for views
├── Views/           # Razor views
└── wwwroot/         # Static files
```

### Database Migrations

**Creating a migration:**
```bash
dotnet ef migrations add AddTailorVerificationStatus --project TafsilkPlatform.Web
```

**Review the migration:**
- Check `Up()` and `Down()` methods
- Ensure data integrity
- Test rollback

**Apply migration:**
```bash
dotnet ef database update --project TafsilkPlatform.Web
```

### Testing

**Run tests:**
```bash
dotnet test
```

**Write unit tests for:**
- Services (business logic)
- Repositories (data access)
- Validators
- Extensions

**Example test:**
```csharp
[Fact]
public async Task GetTailorByIdAsync_WhenTailorExists_ReturnsTailor()
{
    // Arrange
    var tailorId = Guid.NewGuid();
    var expectedTailor = new TailorProfile { Id = tailorId };
    _mockRepository.Setup(x => x.GetByIdAsync(tailorId))
        .ReturnsAsync(expectedTailor);

    // Act
    var result = await _service.GetTailorByIdAsync(tailorId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(tailorId, result.Id);
}
```

## Git Workflow

### Branch Naming

- `feature/feature-name` - New features
- `bugfix/bug-description` - Bug fixes
- `hotfix/critical-fix` - Critical production fixes
- `refactor/refactor-description` - Code improvements
- `docs/documentation-update` - Documentation

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Formatting, missing semicolons, etc.
- `refactor`: Code restructuring
- `test`: Adding tests
- `chore`: Maintenance

**Examples:**
```
feat(orders): add order cancellation feature

Implement order cancellation with refund logic.
Add validation for cancellation timeframe.

Closes #123
```

```
fix(auth): resolve Google OAuth redirect issue

Fixed state parameter validation causing OAuth failures.

Fixes #456
```

### Pull Request Process

1. **Update documentation** if needed
2. **Ensure all tests pass**
3. **Update CHANGELOG.md** (for maintainers)
4. **Get code review** from at least one maintainer
5. **Squash commits** if requested
6. **Merge** after approval

**PR Template:**
```markdown
## Description
Brief description of changes.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review
- [ ] I have commented my code where needed
- [ ] I have updated the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix/feature works
- [ ] New and existing unit tests pass locally

## Related Issues
Closes #(issue number)

## Screenshots (if applicable)
```

## Community

- **Questions?** Open a [Discussion](https://github.com/AhmedTyson/Tafsilk/discussions)
- **Bug reports?** Open an [Issue](https://github.com/AhmedTyson/Tafsilk/issues)
- **Security?** See [SECURITY.md](SECURITY.md)

## License

By contributing, you agree that your contributions will be licensed under the project's license.

## Recognition

Contributors will be acknowledged in the project README.

---

Thank you for contributing to Tafsilk Platform! 🙏
