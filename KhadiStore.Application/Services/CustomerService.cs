using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;

namespace KhadiStore.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<CustomerDto?> GetCustomerByPhoneAsync(string phone)
        {
            var customer = await _unitOfWork.Customers.GetByPhoneAsync(phone);
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }

        public async Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync()
        {
            var customers = await _unitOfWork.Customers.GetActiveCustomersAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            var customer = _mapper.Map<Customer>(createCustomerDto);
            customer.CreatedAt = DateTime.UtcNow;

            var createdCustomer = await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerDto>(createdCustomer);
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(CustomerDto updateCustomerDto)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(updateCustomerDto.Id);
            if (existingCustomer == null) return null;

            _mapper.Map(updateCustomerDto, existingCustomer);
            existingCustomer.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Customers.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerDto>(existingCustomer);
        }

        public async Task<CustomerDto> GetCustomerByMobileAsync(string mobile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(mobile))
                    return null;

                var customer = await _unitOfWork.Customers.GetByPhoneAsync(mobile.Trim());
                return _mapper.Map<CustomerDto>(customer);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int customerId)
        {
            try
            {
                var sales = await _unitOfWork.Sales.GetByCustomerAsync(customerId);

                var statistics = new CustomerStatisticsDto
                {
                    CustomerId = customerId,
                    TotalOrders = sales.Count(),
                    TotalPurchases = sales.Sum(s => s.TotalAmount),
                    LastPurchaseDate = sales.Any() ? sales.Max(s => s.SaleDate) : null,
                    FirstPurchaseDate = sales.Any() ? sales.Min(s => s.SaleDate) : null,
                    AverageOrderValue = sales.Any() ? sales.Average(s => s.TotalAmount) : 0
                };

                return statistics;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get customer statistics: {ex.Message}");
            }
        }

        public async Task UpdateCustomerStatisticsAsync(int customerId, decimal saleAmount)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
                if (customer == null)
                    return;

                // Update customer statistics
                customer.TotalOrders++;
                customer.TotalPurchases += saleAmount;

                await _unitOfWork.Customers.UpdateAsync(customer);
            }
            catch (Exception ex)
            {
                // Handle error - you might want to log this
                throw new Exception($"Failed to update customer statistics: {ex.Message}");
            }
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(int id, CreateCustomerDto updateCustomerDto)
        {
            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (existingCustomer == null) return null;

            _mapper.Map(updateCustomerDto, existingCustomer);
            existingCustomer.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Customers.UpdateAsync(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CustomerDto>(existingCustomer);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var result = await _unitOfWork.Customers.DeleteAsync(id);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }

        public async Task<IEnumerable<CustomerDto>> GetPagedCustomersAsync(int pageIndex, int pageSize)
        {
            var customers = await _unitOfWork.Customers.GetPagedAsync(pageIndex, pageSize);
            return _mapper.Map<IEnumerable<CustomerDto>>(customers);
        }
    }
}