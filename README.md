# Pizzeria Order Processing System

A .NET 9 console application that processes pizza orders, calculates total prices, and determines ingredient requirements for a pizzeria business.

## 📋 Table of Contents

- [Features](#features)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Running with VS Code](#running-with-vs-code)
- [Testing](#testing)
- [Data Formats](#data-formats)
- [Architecture](#architecture)
- [Code Quality & Analysis](#code-quality--analysis)
- [Git History](#git-history)

## Features

- **Multi-format Order Processing**: Supports both CSV and JSON order files
- **Order Validation**: Validates orders and filters out invalid entries
- **Price Calculation**: Calculates total order prices based on product pricing
- **Ingredient Management**: Calculates total ingredient requirements for all orders
- **Error Handling**: Error handling with detailed logging
- **Extensible Design**: Factory and Strategy patterns for parsers, dependency injection ready

## Project Structure

```
pizzeria/
├── .github/
│   └── workflows/
│       └── build.yml                # GitHub Actions CI/CD pipeline
├── src/
│   └── Pizzeria.App/
│       ├── Data/                    # Sample data files
│       ├── Interfaces/              # Service contracts
│       ├── Models/                  # Domain models
│       ├── Parsers/                 # File parsing implementations
│       ├── Services/                # Business logic services
│       ├── Utils/                   # Utility classes
│       ├── Validators/              # Order validation logic
│       └── Program.cs               # Application entry point
├── tests/
│   └── Pizzeria.App.Tests/         # Unit tests
└── .vscode/                         # VS Code configuration
```

## ⚡ Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Visual Studio Code (recommended)
- C# Dev kit Extension for VS Code

## 🚀 Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/ashil816/pizzeria.git
   cd pizzeria
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

## Usage

The application requires three arguments:
1. Orders file path (CSV or JSON)
2. Products file path (JSON)
3. Ingredients file path (JSON)

### Command Line Usage

```bash
dotnet run --project src/Pizzeria.App -- \
  "src/Pizzeria.App/Data/orders.json" \
  "src/Pizzeria.App/Data/Products.json" \
  "src/Pizzeria.App/Data/Ingredients.json"
```

### Sample Output

```
Parsed 10 orders from orders.json.
Validated 8 orders from orders.json.
Order ID: 11111111-1111-1111-1111-111111111111 with 2 items.
Total price for Order ID 11111111-1111-1111-1111-111111111111: $25.50.
Total ingredients required for all orders:
Cheese: 450 g
Pepperoni: 240 g
Tomato Sauce: 750 ml
```

## Running with VS Code

The project is pre-configured for VS Code debugging and includes sample data files for quick testing.

### Quick Start with Sample Data

For rapid testing, use the included sample data files in the `src/Pizzeria.App/Data/` folder:
- **`orders.json`** - Sample pizza orders in JSON format
- **`orders.csv`** - Sample pizza orders in CSV format  
- **`Products.json`** - Pizza products with pricing
- **`Ingredients.json`** - Ingredient requirements per product

```json
{
  "name": ".NET 9 Console App",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "build",
  "program": "${workspaceFolder}/src/Pizzeria.App/bin/Debug/net9.0/Pizzeria.App.dll",
  "args": [
    "${workspaceFolder}/src/Pizzeria.App/Data/orders.json",
    "${workspaceFolder}/src/Pizzeria.App/Data/Products.json",
    "${workspaceFolder}/src/Pizzeria.App/Data/Ingredients.json"
  ],
  "cwd": "${workspaceFolder}/src/Pizzeria.App"
}
```

### Custom Data Setup

If you want to test with your own data files, configure file paths in `.vscode/launch.json`:

1. **Configure file paths** in `.vscode/launch.json`:
   ```json
   {
     "name": ".NET 9 Console App",
     "type": "coreclr",
     "request": "launch",
     "preLaunchTask": "build",
     "program": "${workspaceFolder}/src/Pizzeria.App/bin/Debug/net9.0/Pizzeria.App.dll",
     "args": [
       "/absolute/path/to/orders.json",
       "/absolute/path/to/products.json", 
       "/absolute/path/to/ingredients.json"
     ],
     "cwd": "${workspaceFolder}/src/Pizzeria.App"
   }
   ```

2. **Update the args section** with absolute paths to your data files:
   - Replace `/absolute/path/to/orders.json` with full path to your orders file
   - Replace `/absolute/path/to/products.json` with full path to your products file  
   - Replace `/absolute/path/to/ingredients.json` with full path to your ingredients file

### Running

1. **Open in VS Code**: `code .`
2. **Set breakpoints** as needed
3. **Press F5** or go to Run and Debug → `.NET 9 Console App`

The debugger will build the project and run with the configured file paths.

## Testing

Run all tests:
```bash
dotnet test
```

### Test Structure

- **Unit Tests**: Comprehensive test coverage for all services and utilities
- **Test Frameworks**: xUnit, Shouldly, NSubstitute, AutoFixture
- **Test Data**: Sample data files in `tests/TestData/`

## Data Formats

### Orders (CSV/JSON)
```json
{
  "OrderId": "11111111-1111-1111-1111-111111111111",
  "ProductId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "Quantity": 2,
  "DeliveryAt": "2025-06-12T12:00:00",
  "CreatedAt": "2025-06-11T10:00:00",
  "DeliveryAddress": "123 Main St"
}
```

### Products (JSON)
```json
{
  "ProductId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "ProductName": "Margherita Pizza",
  "Price": 12.50
}
```

### Ingredients (JSON)
```json
{
  "ProductId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "Ingredients": [
    { "Name": "Tomato Sauce", "Amount": 150, "Unit": "ml" },
    { "Name": "Mozzarella", "Amount": 200, "Unit": "g" }
  ]
}
```

## Architecture

### Design Patterns
- **Factory Pattern**: `OrderParserFactory` for creating appropriate parsers
- **Strategy Pattern**: Different parsers for CSV and JSON formats
- **Dependency Injection**: Service registration and resolution

### Key Components
- **OrderService**: Main business logic for order processing
- **OrderValidator**: Validates order data and business rules
- **OrderUtils**: Static utility methods for calculations
- **Parser Factory**: Creates appropriate parsers based on file extension

## Code Quality & Analysis

### SonarQube Integration

This project is configured for SonarQube Cloud analysis for code quality monitoring. However, **SonarQube Cloud analysis is currently disabled** as the free tier makes project code and analysis results publicly visible.

**Note**: The GitHub Actions workflow (`.github/workflows/build.yml`) is configured to trigger on every commit to the main branch and pull requests. Due to the disabled SonarQube integration, these workflow runs will fail at the analysis step, but this is expected behavior.

## Git History

Review the git commit history to gain insights into how the code got evolved during commits. The commit messages follow conventional commit format.

```bash
# View commit history with details
git log --oneline --graph
```


