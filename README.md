# 💸 Expense Tracker — ASP.NET Core MVC 8

A full-stack personal finance tracking web application built with **ASP.NET Core MVC .NET 8**. Track your daily expenses, visualize spending with interactive charts, manage categories, and export monthly reports to Excel or PDF.

![.NET](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)
![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-MVC-blue)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Express-red?logo=microsoftsqlserver)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0-green)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-blueviolet?logo=bootstrap)

---

## ✨ Features

| Feature | Details |
|---|---|
| 🔐 **Authentication** | Register, Login, Logout via ASP.NET Identity |
| 💰 **Expense Management** | Add, Edit, Delete expenses with category & notes |
| 🏷️ **Categories** | 9 default categories + create your own with custom color & icon |
| 📊 **Dashboard** | Summary cards, Doughnut chart & Line chart (Chart.js) |
| 🔍 **Filter & Search** | Filter expenses by title, category, and month |
| 📅 **Monthly Reports** | Category breakdown with progress bars |
| 📥 **Export to Excel** | Styled `.xlsx` report via EPPlus |
| 📄 **Export to PDF** | Formatted PDF report via iTextSharp |
| 🔒 **User Isolation** | Each user sees only their own data |
| 📱 **Responsive UI** | Dark sidebar layout with Bootstrap 5 |

---

## 🖥️ Screenshots

> Dashboard with charts, expense list, and monthly reports.
https://github.com/user-attachments/assets/70be3623-f9b3-421e-8194-1d4002b89af2
---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core 8 (Code-First) |
| Database | SQL Server / SQL Server Express |
| Authentication | ASP.NET Core Identity |
| UI | Bootstrap 5 + Font Awesome 6 |
| Charts | Chart.js 4 |
| Excel Export | EPPlus 7 |
| PDF Export | iTextSharp 5 |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server or SQL Server Express
- Visual Studio 2022 or VS Code

### 1. Clone the repository
```bash
git clone https://github.com/YOUR_USERNAME/expense-tracker-mvc.git
cd expense-tracker-mvc
```

### 2. Configure the database connection
Open `appsettings.json` and update the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ExpenseTrackerDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 3. Apply migrations & create the database
```bash
dotnet ef database update
```
This creates the database and seeds 9 default categories automatically.

### 4. Run the app
```bash
dotnet run
```

Open your browser at `https://localhost:5001`

### 5. Register & Login
- Click **"Create one"** to register a new account
- Login and start adding expenses!

---

## 📁 Project Structure

```
ExpenseTracker/
├── Controllers/
│   ├── AccountController.cs     # Register, Login, Logout
│   ├── DashboardController.cs   # Analytics & charts
│   ├── ExpenseController.cs     # Expense CRUD
│   ├── CategoryController.cs    # Category CRUD
│   └── ReportController.cs      # Reports & exports
├── Data/
│   └── ApplicationDbContext.cs  # EF Core DbContext
├── Migrations/                  # EF Core migrations
├── Models/
│   ├── Expense.cs
│   ├── Category.cs
│   ├── ApplicationUser.cs
│   └── ViewModels/              # View-specific models
├── Services/
│   ├── ExpenseService.cs        # Business logic
│   └── ReportService.cs        # Excel & PDF generation
├── Views/
│   ├── Dashboard/               # Chart dashboard
│   ├── Expense/                 # CRUD views
│   ├── Category/                # Category views
│   ├── Report/                  # Monthly report view
│   ├── Account/                 # Login & Register
│   └── Shared/_Layout.cshtml   # Master sidebar layout
├── wwwroot/
│   ├── css/site.css             # Custom styles
│   └── js/site.js              # Sidebar & alerts JS
├── Program.cs                   # App configuration
└── appsettings.json             # Settings (not committed)
```

---

## 📊 Database Schema

```
AspNetUsers (Identity)
    │
    ├──< Expenses (CategoryId FK, UserId FK)
    │
    └──< Categories (UserId FK, nullable = default categories)
```

---

## 🔑 Key Concepts Demonstrated

- ✅ **MVC Pattern** with Service Layer separation
- ✅ **EF Core Code-First** with migrations & data seeding
- ✅ **ASP.NET Identity** for secure authentication
- ✅ **User-scoped data** — strict data isolation per user
- ✅ **Chart.js integration** via C# → JSON → JavaScript
- ✅ **File exports** (Excel + PDF) returned as byte arrays
- ✅ **Repository/Service pattern** for clean, testable code
- ✅ **Responsive sidebar layout** with Bootstrap 5

---

## 📦 NuGet Packages

```xml
<PackageReference Include="EPPlus" Version="7.0.9" />
<PackageReference Include="iTextSharp" Version="5.5.13.3" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

---

## 👤 Author

**Mubasherjam**
- GitHub: [@mubasherjam](https://github.com/mubasherjam)
- LinkedIn: [Mubasher Jam](https://linkedin.com/in/mubasherjam)

---

## 📄 License

This project is open source and available under the [MIT License](LICENSE).
