# 🏪 KhadiStore - Complete E-commerce Management System

![.NET Core](https://img.shields.io/badge/.NET%20Core-6.0-blue) ![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-green) ![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-purple) ![License](https://img.shields.io/badge/License-MIT-yellow)

## 📋 Table of Contents

- [Project Overview](#-project-overview)
- [Technology Stack](#-technology-stack)
- [Architecture & Project Structure](#-architecture--project-structure)
- [Database Schema](#-database-schema)
- [Core Features](#-core-features)
- [Business Logic](#-business-logic)
- [API Endpoints](#-api-endpoints)
- [Setup Instructions](#-setup-instructions)
- [Usage Examples](#-usage-examples)
- [Code Conventions](#-code-conventions)
- [AI Assistant Guidelines](#-ai-assistant-guidelines)
- [Known Issues & Solutions](#-known-issues--solutions)
- [Future Enhancements](#-future-enhancements)

---

## 🎯 Project Overview

**KhadiStore** is a comprehensive e-commerce management system built specifically for Indian retail businesses, particularly those dealing with traditional Indian clothing like Kurtas, Dhotis, and Sarees. The system provides complete retail management capabilities including inventory, sales, customers, and returns.

### Key Highlights:
- **Target Market**: Indian SME retailers and traditional clothing stores
- **Architecture**: Clean Architecture with .NET Core MVC
- **Approach**: Code-first Entity Framework with Repository pattern
- **UI/UX**: Mobile-responsive, bilingual (Hindi/English) interface
- **Business Model**: One-time purchase system (no subscription)

### Project Goals:
1. **Modernize Traditional Retail**: Bring digital efficiency to traditional Indian clothing stores
2. **GST Compliance**: Built-in GST calculations and compliant invoicing
3. **User-Friendly**: Intuitive interface that requires minimal training
4. **Mobile-Ready**: Works seamlessly on tablets and mobile devices
5. **Cost-Effective**: One-time purchase with ongoing support

---

## 🛠️ Technology Stack

### Backend Technologies:
- **.NET Core 6.0** - Main framework
- **ASP.NET Core MVC** - Web application framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Primary database
- **C# 10** - Programming language

### Frontend Technologies:
- **Razor Views** - Server-side rendering
- **Bootstrap 5.3** - CSS framework for responsive design
- **jQuery 3.6** - JavaScript library for DOM manipulation
- **Font Awesome** - Icons and visual elements
- **SweetAlert2** - User-friendly alerts and confirmations

### Development Tools:
- **Visual Studio 2022** - Primary IDE
- **SQL Server Management Studio** - Database management
- **Package Manager Console** - EF migrations and package management
- **Chrome DevTools** - Frontend debugging

### Additional Libraries:
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Model validation
- **Serilog** - Structured logging (optional)

---

## 🏗️ Architecture & Project Structure

### Clean Architecture Implementation:

```
KhadiStore/
├── 📁 KhadiStore.Web/              # Presentation Layer
│   ├── Controllers/                # MVC Controllers
│   ├── Views/                      # Razor Views
│   ├── wwwroot/                   # Static files (CSS, JS, Images)
│   ├── Models/                    # View Models
│   └── Program.cs                 # Application entry point
│
├── 📁 KhadiStore.Application/      # Application Layer
│   ├── DTOs/                      # Data Transfer Objects
│   ├── Interfaces/                # Service interfaces
│   ├── Services/                  # Business logic implementation
│   └── Mappings/                  # AutoMapper profiles
│
├── 📁 KhadiStore.Domain/           # Domain Layer
│   ├── Entities/                  # Domain entities
│   ├── Interfaces/                # Repository interfaces
│   └── Enums/                     # Domain enumerations
│
└── 📁 KhadiStore.Infrastructure/   # Infrastructure Layer
    ├── Data/                      # DbContext and configurations
    ├── Repositories/              # Repository implementations
    └── Migrations/                # EF Core migrations
```

### Key Design Patterns:
1. **Repository Pattern** - Data access abstraction
2. **Unit of Work Pattern** - Transaction management
3. **Dependency Injection** - Loose coupling and testability
4. **DTO Pattern** - Data transfer and validation
5. **MVC Pattern** - Separation of concerns

---

## 🗄️ Database Schema

### Core Entities:

#### 1. **Categories**
```sql
Categories (
    Id (PK, Identity),
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
)
```

#### 2. **Products**
```sql
Products (
    Id (PK, Identity),
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    CategoryId (FK → Categories.Id),
    PurchasePrice DECIMAL(18,2),
    SalePrice DECIMAL(18,2),
    GSTRate DECIMAL(5,2) DEFAULT 5.0,
    StockQuantity INT DEFAULT 0,
    MinimumStock INT DEFAULT 5,
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
)
```

#### 3. **Customers**
```sql
Customers (
    Id (PK, Identity),
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Address TEXT,
    TotalOrders INT DEFAULT 0,              -- Customer statistics
    TotalPurchases DECIMAL(18,2) DEFAULT 0, -- Customer statistics
    LastPurchaseDate DATETIME2,             -- Customer statistics
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
)
```

#### 4. **Sales**
```sql
Sales (
    Id (PK, Identity),
    InvoiceNumber VARCHAR(50) NOT NULL UNIQUE,
    SaleDate DATETIME2 NOT NULL,
    CustomerId (FK → Customers.Id) NULL,    -- NULL for walk-in customers
    PaymentMethod VARCHAR(50),              -- Cash, Card, UPI, BankTransfer
    SubTotal DECIMAL(18,2),                 -- Sum of all line items before GST
    GSTAmount DECIMAL(18,2),                -- Total GST amount
    DiscountAmount DECIMAL(18,2),           -- BILL-LEVEL discount amount
    TotalAmount DECIMAL(18,2),              -- Final amount after discount
    Status VARCHAR(20) DEFAULT 'Completed', -- Completed, Cancelled
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
)
```

#### 5. **SaleItems**
```sql
SaleItems (
    Id (PK, Identity),
    SaleId (FK → Sales.Id),
    ProductId (FK → Products.Id),
    ProductName VARCHAR(200),               -- Snapshot for history
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2),               -- Price at time of sale
    DiscountAmount DECIMAL(18,2) DEFAULT 0, -- Legacy field (new sales use 0)
    GSTRate DECIMAL(5,2),                  -- GST rate at time of sale
    GSTAmount DECIMAL(18,2),               -- Calculated GST amount
    LineTotal DECIMAL(18,2),               -- Quantity × UnitPrice + GST
    CreatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
)
```

#### 6. **Returns**
```sql
Returns (
    Id (PK, Identity),
    ReturnNumber VARCHAR(50) NOT NULL UNIQUE,
    SaleId (FK → Sales.Id),
    ReturnDate DATETIME2 NOT NULL,
    RefundReference VARCHAR(100),
    AdditionalNotes TEXT,
    SubTotal DECIMAL(18,2),
    GSTAmount DECIMAL(18,2),
    DiscountAmount DECIMAL(18,2),           -- Bill-level discount portion
    TotalAmount DECIMAL(18,2),              -- Total refund amount
    Status VARCHAR(20) DEFAULT 'Completed',
    CreatedAt DATETIME2,
    UpdatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
)
```

#### 7. **ReturnItems**
```sql
ReturnItems (
    Id (PK, Identity),
    ReturnId (FK → Returns.Id),
    SaleItemId (FK → SaleItems.Id),
    ProductId (FK → Products.Id),
    ProductName VARCHAR(200),
    ReturnQuantity INT NOT NULL,
    UnitPrice DECIMAL(18,2),
    DiscountAmount DECIMAL(18,2),           -- Proportional bill-level discount
    GSTRate DECIMAL(5,2),
    GSTAmount DECIMAL(18,2),
    LineTotal DECIMAL(18,2),
    CreatedAt DATETIME2,
    IsDeleted BIT DEFAULT 0
)
```

### Relationships:
- **One-to-Many**: Category → Products
- **One-to-Many**: Customer → Sales
- **One-to-Many**: Sale → SaleItems
- **One-to-Many**: Sale → Returns
- **One-to-Many**: Return → ReturnItems
- **Many-to-One**: SaleItem ← Product (for inventory tracking)

---

## ✨ Core Features

### 1. **Product Management**
- **Categories**: Hierarchical organization (Kurtas, Dhotis, Sarees, etc.)
- **Inventory Tracking**: Real-time stock updates with low-stock alerts
- **Pricing**: Purchase price, sale price, and GST rate configuration
- **Product Search**: Quick product lookup by name or category

**Key Logic:**
```csharp
// Stock is decremented atomically during sales
await _productRepository.DecrementStockAsync(productId, quantity);

// Low stock alerts when StockQuantity <= MinimumStock
var lowStockProducts = await _productRepository.GetLowStockProductsAsync();
```

### 2. **Sales & Billing System**
- **Professional Invoicing**: GST-compliant invoices with bilingual support
- **Multiple Payment Methods**: Cash, Card, UPI, Bank Transfer
- **Real-time Calculations**: Live totals with GST and discount calculations
- **Bill-Level Discounting**: Single discount percentage applied to entire bill

**Key Logic:**
```csharp
// Sales calculation logic (CRITICAL):
decimal subtotal = saleItems.Sum(item => item.Quantity * item.UnitPrice);
decimal gstAmount = saleItems.Sum(item => {
    var lineSubtotal = item.Quantity * item.UnitPrice;
    return lineSubtotal * (item.GSTRate / 100);
});
decimal totalBeforeDiscount = subtotal + gstAmount;
decimal discountAmount = totalBeforeDiscount * (billDiscountPercentage / 100);
decimal finalAmount = totalBeforeDiscount - discountAmount;
```

### 3. **Customer Management**
- **Auto-Creation**: Customers automatically created during sales with name/phone
- **Customer Statistics**: Total orders, total purchases, last purchase date
- **Purchase History**: Complete sales history for each customer
- **Walk-in Support**: Sales can be processed without customer information

**Key Logic:**
```csharp
// Customer auto-creation during sales
private async Task<int?> CreateCustomerIfNeededAsync(string name, string phone)
{
    if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phone))
        return null;
        
    // Check existing customer by phone
    var existingCustomer = await _customerRepository.GetCustomerByPhoneAsync(phone);
    if (existingCustomer != null) return existingCustomer.Id;
    
    // Create new customer
    var customer = new Customer { Name = name, Phone = phone };
    return await _customerRepository.AddAsync(customer);
}
```

### 4. **Returns & Refund Management**
- **Partial Returns**: Return any quantity up to originally purchased amount
- **Return Tracking**: Complete audit trail with return numbers
- **Inventory Restoration**: Stock automatically restored on returns
- **Refund Calculations**: Proportional bill-level discount applied to returns

**Key Logic:**
```csharp
// Return calculation with bill-level discount
decimal lineSubtotal = returnQuantity * originalUnitPrice;
decimal lineGST = lineSubtotal * (originalGSTRate / 100);
decimal totalBeforeDiscount = lineSubtotal + lineGST;
decimal discountAmount = totalBeforeDiscount * (originalBillDiscountPercentage / 100);
decimal refundAmount = totalBeforeDiscount - discountAmount;
```

### 5. **Inventory Management**
- **Real-time Updates**: Stock changes immediately with sales and returns
- **Stock Validation**: Prevents overselling with real-time checks
- **Inventory Reports**: Low stock alerts and inventory valuation
- **Audit Trail**: Complete history of stock movements

### 6. **Financial Management**
- **GST Compliance**: Proper GST calculations and reporting
- **Multiple Tax Rates**: Support for different GST rates (0%, 5%, 12%, 18%)
- **Financial Reports**: Sales summaries, tax reports, customer analytics
- **Profit Tracking**: Purchase vs sale price analysis

---

## 🧠 Business Logic

### 1. **Sales Processing Workflow**
```
1. User selects category → Products load via AJAX
2. User selects product → Product details populate (price, stock, GST)
3. User sets quantity → Validation against available stock
4. User adds to cart → Item added with bill-level discount calculation
5. User sets bill discount % → Real-time total recalculation
6. User enters customer info → Auto-creation if not exists
7. User submits → Transaction processing begins
8. Sale created → Stock decremented → Customer stats updated
9. Redirect to sale details → Professional invoice display
```

### 2. **Discount Calculation Logic (CRITICAL)**
**Old Approach (Individual Product Discounts):**
```
Line Total = (Qty × Price - Individual Discount) + GST per item
Sale Total = Sum of all Line Totals
```

**New Approach (Bill-Level Discount):**
```
Step 1: Calculate each line without discount
  Line Subtotal = Qty × Unit Price
  Line GST = Line Subtotal × GST Rate
  
Step 2: Calculate sale totals
  Sale Subtotal = Sum of all Line Subtotals
  Sale GST = Sum of all Line GST
  Total Before Discount = Sale Subtotal + Sale GST
  
Step 3: Apply bill-level discount
  Discount Amount = Total Before Discount × Discount Percentage
  Final Sale Amount = Total Before Discount - Discount Amount
```

### 3. **Return Processing Logic**
```
1. Identify returnable quantities per item
2. For each return item:
   - Calculate original line totals (without discount)
   - Apply original bill discount percentage proportionally
   - Calculate refund amount
3. Restore inventory for returned quantities
4. Update customer statistics (decrement orders/purchases)
5. Generate return receipt
```

### 4. **Customer Auto-Creation Logic**
```
Priority Order:
1. If both name and phone provided:
   - Check if customer exists by phone
   - If exists: use existing customer
   - If not: create new customer with both details
   
2. If only name provided:
   - Create customer with name, empty phone
   
3. If only phone provided:
   - Check existing by phone
   - Create with phone, generic name if not found
   
4. If neither provided:
   - Process as walk-in sale (CustomerId = null)
```

### 5. **Inventory Management Logic**
```csharp
// Stock Decrement (Atomic Operation)
UPDATE Products 
SET StockQuantity = StockQuantity - @quantity,
    UpdatedAt = GETDATE()
WHERE Id = @productId 
  AND StockQuantity >= @quantity 
  AND IsDeleted = 0;

// Return: if @@ROWCOUNT = 0 then insufficient stock
```

---

## 🔗 API Endpoints

### Sales Controller
```csharp
GET  /Sales                              → Index (Sales list)
GET  /Sales/Create                       → Create sale form
POST /Sales/Create                       → Process new sale
GET  /Sales/Details/{id}                 → Sale details view
GET  /Sales/GetProductsByCategory?categoryId={id}  → AJAX product list
GET  /Sales/GetProductDetails?productId={id}       → AJAX product details
```

### Returns Controller
```csharp
GET  /Returns                            → Returns list
GET  /Returns/Create?saleId={id}         → Create return form
POST /Returns/Create                     → Process return
GET  /Returns/Details/{id}               → Return details
```

### Customers Controller
```csharp
GET  /Customers                          → Customers list
GET  /Customers/Create                   → Create customer form  
POST /Customers/Create                   → Add customer
GET  /Customers/Details/{id}             → Customer details with stats
GET  /Customers/Edit/{id}                → Edit customer
POST /Customers/Edit/{id}                → Update customer
```

### Products Controller
```csharp
GET  /Products                           → Products list
GET  /Products/Create                    → Create product form
POST /Products/Create                    → Add product
GET  /Products/Details/{id}              → Product details
GET  /Products/Edit/{id}                 → Edit product
POST /Products/Edit/{id}                 → Update product
GET  /Products/LowStock                  → Low stock report
```

---

## 🚀 Setup Instructions

### Prerequisites:
- **Visual Studio 2022** (Community or higher)
- **.NET 6.0 SDK** or later
- **SQL Server 2019** or later (Express edition acceptable)
- **SQL Server Management Studio** (recommended)

### Installation Steps:

#### 1. **Clone Repository**
```bash
git clone https://github.com/yourusername/KhadiStore.git
cd KhadiStore
```

#### 2. **Database Setup**
```bash
# Update connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KhadiStoreDB;Trusted_Connection=true;MultipleActiveResultSets=true"
}

# Run migrations in Package Manager Console
Update-Database
```

#### 3. **Build Solution**
```bash
# In Visual Studio Package Manager Console
dotnet build
```

#### 4. **Run Application**
```bash
# Press F5 in Visual Studio or
dotnet run --project KhadiStore.Web
```

#### 5. **Initial Data (Optional)**
```sql
-- Insert sample categories
INSERT INTO Categories (Name, Description, CreatedAt, IsDeleted) VALUES
('Kurtas', 'Traditional Indian Kurtas', GETDATE(), 0),
('Dhotis', 'Traditional Dhotis', GETDATE(), 0),
('Sarees', 'Traditional Sarees', GETDATE(), 0);

-- Insert sample products
INSERT INTO Products (Name, CategoryId, PurchasePrice, SalePrice, GSTRate, StockQuantity, CreatedAt, IsDeleted)
VALUES ('Blue Cotton Kurti', 1, 600.00, 850.00, 5.0, 10, GETDATE(), 0);
```

### Configuration:

#### appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KhadiStoreDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 💡 Usage Examples

### 1. **Creating a Sale with Bill Discount**
```javascript
// Frontend JavaScript flow
1. Select category: "Kurtas"
2. Select product: "Blue Cotton Kurti" (₹850, Stock: 10, GST: 5%)
3. Set quantity: 2
4. Add to cart → Line total: ₹1785 (₹1700 + ₹85 GST)
5. Set bill discount: 10%
6. Final calculation:
   - Subtotal: ₹1700
   - GST: ₹85  
   - Total before discount: ₹1785
   - Discount (10%): ₹178.50
   - Final amount: ₹1606.50
```

### 2. **Processing a Return**
```javascript
// Return calculation for 1 item from above sale
1. Original line: ₹850 + ₹42.50 GST = ₹892.50
2. Apply bill discount (10%): ₹89.25
3. Refund amount: ₹892.50 - ₹89.25 = ₹803.25
```

### 3. **Customer Auto-Creation**
```csharp
// During sale creation
CustomerName: "Ramesh Kumar"
CustomerPhone: "9876543210"

// System checks existing customer by phone
var existing = await _customerRepository.GetCustomerByPhoneAsync("9876543210");

// If not found, creates new customer
var customer = new Customer 
{ 
    Name = "Ramesh Kumar", 
    Phone = "9876543210",
    TotalOrders = 0,
    TotalPurchases = 0
};
```

---

## 📝 Code Conventions

### 1. **Naming Conventions**
```csharp
// Classes: PascalCase
public class SaleService

// Methods: PascalCase  
public async Task<Sale> CreateSaleAsync()

// Properties: PascalCase
public string InvoiceNumber { get; set; }

// Fields: camelCase with underscore
private readonly IRepository<Sale> _saleRepository;

// Parameters: camelCase
public async Task ProcessSale(int saleId, decimal amount)
```

### 2. **File Organization**
```
Controllers/    → {Entity}Controller.cs (e.g., SalesController.cs)
Services/       → I{Entity}Service.cs + {Entity}Service.cs  
DTOs/          → {Entity}Dto.cs, Create{Entity}Dto.cs
Views/         → {Controller}/{Action}.cshtml
Models/        → {Purpose}ViewModel.cs
```

### 3. **Error Handling Pattern**
```csharp
public async Task<IActionResult> Create(CreateSaleDto model)
{
    try
    {
        // Business logic
        var result = await _saleService.CreateSaleAsync(model);
        TempData["Success"] = "Sale created successfully!";
        return RedirectToAction(nameof(Details), new { id = result.Id });
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Error creating sale: " + ex.Message;
        // Reload form data
        ViewBag.Products = await _productService.GetActiveProductsAsync();
        return View(model);
    }
}
```

### 4. **Repository Pattern**
```csharp
// Interface
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();  
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}

// Implementation
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    // Implementation...
}
```

---

## 🤖 AI Assistant Guidelines

### When Providing Help for This Project:

#### 1. **Always Consider Existing Architecture**
- Use the established Repository pattern
- Follow the DTO mapping approach  
- Maintain the service layer separation
- Respect the bill-level discount logic

#### 2. **Database Operations**
- Always use Entity Framework with async/await
- Include proper error handling and transactions
- Consider soft deletes (IsDeleted flag)
- Maintain audit fields (CreatedAt, UpdatedAt)

#### 3. **Business Logic Rules**
- **CRITICAL**: Sales use bill-level discounts, NOT individual product discounts
- Stock decrements must be atomic operations
- Customer statistics update after successful sales
- GST calculations follow Indian tax structure
- Invoice numbers must be unique and sequential

#### 4. **Frontend Guidelines**
- Use jQuery for DOM manipulation
- Implement SweetAlert2 for user feedback
- Ensure mobile responsiveness with Bootstrap
- Include bilingual support where applicable
- Real-time calculations for sales and returns

#### 5. **Common Code Patterns**

**Controller Action Pattern:**
```csharp
[HttpPost]
public async Task<IActionResult> ActionName(ModelType model)
{
    if (!ModelState.IsValid)
    {
        // Reload ViewBag data
        return View(model);
    }
    
    try
    {
        var result = await _service.MethodAsync(model);
        TempData["Success"] = "Success message";
        return RedirectToAction("TargetAction", new { id = result.Id });
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Error: " + ex.Message;
        // Reload ViewBag data  
        return View(model);
    }
}
```

**AJAX Endpoint Pattern:**
```csharp
[HttpGet]
public async Task<IActionResult> GetData(int id)
{
    try
    {
        var data = await _service.GetDataAsync(id);
        return Json(data);
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}
```

#### 6. **Calculation Formulas (IMPORTANT)**

**Sales Calculation:**
```csharp
decimal subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
decimal gstAmount = items.Sum(i => (i.Quantity * i.UnitPrice) * (i.GSTRate / 100));
decimal totalBeforeDiscount = subtotal + gstAmount;
decimal discountAmount = totalBeforeDiscount * (discountPercentage / 100);
decimal finalAmount = totalBeforeDiscount - discountAmount;
```

**Return Calculation:**
```csharp
decimal returnSubtotal = returnQty * originalUnitPrice;
decimal returnGST = returnSubtotal * (originalGSTRate / 100);
decimal totalBeforeDiscount = returnSubtotal + returnGST;  
decimal returnDiscount = totalBeforeDiscount * (originalBillDiscountPercentage / 100);
decimal refundAmount = totalBeforeDiscount - returnDiscount;
```

#### 7. **Testing Scenarios to Always Verify**
- Sales with and without customers
- Sales with and without discounts
- Returns for partial quantities
- Stock validation during sales
- Customer statistics updates
- GST calculations for different rates
- Mobile responsiveness
- Bilingual display

---

## ⚠️ Known Issues & Solutions

### 1. **Sale Details Redirect Issue**
**Problem**: After creating sale, redirects to Index instead of Details
**Cause**: Exception in Details action due to returns processing
**Solution**: 
```csharp
// Add proper null checks in Details action
if (sale.Status == "Completed")
{
    try
    {
        var remainingQuantities = await _returnService.GetRemainingReturnableQuantitiesAsync(id);
        // Process returns data safely
    }
    catch (Exception ex)
    {
        // Log error but don't fail the page
        ViewBag.HasReturnableItems = false;
    }
}
```

### 2. **Stock Concurrency Issues**
**Problem**: Multiple users might oversell products
**Solution**: Use atomic SQL updates
```csharp
var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
    "UPDATE Products SET StockQuantity = StockQuantity - {0} WHERE Id = {1} AND StockQuantity >= {2}",
    quantity, productId, quantity);
    
if (rowsAffected == 0) throw new InvalidOperationException("Insufficient stock");
```

### 3. **Discount Calculation Inconsistencies**
**Problem**: Mixed individual and bill-level discount calculations
**Solution**: Always use bill-level discount approach
```csharp
// NEVER use individual discounts in new code
// ALWAYS calculate: subtotal + GST, then apply bill discount percentage
```

### 4. **Customer Statistics Not Updating**
**Problem**: Customer stats don't update after sales
**Solution**: Add customer stats update after successful sale creation
```csharp
if (sale.CustomerId.HasValue)
{
    await UpdateCustomerStatisticsAsync(sale.CustomerId, sale.TotalAmount);
}
```

---

## 🔮 Future Enhancements

### Planned Features:
1. **Reporting Module**: Sales reports, inventory reports, customer analytics
2. **Barcode Support**: Product barcode scanning and label printing
3. **Multi-location**: Support for multiple store locations
4. **Online Integration**: Basic e-commerce website integration
5. **SMS Notifications**: Customer purchase confirmations
6. **Advanced Inventory**: Automated reordering, supplier management
7. **Employee Management**: Multi-user access with role-based permissions
8. **Data Export**: Excel/PDF export for reports
9. **Backup & Restore**: Automated database backup functionality
10. **Cloud Deployment**: Azure/AWS deployment guides

### Technical Debt:
1. **Add Unit Tests**: Comprehensive test coverage for business logic
2. **Implement Caching**: Redis cache for frequently accessed data
3. **Add Logging**: Structured logging with Serilog
4. **API Versioning**: Prepare for mobile app integration
5. **Performance Optimization**: Query optimization and pagination
6. **Security Enhancements**: HTTPS enforcement, data encryption

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines:
- Follow existing code conventions
- Include unit tests for new features
- Update documentation for significant changes
- Test on both desktop and mobile devices
- Verify GST calculations are correct
- Test with sample Indian retail scenarios

---

## 📞 Support

For support and questions:
- **Email**: support@khadistore.com
- **Documentation**: [Wiki](https://github.com/yourusername/KhadiStore/wiki)
- **Issues**: [GitHub Issues](https://github.com/yourusername/KhadiStore/issues)

---

## 🏷️ Version History

### v1.0.0 (Current)
- Complete sales and inventory management
- Customer auto-creation and statistics
- Returns and refund processing
- Bill-level discounting system
- GST compliant invoicing
- Mobile-responsive interface
- Bilingual support (Hindi/English)

---

**Built with ❤️ for Indian Traditional Retail Businesses**

*KhadiStore - Modernizing Traditional Commerce* 🇮🇳
