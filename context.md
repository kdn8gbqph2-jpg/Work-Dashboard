# Work Dashboard - Project Context

## Project Overview
**Work Dashboard** is a Blazor Server application built on .NET 8 for managing works and projects at Bharatpur Development Authority (BDA). The system tracks work assignments, progress, remarks, and engineer assignments across different organizational levels.

---

## Technology Stack

### Framework & Runtime
- **.NET 8.0** - Target framework
- **Blazor Server** - Interactive server-side rendering
- **C# 12** - Programming language

### Database & ORM
- **MySQL** - Database system (via MySqlConnector)
- **Entity Framework Core 8.0** - ORM
- **Pomelo.EntityFrameworkCore.MySql** - MySQL provider

### Authentication & Security
- **ASP.NET Core Cookie Authentication** - Session management
- **BCrypt.Net-Next** - Password hashing

### UI Framework
- **Bootstrap 5** - CSS framework
- **Bootstrap Icons** - Icon library
- Custom CSS (`wwwroot/app.css`) - Application-specific styles

---

## Project Structure

```
Work-Dashboard/
├── Components/
│   ├── Pages/
│   │   ├── Admin.razor          # Admin dashboard (main page)
│   │   ├── Jen.razor            # JEN engineer dashboard
│   │   ├── Login.razor          # Login page
│   │   ├── Home.razor           # Home page
│   │   └── Error.razor          # Error handling page
│   ├── Layout/
│   │   └── MainLayout.razor     # Application layout
│   ├── App.razor                # Root component
│   ├── Routes.razor             # Routing configuration
│   └── _Imports.razor           # Global using statements
├── Data/
│   ├── BdaDbContext.cs          # EF Core DbContext
│   ├── DataSeeder.cs            # CSV data seeding
│   └── Entities/
│       ├── Work.cs              # Work entity
│       ├── Engineer.cs          # Engineer/User entity
│       ├── WorkCategory.cs      # Category reference data
│       ├── FundSource.cs        # Fund source reference data
│       └── WorkRemark.cs        # Remarks/comments entity
├── Services/
│   ├── IAuthService.cs          # Authentication interface
│   └── AuthService.cs           # Authentication implementation
├── wwwroot/
│   ├── app.css                  # Custom application styles
│   └── images/                  # Static assets
├── Program.cs                   # Application startup & configuration
└── Work-Dashboard.csproj        # Project file
```

---

## Key Features

### User Management
- **Roles**: ADMIN, JEN (Junior Engineer), AEN (Assistant Engineer), XEN (Executive Engineer)
- **Authentication**: Cookie-based with role-based authorization
- **Password Management**: BCrypt hashing, change password functionality

### Work Management
- **CRUD Operations**: Create, Read, Update works
- **Status Tracking**: ONGOING, COMPLETED, STALLED, NOT_STARTED
- **Progress Monitoring**: Physical and financial progress percentages
- **Assignment**: Multi-level engineer assignments (JEN, AEN, XEN)
- **Categorization**: Work categories and fund sources
- **Flags**: Annual Contract, Scheme, CM Budget

### Remarks System
- **Dynamic Remarks**: User-added comments with author and timestamp
- **Static Remarks**: Imported remarks from original data
- **Real-time Updates**: View and add remarks on works

### Dashboard Views
- **Admin Dashboard**:
  - Overview statistics (total, ongoing, completed, annual works)
  - Category-wise and fund-wise breakdowns
  - User management
  - Work management with advanced filtering
  - Reference data management (categories, fund sources, departments)

- **JEN Dashboard**:
  - Work summary by category
  - Assigned works list
  - Remarks management

### Filtering & Search
- Filter by: Status, Engineer (JEN/AEN/XEN), Date range
- Search by work name
- Real-time filter application

---

## Database Schema

### Core Tables

#### `engineers`
- `engineer_id` (PK, Auto-increment)
- `name`, `username`, `password_hash`
- `role` (ENUM: ADMIN, JEN, AEN, XEN)
- `email`, `mobile`
- `is_active`, `is_deleted`
- `created_at`, `updated_at`

#### `works`
- `work_id` (PK, Auto-increment)
- `work_name`, `work_code`
- `category_id` (FK), `fund_source_id` (FK)
- `assigned_jen_id`, `assigned_aen_id`, `assigned_exen_id` (FK)
- `status` (ENUM: ONGOING, COMPLETED, STALLED, NOT_STARTED)
- `sanctioned_amount`, `agreement_amount`, `expenditure`
- `progress_percent`, `financial_progress_percent`
- `start_date`, `expected_completion`, `actual_completion`
- `is_annual_contract`, `is_scheme`, `is_cm_budget`
- `location`, `ward_number`, `department`
- `contractor_name`, `contractor_mobile`
- `remarks` (static)
- `created_at`, `updated_at`, `created_by`
- `is_deleted`

#### `work_remarks`
- `remark_id` (PK, Auto-increment)
- `work_id` (FK)
- `author_id` (FK to engineers)
- `author_name`
- `content`
- `created_at`

#### `work_categories`
- `category_id` (PK)
- `category_code`, `category_name`
- `is_active`

#### `fund_sources`
- `fund_source_id` (PK)
- `source_code`, `source_name`
- `is_active`

---

## Authentication & Authorization

### Login Flow
1. User enters username/password at `/login`
2. `AuthService.ValidateAsync()` verifies credentials
3. Claims created: NameIdentifier, Name, Role, username
4. Cookie authentication with 8-hour expiration
5. Redirect based on role:
   - ADMIN → `/admin`
   - Others → `/jen`

### Authorization
- `[Authorize(Roles = "ADMIN")]` - Admin-only pages
- `[Authorize(Roles = "JEN,ADMIN,AEN,XEN")]` - Engineer dashboards
- Cascading authentication state via `<CascadingAuthenticationState>`

---

## UI/UX Design System

### Color Palette
- **Primary Orange**: `#f97316` (active states, accents)
- **Bright Orange**: `#ff8c00` (table headers)
- **Dark Slate**: `#1a2e44` (sidebar, text)
- **Light Gray**: `#f8fafc` (backgrounds)
- **Red**: `#dc2626` (logout, destructive actions)
- **Green**: `#22c55e` (success, completed)
- **Blue**: `#0d6efd` (info, primary actions)

### Component Patterns

#### Buttons
- **Change Password**: Dark slate with neutral styling
- **Logout**: Red destructive styling
- **Reset**: Light gray with orange hover
- **Action Icons**: Filled Bootstrap icons

#### Tables
- **Header**: Bright orange (`#ff8c00`) with white text
- **Rows**: Alternating backgrounds, hover effects
- **Actions**: Filled icon buttons (eye, chat, pencil)

#### Modals
- **Overlay**: Semi-transparent dark background
- **Box**: White with rounded corners, shadow
- **Header**: Flexible layout with word-wrap for long text
- **Footer**: Right-aligned action buttons

#### Filters
- **Background**: `#f8fafc`
- **Borders**: `#cbd5e1`
- **Text**: `#1e293b`
- **Focus**: Orange outline (`#f97316`)
- **Hover**: Soft orange tint (`#ffead5`)

---

## Recent Fixes & Improvements

### Modal Header Text Overflow (Fixed)
- **Issue**: Long work names were cut off in modal headers
- **Solution**: Added `word-break: break-word` and proper flexbox constraints

### Edit Work Crash (Fixed)
- **Issue**: NullReferenceException when editing from Details view
- **Solution**: Added null checks before calling `OpenEditWork()`

### Icon Styling (Enhanced)
- **Change**: Switched from outline to filled Bootstrap Icons
- **Icons**: `bi-eye-fill`, `bi-chat-dots-fill`, `bi-pencil-fill`, `bi-info-circle-fill`

### Button Theme (Updated)
- **Change Password**: Neutral dark slate styling
- **Logout**: Prominent red destructive styling
- **Filters**: Light gray with orange accents
- **Reset**: Orange hover tint

### Static Remarks (Removed)
- **Removed**: Static remarks field from Edit Work modal
- **Reason**: Separation between imported and user-generated remarks

---

## Configuration

### Connection String
```csharp
"DefaultConnection": "server=localhost;database=bda_db;user=root;password=yourpassword"
```

### Authentication Settings
```csharp
LoginPath: "/login"
LogoutPath: "/account/logout"
ExpireTimeSpan: 8 hours
SlidingExpiration: true
Cookie.Name: "bda.auth"
```

### Blazor Configuration
```csharp
AddRazorComponents()
    .AddInteractiveServerComponents()
```

---

## Development Environment

### IDE
- **Visual Studio Community 2026** (Version 18.5.1)

### Version Control
- **Git** repository
- **GitHub**: https://github.com/kdn8gbqph2-jpg/Work-Dashboard
- **Branch**: master

### Project Location
```
C:\Users\IT Cell\source\repos\Work-Dashboard\
```

### Terminal
- **PowerShell** (default shell)

---

## Build & Run

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Access
- **Local**: http://localhost:5122
- **Default Admin**: 
  - Username: `admin`
  - Password: `Admin@1234`

---

## Data Seeding

### Initial Setup
1. **Default Admin**: Created on first run if no engineers exist
2. **CSV Import**: Work data seeded from CSV files (once, with guard)
3. **Seeding Location**: `Work_Dashboard.Data.DataSeeder.SeedAsync()`

---

## Known Issues & TODOs

### Current Issues
1. **User Save Error**: Database constraint validation needs investigation
2. **Remarks Modal**: Work name display fixed but needs testing with very long names

### Future Enhancements
- [ ] Add export functionality (Excel/PDF)
- [ ] Implement work history/audit trail
- [ ] Add file attachments to works
- [ ] Enhanced reporting with charts
- [ ] Email notifications
- [ ] Mobile-responsive improvements
- [ ] Dark mode support
- [ ] Bulk operations (assign, update status)

---

## API Endpoints

### Authentication
- `POST /account/login` - User login
- `POST /account/logout` - User logout

### Blazor Endpoints
- All component routing handled by Blazor Router
- Interactive server components via SignalR

---

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.x" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.x" />
<PackageReference Include="MySqlConnector" Version="2.x.x" />
```

---

## Coding Standards

### Naming Conventions
- **Components**: PascalCase (`Admin.razor`)
- **Methods**: PascalCase (`OpenEditWork()`)
- **Variables**: camelCase (`workFilter`)
- **CSS Classes**: kebab-case (`modal-box-header`)
- **Database**: snake_case (`work_id`, `created_at`)

### Code Organization
- **Separation of Concerns**: Data/Services/Components
- **Async/Await**: Used throughout for DB operations
- **Null Safety**: Null checks added after recent fixes
- **Error Handling**: Try-catch with user-friendly messages

---

## Performance Considerations

- **DbContextFactory**: Used for proper async DB access in Blazor
- **Eager Loading**: Includes for related entities (Categories, Engineers)
- **Filtering**: Server-side with indexed queries
- **Caching**: Consider implementing for reference data

---

## Security Considerations

- **Password Hashing**: BCrypt with work factor 11
- **SQL Injection**: Protected via EF Core parameterization
- **XSS Protection**: Blazor automatic escaping
- **CSRF Protection**: Antiforgery tokens on forms
- **Authorization**: Role-based access control throughout

---

## Contact & Support

**Organization**: Bharatpur Development Authority (BDA)  
**Development Team**: IT Cell  
**Repository**: https://github.com/kdn8gbqph2-jpg/Work-Dashboard

---

*Last Updated: 2024*  
*Version: 1.0*  
*Framework: .NET 8 / Blazor Server*
