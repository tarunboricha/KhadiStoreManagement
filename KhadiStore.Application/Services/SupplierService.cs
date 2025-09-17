using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Application.Interfaces;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhadiStore.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync()
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
                var supplierDtos = _mapper.Map<IEnumerable<SupplierDto>>(suppliers);

                // Populate statistics
                foreach (var supplierDto in supplierDtos)
                {
                    var purchases = await _unitOfWork.Purchases.GetBySupplierAsync(supplierDto.Id);
                    supplierDto.TotalPurchases = purchases.Count();
                    supplierDto.TotalPurchaseAmount = purchases.Where(p => p.Status == PurchaseStatus.Received).Sum(p => p.TotalAmount);
                }

                return supplierDtos;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving suppliers: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SupplierDto>> GetActiveSuppliersAsync()
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.GetActiveAsync();
                return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving active suppliers: {ex.Message}", ex);
            }
        }

        public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                    return null;

                var supplierDto = _mapper.Map<SupplierDto>(supplier);

                // Populate statistics
                supplierDto.TotalPurchases = supplier.Purchases.Count;
                supplierDto.TotalPurchaseAmount = supplier.Purchases.Where(p => p.Status == PurchaseStatus.Received).Sum(p => p.TotalAmount);

                return supplierDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving supplier: {ex.Message}", ex);
            }
        }

        public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto)
        {
            try
            {
                // Validate supplier name uniqueness
                if (await SupplierExistsAsync(createSupplierDto.Name))
                {
                    throw new InvalidOperationException($"Supplier with name '{createSupplierDto.Name}' already exists.");
                }

                // Validate GST number uniqueness if provided
                if (!string.IsNullOrWhiteSpace(createSupplierDto.GSTNumber))
                {
                    if (await GSTNumberExistsAsync(createSupplierDto.GSTNumber))
                    {
                        throw new InvalidOperationException($"Supplier with GST number '{createSupplierDto.GSTNumber}' already exists.");
                    }
                }

                var supplier = _mapper.Map<Supplier>(createSupplierDto);
                supplier.CreatedAt = DateTime.UtcNow;

                var createdSupplier = await _unitOfWork.Suppliers.AddAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<SupplierDto>(createdSupplier);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating supplier: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateSupplierAsync(UpdateSupplierDto updateSupplierDto)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(updateSupplierDto.Id);
                if (supplier == null)
                    return false;

                // Validate supplier name uniqueness (excluding current supplier)
                if (await SupplierExistsAsync(updateSupplierDto.Name, updateSupplierDto.Id))
                {
                    throw new InvalidOperationException($"Another supplier with name '{updateSupplierDto.Name}' already exists.");
                }

                // Validate GST number uniqueness if provided (excluding current supplier)
                if (!string.IsNullOrWhiteSpace(updateSupplierDto.GSTNumber))
                {
                    if (await GSTNumberExistsAsync(updateSupplierDto.GSTNumber, updateSupplierDto.Id))
                    {
                        throw new InvalidOperationException($"Another supplier with GST number '{updateSupplierDto.GSTNumber}' already exists.");
                    }
                }

                _mapper.Map(updateSupplierDto, supplier);

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating supplier: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                    return false;

                // Check if supplier has any purchases
                var purchases = await _unitOfWork.Purchases.GetBySupplierAsync(id);
                if (purchases.Any())
                {
                    // Soft delete - just mark as inactive
                    supplier.IsActive = false;
                }
                else
                {
                    // Hard delete if no purchases
                    await _unitOfWork.Suppliers.DeleteAsync(supplier.Id);
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting supplier: {ex.Message}", ex);
            }
        }

        public async Task<bool> ToggleSupplierStatusAsync(int id)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
                if (supplier == null)
                    return false;

                supplier.IsActive = !supplier.IsActive;

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error toggling supplier status: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string searchTerm)
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.SearchAsync(searchTerm);
                return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching suppliers: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SupplierDto>> GetSuppliersByLocationAsync(string city, string state)
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.GetByLocationAsync(city, state);
                return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving suppliers by location: {ex.Message}", ex);
            }
        }

        public async Task<SupplierDto?> GetSupplierByGSTNumberAsync(string gstNumber)
        {
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByGSTNumberAsync(gstNumber);
                return supplier != null ? _mapper.Map<SupplierDto>(supplier) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving supplier by GST number: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<SupplierDto>> GetPagedSuppliersAsync(SupplierFilterDto filter)
        {
            try
            {
                var totalCount = await _unitOfWork.Suppliers.GetCountAsync(filter.Name, filter.City, filter.State, filter.IsActive);
                var suppliers = await _unitOfWork.Suppliers.GetPagedAsync(filter.Page, filter.PageSize, filter.Name, filter.City, filter.State, filter.IsActive);
                var supplierDtos = _mapper.Map<IEnumerable<SupplierDto>>(suppliers);

                return new PagedResult<SupplierDto>
                {
                    Items = supplierDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paged suppliers: {ex.Message}", ex);
            }
        }

        public async Task<int> GetSuppliersCountAsync(SupplierFilterDto filter)
        {
            try
            {
                return await _unitOfWork.Suppliers.GetCountAsync(filter.Name, filter.City, filter.State, filter.IsActive);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting suppliers count: {ex.Message}", ex);
            }
        }

        public async Task<bool> SupplierExistsAsync(string name, int excludeId = 0)
        {
            try
            {
                return await _unitOfWork.Suppliers.ExistsAsync(name, excludeId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking supplier existence: {ex.Message}", ex);
            }
        }

        public async Task<bool> GSTNumberExistsAsync(string gstNumber, int excludeId = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gstNumber))
                    return false;

                var supplier = await _unitOfWork.Suppliers.GetByGSTNumberAsync(gstNumber);
                return supplier != null && supplier.Id != excludeId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking GST number existence: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalSuppliersCountAsync()
        {
            try
            {
                return await _unitOfWork.Suppliers.GetTotalSuppliersAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting total suppliers count: {ex.Message}", ex);
            }
        }

        public async Task<int> GetActiveSuppliersCountAsync()
        {
            try
            {
                return await _unitOfWork.Suppliers.GetActiveSuppliersAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting active suppliers count: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<SupplierDto>> GetTopSuppliersByPurchaseAmountAsync(int count = 10)
        {
            try
            {
                var suppliers = await _unitOfWork.Suppliers.GetTopSuppliersByPurchaseAmountAsync(count);
                return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting top suppliers: {ex.Message}", ex);
            }
        }
    }
}
