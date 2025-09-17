using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Domain.Entities;

namespace KhadiStore.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Product Mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock))
                .ForMember(dest => dest.PriceWithGST, opt => opt.MapFrom(src => src.PriceWithGST))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.DisplayName));

            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.SaleItems, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseItems, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.SaleItems, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseItems, opt => opt.Ignore());

            // Customer Mappings
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.CustomerType, opt => opt.MapFrom(src => src.CustomerType.ToString()))
                .ForMember(dest => dest.IsWholesale, opt => opt.MapFrom(src => src.IsWholesale));

            CreateMap<CreateCustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPurchases, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalOrders, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Sales, opt => opt.Ignore());

            // Category Mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));

            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // Supplier Mappings
            // Supplier mappings
            CreateMap<Supplier, SupplierDto>();

            CreateMap<CreateSupplierDto, Supplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Purchases, opt => opt.Ignore());

            CreateMap<UpdateSupplierDto, Supplier>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Purchases, opt => opt.Ignore());

            // Purchase mappings
            CreateMap<Purchase, PurchaseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : ""))
                .ForMember(dest => dest.SupplierPhone, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Phone : ""))
                .ForMember(dest => dest.SupplierEmail, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Email : ""))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.PurchaseItems.Sum(pi => pi.Quantity)));

            CreateMap<CreatePurchaseDto, Purchase>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseOrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.GSTAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore());

            // Purchase Item mappings
            CreateMap<PurchaseItem, PurchaseItemDto>()
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.TotalAmount));

            CreateMap<CreatePurchaseItemDto, PurchaseItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.GSTRate, opt => opt.Ignore())
                .ForMember(dest => dest.GSTAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Purchase, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Sale Mappings
            CreateMap<Sale, SaleDto>()
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TotalItems, opt => opt.MapFrom(src => src.TotalItems))
                .ForMember(dest => dest.SaleItems, opt => opt.MapFrom(src => src.SaleItems));

            CreateMap<CreateSaleDto, Sale>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.GSTAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => SaleStatus.Completed))
                .ForMember(dest => dest.Customer, opt => opt.Ignore());

            // Sale Item Mappings
            CreateMap<SaleItem, SaleItemDto>()
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.LineTotal));

            CreateMap<CreateSaleItemDto, SaleItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.SaleId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.GSTRate, opt => opt.Ignore())
                .ForMember(dest => dest.GSTAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Sale, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            // Dashboard Mappings
            CreateMap<Product, LowStockProductDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.MinStockLevel, opt => opt.MapFrom(src => src.MinStockLevel));

            // Return Entity to ReturnDto
            CreateMap<Return, ReturnDto>()
                .ForMember(dest => dest.Sale, opt => opt.MapFrom(src => src.Sale))
                .ForMember(dest => dest.Customer, opt => opt.MapFrom(src => src.Customer))
                .ForMember(dest => dest.ReturnItems, opt => opt.MapFrom(src => src.ReturnItems));

            // ReturnDto to Return Entity
            CreateMap<ReturnDto, Return>()
                .ForMember(dest => dest.Sale, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnItems, opt => opt.MapFrom(src => src.ReturnItems));

            // CreateReturnDto to Return Entity
            CreateMap<CreateReturnDto, Return>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnNumber, opt => opt.Ignore()) // Generated in service
                .ForMember(dest => dest.ReturnDate, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Set from sale
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.GSTAmount, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Sale, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnItems, opt => opt.Ignore()); // Handled separately in service

            // ReturnItem Entity to ReturnItemDto
            CreateMap<ReturnItem, ReturnItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName));

            // ReturnItemDto to ReturnItem Entity
            CreateMap<ReturnItemDto, ReturnItem>()
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.SaleItem, opt => opt.Ignore())
                .ForMember(dest => dest.Return, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // CreateReturnItemDto to ReturnItem Entity
            CreateMap<CreateReturnItemDto, ReturnItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnId, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.ProductName, opt => opt.Ignore()) // Set from original sale item
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore()) // Set from original sale item
                .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.GSTRate, opt => opt.Ignore()) // Set from original sale item
                .ForMember(dest => dest.GSTAmount, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.LineTotal, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.SaleItem, opt => opt.Ignore())
                .ForMember(dest => dest.Return, opt => opt.Ignore());

            CreateMap<CreateReturnItemDto, ReturnItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.GSTRate, opt => opt.Ignore())
                .ForMember(dest => dest.GSTAmount, opt => opt.Ignore())
                .ForMember(dest => dest.LineTotal, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Return, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.SaleItem, opt => opt.Ignore());
        }
    }
}