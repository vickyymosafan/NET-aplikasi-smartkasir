# SmartKasir POS System

Sistem Point of Sale (POS) enterprise berbasis WPF .NET 10 dengan arsitektur offline-first untuk toko retail.

## 🏗️ Arsitektur

```
SmartKasir/
├── SmartKasir/                    # WPF Client (Desktop App)
├── SmartKasir.Core/               # Domain Layer (Entities, Interfaces)
├── SmartKasir.Application/        # Application Layer (Services, DTOs)
├── SmartKasir.Infrastructure/     # Infrastructure Layer (EF Core, Repositories)
├── SmartKasir.Server/             # ASP.NET Core Web API
├── SmartKasir.Shared/             # Shared DTOs & Contracts
└── SmartKasir.Tests/              # Unit & Property-Based Tests
```

## 🚀 Fitur Utama

- **Autentikasi JWT** dengan Role-Based Access Control (Admin/Kasir)
- **Manajemen Produk** dengan barcode scanning
- **Keranjang Belanja** dengan real-time calculation
- **Transaksi** dengan invoice generation (INV-YYYYMMDD-XXXX)
- **Offline-First** dengan SQLite lokal dan sync ke PostgreSQL
- **Laporan Penjualan** dengan export PDF/Excel
- **Dark Mode UI** dengan Vercel-style design

## 📋 Prerequisites

- .NET 10 SDK
- PostgreSQL (untuk server)
- Visual Studio 2022 atau VS Code

## 🛠️ Setup Development

```bash
# Clone repository
git clone https://github.com/[username]/SmartKasir.git
cd SmartKasir

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run server
cd SmartKasir.Server
dotnet run

# Run client (separate terminal)
cd SmartKasir
dotnet run
```

## 📦 Project Structure

| Project                     | Description                              | Dependencies                        |
| --------------------------- | ---------------------------------------- | ----------------------------------- |
| `SmartKasir.Core`           | Domain entities, enums, interfaces       | None                                |
| `SmartKasir.Shared`         | Shared DTOs, contracts, constants        | Core                                |
| `SmartKasir.Application`    | Application services, validators         | Core, Shared                        |
| `SmartKasir.Infrastructure` | EF Core, repositories, external services | Core, Application                   |
| `SmartKasir.Server`         | ASP.NET Core Web API                     | Application, Infrastructure, Shared |
| `SmartKasir`                | WPF Desktop Client                       | Application, Shared                 |
| `SmartKasir.Tests`          | Unit & Property-Based Tests              | All projects                        |

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test SmartKasir.Tests
```

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

## 👥 Contributors

- [vickymosafan]
