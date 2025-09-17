using KhadiStore.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhadiStore.Application.Interfaces
{
    public interface ISupplierService
    {
        // Basic CRUD operations
        Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync();
        Task<IEnumerable<SupplierDto>> GetActiveSuppliersAsync();
        Task<SupplierDto?> GetSupplierByIdAsync(int id);
        Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto);
        Task<bool> UpdateSupplierAsync(UpdateSupplierDto updateSupplierDto);
        Task<bool> DeleteSupplierAsync(int id);
        Task<bool> ToggleSupplierStatusAsync(int id);

        // Search and filtering
        Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string searchTerm);
        Task<IEnumerable<SupplierDto>> GetSuppliersByLocationAsync(string city, string state);
        Task<SupplierDto?> GetSupplierByGSTNumberAsync(string gstNumber);

        // Pagination
        Task<PagedResult<SupplierDto>> GetPagedSuppliersAsync(SupplierFilterDto filter);
        Task<int> GetSuppliersCountAsync(SupplierFilterDto filter);

        // Validation
        Task<bool> SupplierExistsAsync(string name, int excludeId = 0);
        Task<bool> GSTNumberExistsAsync(string gstNumber, int excludeId = 0);

        // Statistics
        Task<int> GetTotalSuppliersCountAsync();
        Task<int> GetActiveSuppliersCountAsync();
        Task<IEnumerable<SupplierDto>> GetTopSuppliersByPurchaseAmountAsync(int count = 10);
    }
}
