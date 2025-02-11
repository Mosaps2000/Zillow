﻿using Zillow.Core.Dto;
using Zillow.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Zillow.Core.Dto.CreateDto;
using Zillow.Core.Dto.UpdateDto;
using Zillow.Data.Data;
using Zillow.Data.DbEntity;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Zillow.Core.Constant;
using Zillow.Core.Exceptions;

namespace Zillow.Service.Services.CustomerServices
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly EntityNotFoundException _notFoundException;
        public CustomerService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _notFoundException = new EntityNotFoundException("Customer");
        }
        public async Task<PagingViewModel> GetAll(int page, int pageSize)
        {
            var pagesCount = (int)Math.Ceiling(await _dbContext.Customer.CountAsync() / (double)pageSize);

            if (page > pagesCount || page < 1)
                page = 1;

            var skipVal = (page - 1) * pageSize;

            var customers = await _dbContext.Customer.Skip(skipVal).Take(pageSize).ToListAsync();

            var customersViewModel = _mapper.Map<List<CustomerViewModel>>(customers);

            return new PagingViewModel()
            {
                CurrentPage = page,
                PagesCount = pagesCount,
                Data = customersViewModel
            };
        }
        public async Task<CustomerViewModel> Get(int id)
        {
            var customer = await _dbContext.Customer
                .SingleOrDefaultAsync(x => x.Id == id);

            if (customer == null) throw _notFoundException;

                return _mapper.Map<CustomerViewModel>(customer);
        }
        public async Task<int> Create(CreateCustomerDto dto, string userId)
        {
            var createdCustomer = _mapper.Map<CustomerDbEntity>(dto);

            createdCustomer.CreatedBy = userId;

            await _dbContext.Customer.AddAsync(createdCustomer);
            await _dbContext.SaveChangesAsync();

            return createdCustomer.Id;
        }
        public async Task<int> Update(int id, UpdateCustomerDto dto, string userId)
        {
            var oldCustomer = await _dbContext.Customer.SingleOrDefaultAsync(x => x.Id == id);

            if (oldCustomer == null) throw _notFoundException;
            
            if (id != dto.Id)
                throw new UpdateEntityException(ExceptionMessage.UpdateEntityIdError);
            
            var updatedCustomer = _mapper.Map(dto, oldCustomer);

            updatedCustomer.UpdatedAt = DateTime.Now;
            updatedCustomer.UpdatedBy = userId;

            _dbContext.Customer.Update(updatedCustomer);
            await _dbContext.SaveChangesAsync();

            return updatedCustomer.Id;
        }
        public async Task<int> Delete(int id, string userId)
        {
            var deletedCustomer = await _dbContext.Customer.SingleOrDefaultAsync(x => x.Id == id);

            if (deletedCustomer == null) throw _notFoundException;
            
            deletedCustomer.UpdatedAt = DateTime.Now;
            deletedCustomer.UpdatedBy = userId;
            deletedCustomer.IsDelete = true;

            _dbContext.Customer.Update(deletedCustomer);
            await _dbContext.SaveChangesAsync();

            return deletedCustomer.Id;
        }



    }
}
