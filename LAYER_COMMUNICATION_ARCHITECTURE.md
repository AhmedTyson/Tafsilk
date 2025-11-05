# 🏗️ TAFSILK PLATFORM - LAYER COMMUNICATION ARCHITECTURE

## **📊 Complete System Architecture**

This document provides a comprehensive view of the Tafsilk Platform's layered architecture, showing how different components communicate with each other.

---

## **🎯 HIGH-LEVEL ARCHITECTURE OVERVIEW**

```mermaid
graph TB
    subgraph "Client Layer"
        WEB[Web Browser]
        MOBILE[Mobile App]
        SPA[SPA/React]
    end

    subgraph "Presentation Layer - ASP.NET Core MVC"
        CTRL[Controllers]
    VIEWS[Razor Views]
        PAGES[Razor Pages]
        API[API Controllers]
    end

    subgraph "Business Logic Layer"
        SERVICES[Services]
        VALIDATORS[Validators]
        AUTH[Auth Service]
        PROFILE[Profile Service]
      ORDER[Order Service]
 end

    subgraph "Data Access Layer"
        REPOS[Repositories]
   UOW[Unit of Work]
        DBCTX[DbContext]
    end

    subgraph "Infrastructure"
        EMAIL[Email Service]
        FILE[File Upload Service]
        JWT[JWT Token Service]
        DATETIME[DateTime Service]
    end

    subgraph "Database"
        SQL[(SQL Server<br/>LocalDB)]
    end

    WEB --> CTRL
    WEB --> PAGES
    MOBILE --> API
    SPA --> API
    
    CTRL --> SERVICES
    PAGES --> SERVICES
    API --> SERVICES
CTRL --> VIEWS
    
    SERVICES --> REPOS
    SERVICES --> AUTH
    SERVICES --> VALIDATORS
    SERVICES --> EMAIL
    SERVICES --> FILE
    
    AUTH --> JWT
    AUTH --> REPOS
    
    REPOS --> UOW
    UOW --> DBCTX
    DBCTX --> SQL
    
    SERVICES --> DATETIME

    style WEB fill:#e1f5ff
    style MOBILE fill:#e1f5ff
    style SPA fill:#e1f5ff
    style CTRL fill:#fff9e1
    style API fill:#fff9e1
    style SERVICES fill:#e8f5e9
    style REPOS fill:#f3e5f5
    style SQL fill:#ffebee
```

---

## **🔄 DETAILED LAYER COMMUNICATION**

### **1. Presentation → Business Logic → Data Access**

```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant Service
    participant Repository
    participant DbContext
    participant Database

    Client->>Controller: HTTP Request
    activate Controller
    
    Controller->>Service: Call Business Logic
    activate Service
    
    Service->>Repository: Request Data
    activate Repository
    
    Repository->>DbContext: Query Entity
    activate DbContext
    
    DbContext->>Database: Execute SQL
    activate Database
    Database-->>DbContext: Return Data
    deactivate Database
    
    DbContext-->>Repository: Entity Objects
    deactivate DbContext
    
    Repository-->>Service: Domain Models
    deactivate Repository
    
    Service-->>Controller: DTOs/ViewModels
deactivate Service
    
    Controller-->>Client: HTTP Response
    deactivate Controller
```

---

## **🎭 AUTHENTICATION FLOW**

```mermaid
flowchart TD
    START([User Login Request])
    
    subgraph "Presentation Layer"
     A[AccountController<br/>Login Action]
    end
 
    subgraph "Business Logic"
        B[AuthService<br/>ValidateUserAsync]
        C[PasswordHasher<br/>Verify Password]
    D[TokenService<br/>Generate JWT]
    end
    
    subgraph "Data Access"
 E[UserRepository<br/>GetByEmailAsync]
        F[Unit of Work]
        G[AppDbContext]
    end
    
    subgraph "Database"
        H[(Users Table)]
    end
    
    subgraph "Response"
        I{Authentication<br/>Successful?}
      J[Create Claims]
        K[Sign In User]
        L[Return JWT Token]
        M[Redirect to Dashboard]
        N[Return Error]
    end
    
    START --> A
    A --> B
    B --> E
  E --> F
    F --> G
    G --> H
    H --> G
    G --> F
    F --> E
    E --> B
    B --> C
    C --> B
    B --> I
    
    I -->|Yes| J
I -->|No| N
    
    J --> K
    K --> D
    D --> L
    L --> M
    
    N --> END([Return to Login])
    M --> END2([User Dashboard])

    style A fill:#fff9e1
    style B fill:#e8f5e9
    style C fill:#e8f5e9
    style D fill:#e8f5e9
 style E fill:#f3e5f5
    style G fill:#f3e5f5
    style H fill:#ffebee
  style I fill:#ffe0b2
    style M fill:#c8e6c9
    style N fill:#ffcdd2
```

---

## **🛒 ORDER PROCESSING FLOW**

```mermaid
flowchart LR
 subgraph "Client"
        A[Customer<br/>Creates Order]
    end
    
    subgraph "Controllers"
        B[OrdersController<br/>Create Action]
    end
    
    subgraph "Services"
        C[Order Service<br/>Validate & Process]
        D[Notification Service<br/>Send Alerts]
        E[Email Service<br/>Send Confirmation]
    end
 
    subgraph "Repositories"
  F[OrderRepository<br/>Save Order]
        G[PaymentRepository<br/>Record Payment]
        H[NotificationRepository<br/>Store Notification]
    end
    
    subgraph "Unit of Work"
    I[Transaction<br/>Begin/Commit]
    end

    subgraph "Database"
        J[(Orders)]
        K[(Payments)]
        L[(Notifications)]
end
    
    A --> B
    B --> C
  C --> F
    C --> D
  C --> E
    
    F --> I
    G --> I
    H --> I
    
    I --> J
    I --> K
    I --> L
    
    D --> H
    
    J --> M{Success?}
    M -->|Yes| N[Return Success<br/>Order Confirmed]
    M -->|No| O[Rollback<br/>Return Error]
    
    N --> P[Redirect to<br/>Order Details]
    O --> Q[Show Error<br/>Message]

    style A fill:#e1f5ff
    style B fill:#fff9e1
    style C fill:#e8f5e9
    style D fill:#e8f5e9
    style E fill:#e8f5e9
    style F fill:#f3e5f5
    style I fill:#f3e5f5
    style J fill:#ffebee
    style M fill:#ffe0b2
    style N fill:#c8e6c9
    style O fill:#ffcdd2
```

---

## **📱 API AUTHENTICATION (JWT) FLOW**

```mermaid
sequenceDiagram
    participant Mobile as Mobile App
    participant API as ApiAuthController
    participant Auth as AuthService
    participant Token as TokenService
    participant Repo as UserRepository
    participant DB as Database

    Mobile->>API: POST /api/auth/login
  Note over Mobile,API: { email, password }
    
    activate API
 API->>Auth: ValidateUserAsync(email, password)
    activate Auth
    
    Auth->>Repo: GetByEmailAsync(email)
    activate Repo
    Repo->>DB: SELECT * FROM Users
    DB-->>Repo: User Data
    deactivate Repo
  
    Auth->>Auth: PasswordHasher.Verify()
    
    alt Valid Credentials
        Auth-->>API: (true, null, User)
        API->>Token: GenerateJwtToken(User)
        activate Token
        Token->>Token: Create Claims
        Token->>Token: Sign Token
        Token-->>API: JWT Token
        deactivate Token
  API-->>Mobile: 200 OK + JWT
    else Invalid Credentials
  Auth-->>API: (false, "INVALID_CREDENTIALS", null)
        API-->>Mobile: 401 Unauthorized
    end
    
    deactivate Auth
    deactivate API
    
    Note over Mobile: Store JWT Token
    
    Mobile->>API: GET /api/auth/me
    Note over Mobile,API: Header: Authorization Bearer {token}
    
    activate API
    API->>API: Validate JWT Token
    alt Token Valid
  API->>Repo: GetByIdAsync(userId)
        Repo->>DB: SELECT * FROM Users
        DB-->>Repo: User Data
        Repo-->>API: User Profile
        API-->>Mobile: 200 OK + User Data
    else Token Invalid/Expired
        API-->>Mobile: 401 Unauthorized
    end
    deactivate API
```

---

## **🗂️ REPOSITORY PATTERN & UNIT OF WORK**

```mermaid
classDiagram
    class IUnitOfWork {
        <<interface>>
+Users: IUserRepository
   +Customers: ICustomerRepository
        +Tailors: ITailorRepository
        +Orders: IOrderRepository
        +Payments: IPaymentRepository
        +SaveChangesAsync()
        +BeginTransactionAsync()
        +CommitAsync()
        +RollbackAsync()
    }
    
    class UnitOfWork {
     -AppDbContext _context
        +Users: IUserRepository
 +Customers: ICustomerRepository
        +SaveChangesAsync()
        +BeginTransactionAsync()
    }
    
    class IRepository~T~ {
        <<interface>>
        +GetByIdAsync(id)
        +GetAllAsync()
     +AddAsync(entity)
        +UpdateAsync(entity)
        +DeleteAsync(id)
        +FindAsync(predicate)
    }
    
    class EfRepository~T~ {
        -AppDbContext _context
        -DbSet~T~ _dbSet
        +GetByIdAsync(id)
   +GetAllAsync()
        +AddAsync(entity)
}
    
    class UserRepository {
     +GetByEmailAsync(email)
        +GetUserWithProfileAsync(id)
    }
    
    class OrderRepository {
        +GetOrdersByCustomerIdAsync(id)
    +GetOrdersByTailorIdAsync(id)
    }
    
    class AppDbContext {
        +Users: DbSet~User~
   +Orders: DbSet~Order~
        +OnModelCreating()
    }
    
    IUnitOfWork <|.. UnitOfWork
    IRepository~T~ <|.. EfRepository~T~
    EfRepository~T~ <|-- UserRepository
    EfRepository~T~ <|-- OrderRepository
    UnitOfWork --> AppDbContext
    EfRepository~T~ --> AppDbContext
 UnitOfWork --> IRepository~T~

    style IUnitOfWork fill:#e1f5ff
    style IRepository~T~ fill:#e1f5ff
    style UnitOfWork fill:#e8f5e9
    style EfRepository~T~ fill:#f3e5f5
    style AppDbContext fill:#ffebee
```

---

## **🔐 MIDDLEWARE PIPELINE**

```mermaid
graph LR
    subgraph "HTTP Request Pipeline"
        A[HTTP Request] --> B[DeveloperExceptionPage<br/>Development Only]
        B --> C[HTTPS Redirection]
        C --> D[Static Files]
        D --> E[Routing]
        E --> F[Session]
     F --> G[Authentication]
G --> H[Authorization]
        H --> I[UserStatusMiddleware<br/>Custom]
        I --> J[Endpoint Execution<br/>Controller/Page]
        J --> K[Response]
    end
    
    subgraph "Swagger Middleware"
        S1[UseSwagger<br/>Development Only]
   S2[UseSwaggerUI<br/>Development Only]
    end
    
    B --> S1
    S1 --> S2
    S2 --> C
    
    K --> L[HTTP Response]

    style A fill:#e1f5ff
    style B fill:#fff9e1
    style G fill:#ffeb3b
    style H fill:#ff9800
    style I fill:#e91e63
 style J fill:#4caf50
 style L fill:#2196f3
    style S1 fill:#9c27b0
    style S2 fill:#9c27b0
```

---

## **🏛️ LAYERED ARCHITECTURE (N-TIER)**

```mermaid
graph TB
    subgraph "Layer 1: Presentation"
        P1[Web UI<br/>Razor Views/Pages]
P2[API Controllers<br/>REST Endpoints]
        P3[View Models<br/>DTOs]
    end
    
    subgraph "Layer 2: Business Logic"
        B1[Services<br/>Business Rules]
        B2[Validators<br/>FluentValidation]
     B3[Domain Models<br/>Entities]
    B4[Interfaces<br/>Contracts]
    end
    
    subgraph "Layer 3: Data Access"
 D1[Repositories<br/>Data Operations]
        D2[Unit of Work<br/>Transaction Management]
        D3[EF Core<br/>DbContext]
        D4[Migrations<br/>Schema Management]
    end
    
    subgraph "Layer 4: Infrastructure"
        I1[Authentication<br/>JWT/Cookie]
        I2[Email Service<br/>SMTP]
        I3[File Storage<br/>Upload/Download]
        I4[Logging<br/>Serilog]
    end
    
    subgraph "Layer 5: Database"
        DB[(SQL Server<br/>LocalDB)]
    end
 
    P1 --> B1
    P2 --> B1
    P3 --> B1
    
    B1 --> D1
    B1 --> I1
    B1 --> I2
    B1 --> I3
    B2 --> B1
    B3 --> B1
    B4 --> B1
    
    D1 --> D2
 D2 --> D3
 D3 --> D4
    D3 --> DB
    
    I1 -.-> B1
    I2 -.-> B1
    I4 -.-> P1
    I4 -.-> B1

    style P1 fill:#e3f2fd
    style P2 fill:#e3f2fd
    style B1 fill:#e8f5e9
    style B2 fill:#e8f5e9
    style D1 fill:#f3e5f5
    style D2 fill:#f3e5f5
    style D3 fill:#f3e5f5
    style I1 fill:#fff9c4
    style I2 fill:#fff9c4
    style I3 fill:#fff9c4
    style I4 fill:#fff9c4
    style DB fill:#ffebee
```

---

## **🔄 DEPENDENCY INJECTION (IoC CONTAINER)**

```mermaid
graph TB
    subgraph "Program.cs - Service Registration"
  A[builder.Services]
    end
  
    subgraph "Scoped Services"
        B1[IAuthService → AuthService]
        B2[IUserRepository → UserRepository]
        B3[IUnitOfWork → UnitOfWork]
        B4[IOrderRepository → OrderRepository]
    B5[IProfileService → ProfileService]
    end
    
    subgraph "Singleton Services"
C1[ITokenService → TokenService]
        C2[IDateTimeService → DateTimeService]
        C3[ILogger → Logger]
    end
    
    subgraph "Transient Services"
      D1[IEmailService → EmailService]
        D2[IFileUploadService → FileUploadService]
 end
    
    subgraph "DbContext"
        E[AppDbContext<br/>Scoped]
    end
    
    subgraph "Controllers/Services"
     F[Constructor Injection]
 end
    
    A --> B1
    A --> B2
    A --> B3
    A --> B4
    A --> B5
    A --> C1
    A --> C2
    A --> C3
    A --> D1
    A --> D2
    A --> E
    
    B1 --> F
    B2 --> F
    B3 --> F
  C1 --> F
  C2 --> F
  D1 --> F
    E --> F

    style A fill:#2196f3,color:#fff
    style B1 fill:#4caf50
    style B2 fill:#4caf50
    style B3 fill:#4caf50
    style C1 fill:#ff9800
    style C2 fill:#ff9800
    style D1 fill:#9c27b0
    style D2 fill:#9c27b0
    style E fill:#f44336,color:#fff
    style F fill:#00bcd4
```

---

## **📊 DATA FLOW: USER REGISTRATION**

```mermaid
flowchart TD
    START([User Submits<br/>Registration Form])
    
    A[AccountController<br/>Register POST]
    B{Validate<br/>Input}
    
    C[AuthService<br/>RegisterAsync]
    D[Check Email<br/>Exists]
    E{Email<br/>Available?}
    
F[Hash Password<br/>PasswordHasher]
    G[Create User<br/>Entity]
  H[Assign Role]
    I[Create Profile<br/>Customer/Tailor]
    
    J[UserRepository<br/>AddAsync]
    K[Unit of Work<br/>BeginTransaction]
    L[AppDbContext<br/>SaveChangesAsync]
    M[(Database<br/>Insert Records)]
    
    N{Transaction<br/>Success?}
    O[Commit<br/>Transaction]
    P[Rollback<br/>Transaction]
    
    Q[Generate Email<br/>Verification Token]
    R[Send Welcome<br/>Email]
    
    S{Role Type?}
    T[Auto-Login<br/>Customer]
    U[Redirect to<br/>Complete Profile<br/>Tailor]
    
    V[Return Success]
    W[Return Error]
    
    START --> A
    A --> B
    B -->|Valid| C
    B -->|Invalid| W
    
    C --> D
    D --> E
    E -->|Yes| F
    E -->|No| W
    
    F --> G
    G --> H
  H --> I
    
    I --> J
  J --> K
 K --> L
    L --> M
    
    M --> N
    N -->|Yes| O
    N -->|No| P
    
 O --> Q
    Q --> R
    R --> S
    
    S -->|Customer| T
    S -->|Tailor| U
    
    P --> W
    T --> V
    U --> V
    
    V --> END([Success])
    W --> FAIL([Show Error])

    style A fill:#fff9e1
    style C fill:#e8f5e9
    style F fill:#e8f5e9
    style J fill:#f3e5f5
    style L fill:#f3e5f5
    style M fill:#ffebee
 style B fill:#ffe0b2
    style E fill:#ffe0b2
    style N fill:#ffe0b2
    style S fill:#ffe0b2
    style V fill:#c8e6c9
    style W fill:#ffcdd2
```

---

## **🎨 FRONT-END TO BACK-END COMMUNICATION**

```mermaid
sequenceDiagram
    participant Browser
    participant RazorView
    participant Controller
    participant Service
    participant Repository
  participant Database

    Note over Browser,Database: Traditional MVC Flow

    Browser->>RazorView: Navigate to /Account/Login
RazorView-->>Browser: Render Login Form
    
    Browser->>Controller: POST /Account/Login
    Note over Browser,Controller: Form Data: email, password
    
    Controller->>Controller: Validate AntiForgery Token
    Controller->>Controller: Validate Model State
    
    Controller->>Service: AuthService.ValidateUserAsync()
    Service->>Repository: UserRepository.GetByEmailAsync()
    Repository->>Database: SELECT * FROM Users
    Database-->>Repository: User Record
    Repository-->>Service: User Entity
    
    Service->>Service: PasswordHasher.Verify()
    
    alt Valid Login
        Service-->>Controller: (true, null, User)
        Controller->>Controller: Create Claims
      Controller->>Controller: Sign In (Cookie)
    Controller-->>Browser: Redirect to Dashboard
        Browser->>Controller: GET /Dashboards/Customer
        Controller->>RazorView: Return View
     RazorView-->>Browser: Render Dashboard
    else Invalid Login
        Service-->>Controller: (false, error, null)
        Controller->>RazorView: Return Login View + Error
        RazorView-->>Browser: Show Error Message
    end

    Note over Browser,Database: API Flow (Mobile/SPA)
    
    Browser->>Controller: POST /api/auth/login
    Note over Browser,Controller: JSON: { email, password }
    
    Controller->>Service: AuthService.ValidateUserAsync()
    Service->>Repository: UserRepository.GetByEmailAsync()
    Repository->>Database: Query
    Database-->>Repository: Data
    Repository-->>Service: Entity
    Service-->>Controller: Result
    
    alt Valid
        Controller->>Controller: Generate JWT Token
        Controller-->>Browser: 200 OK + JWT
    else Invalid
    Controller-->>Browser: 401 Unauthorized + Error JSON
    end
```

---

## **🔧 SERVICE LAYER INTERACTIONS**

```mermaid
graph LR
    subgraph "Controllers"
        A[AccountController]
        B[ProfilesController]
        C[OrdersController]
   D[ApiAuthController]
    end
    
    subgraph "Services"
        E[AuthService]
        F[ProfileService]
        G[OrderService]
     H[EmailService]
        I[FileUploadService]
      J[ValidationService]
        K[TokenService]
    end
    
    subgraph "Repositories"
        L[UserRepository]
        M[CustomerRepository]
        N[TailorRepository]
     O[OrderRepository]
        P[PaymentRepository]
    end
    
    subgraph "Unit of Work"
        Q[IUnitOfWork]
    end
    
    A --> E
    A --> H
    B --> F
    B --> I
    C --> G
C --> J
    D --> E
    D --> K
    
    E --> L
    E --> Q
    F --> M
    F --> N
    G --> O
G --> P
    
    L --> Q
    M --> Q
 N --> Q
    O --> Q
    P --> Q

    style A fill:#fff9e1
    style B fill:#fff9e1
    style C fill:#fff9e1
    style D fill:#fff9e1
    style E fill:#e8f5e9
    style F fill:#e8f5e9
    style G fill:#e8f5e9
    style H fill:#e8f5e9
    style I fill:#e8f5e9
    style J fill:#e8f5e9
    style K fill:#e8f5e9
    style L fill:#f3e5f5
    style M fill:#f3e5f5
    style N fill:#f3e5f5
    style O fill:#f3e5f5
    style P fill:#f3e5f5
    style Q fill:#e1bee7
```

---

## **📱 COMPLETE USER JOURNEY: CUSTOMER ORDERS TAILOR**

```mermaid
sequenceDiagram
    participant Customer
    participant Web as Web UI
    participant OrderCtrl as OrdersController
    participant OrderSvc as Order Service
participant NotifSvc as Notification Service
    participant EmailSvc as Email Service
    participant OrderRepo as Order Repository
    participant PaymentRepo as Payment Repository
    participant UoW as Unit of Work
    participant DB as Database
    participant Tailor

    Customer->>Web: Browse Tailors
    Web->>Customer: Display Tailor Profiles
  
    Customer->>Web: Select Tailor & Click "Create Order"
    Web->>OrderCtrl: POST /Orders/Create
    
    activate OrderCtrl
    OrderCtrl->>OrderSvc: CreateOrderAsync(orderData)
    activate OrderSvc
    
    OrderSvc->>OrderSvc: Validate Order Details
    OrderSvc->>OrderSvc: Calculate Total Price
    
    OrderSvc->>UoW: BeginTransactionAsync()
    activate UoW
    
    OrderSvc->>OrderRepo: AddAsync(order)
 activate OrderRepo
    OrderRepo->>DB: INSERT INTO Orders
    DB-->>OrderRepo: Order Created
    deactivate OrderRepo
    
    OrderSvc->>PaymentRepo: AddAsync(payment)
  activate PaymentRepo
    PaymentRepo->>DB: INSERT INTO Payments
 DB-->>PaymentRepo: Payment Recorded
    deactivate PaymentRepo
    
    OrderSvc->>UoW: SaveChangesAsync()
    UoW->>DB: COMMIT TRANSACTION
    deactivate UoW
    
    OrderSvc->>NotifSvc: NotifyTailorNewOrder(order)
    activate NotifSvc
  NotifSvc->>DB: INSERT INTO Notifications
    deactivate NotifSvc
    
    OrderSvc->>EmailSvc: SendOrderConfirmation(customer, order)
    activate EmailSvc
    EmailSvc-->>Customer: Email: Order Confirmed
    deactivate EmailSvc
    
    OrderSvc->>EmailSvc: SendNewOrderAlert(tailor, order)
    EmailSvc-->>Tailor: Email: New Order Received
    
    OrderSvc-->>OrderCtrl: Order Created Successfully
    deactivate OrderSvc
    
    OrderCtrl-->>Web: Redirect to Order Details
    deactivate OrderCtrl
    
    Web-->>Customer: Display Order Confirmation

    Note over Customer,Tailor: Tailor receives notification
    
    Tailor->>Web: Login & Check Notifications
    Web->>Tailor: Display New Order Alert
    
    Tailor->>Web: View Order Details
    Web->>OrderCtrl: GET /Orders/{id}
    OrderCtrl->>OrderRepo: GetByIdAsync(id)
    OrderRepo->>DB: SELECT FROM Orders
    DB-->>OrderRepo: Order Details
    OrderRepo-->>OrderCtrl: Order Data
    OrderCtrl-->>Web: Return Order View
    Web-->>Tailor: Display Full Order Info
```

---

## **🎯 KEY ARCHITECTURAL PATTERNS**

### **1. Repository Pattern**
- Abstracts data access logic
- Provides clean API for domain objects
- Makes testing easier (mockable)

### **2. Unit of Work Pattern**
- Coordinates transactions across multiple repositories
- Ensures data consistency
- Manages DbContext lifecycle

### **3. Dependency Injection**
- Loose coupling between components
- Easier testing and maintenance
- Configured in `Program.cs`

### **4. Service Layer Pattern**
- Encapsulates business logic
- Provides reusable operations
- Keeps controllers thin

### **5. DTO/ViewModel Pattern**
- Separates domain models from presentation
- Controls data exposure
- Validates input/output

---

## **📊 TECHNOLOGY STACK**

```mermaid
graph TB
    subgraph "Front-End"
        A[Razor Views/Pages]
     B[Bootstrap 5]
        C[jQuery]
    D[JavaScript]
    end
    
  subgraph "Back-End"
   E[ASP.NET Core 9]
        F[C# 13]
        G[Entity Framework Core 9]
 H[FluentValidation]
    end
    
subgraph "Authentication"
        I[Cookie Authentication]
        J[JWT Bearer]
        K[Google OAuth]
    end
    
    subgraph "Infrastructure"
     L[Serilog]
   M[Swagger/OpenAPI]
        N[AutoMapper]
    end
    
    subgraph "Database"
        O[SQL Server LocalDB]
   P[EF Migrations]
    end
    
    A --> E
    B --> A
    C --> A
    D --> A
    E --> G
    E --> H
    E --> I
    E --> J
    E --> K
    E --> L
    E --> M
    G --> O
    P --> O

 style E fill:#512da8,color:#fff
    style G fill:#7b1fa2,color:#fff
    style O fill:#c62828,color:#fff
 style I fill:#f57c00,color:#fff
 style J fill:#f57c00,color:#fff
    style M fill:#00897b,color:#fff
```

---

## **📝 SUMMARY**

### **Layer Responsibilities:**

| Layer | Responsibility | Examples |
|-------|---------------|----------|
| **Presentation** | User interface, HTTP handling | Controllers, Views, API |
| **Business Logic** | Business rules, validation | Services, Validators |
| **Data Access** | Database operations | Repositories, DbContext |
| **Infrastructure** | Cross-cutting concerns | Email, File Storage, Logging |
| **Database** | Data persistence | SQL Server, Tables |

### **Communication Patterns:**

- **Top-Down**: Presentation → Business → Data → Database
- **Dependency Injection**: All layers use interfaces
- **Async/Await**: Throughout the stack
- **Transaction Management**: Unit of Work pattern
- **Error Handling**: Try-catch with logging

### **Key Benefits:**

✅ **Separation of Concerns** - Each layer has single responsibility  
✅ **Testability** - Mock dependencies easily  
✅ **Maintainability** - Changes isolated to specific layers  
✅ **Scalability** - Can scale individual components  
✅ **Reusability** - Services can be shared across controllers  

---

**Date:** 2025-01-20  
**Architecture:** N-Tier Layered Architecture  
**Framework:** ASP.NET Core 9.0  
**Pattern:** Repository + Unit of Work + Service Layer  

---

**🎉 Tafsilk Platform - Built with Clean Architecture Principles!**

**تفصيلك - نربط بينك وبين أفضل الخياطين** 🧵✂️
