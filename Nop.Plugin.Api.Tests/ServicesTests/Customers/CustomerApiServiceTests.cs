using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Nop.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.DTO.Customers;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests.Customers
{
    // Possible tests
    // 

    //[TestFixture]
    //public class CustomerApiServiceTests
    //{
    //    private ICustomerApiService _customerApiService;

    //    [SetUp]
    //    public new void SetUp()
    //    {
    //        var customerRepositoryStub = Substitute.For<IRepository<Customer>>();
    //        var genericAttributeRepositoryStub = Substitute.For<IRepository<GenericAttribute>>();

    //        customerRepositoryStub.Stub(x => x.Table).Return((new List<Customer>()
    //        {
    //            new Customer()
    //            {
    //                Id = 1,
    //                Email = "test@customer1.com"
    //            },
    //            new Customer()
    //            {
    //                Id = 2,
    //                Email = "test@customer2.com"
    //            }

    //        }).AsQueryable());

    //        genericAttributeRepositoryStub.Stub(x => x.Table).Return((new List<GenericAttribute>()
    //        {
    //            new GenericAttribute()
    //            {
    //                Id = 1,
    //                KeyGroup = "Customer",
    //                Key = "FirstName",
    //                Value = "first name 1",
    //                EntityId = 1
    //            },
    //            new GenericAttribute()
    //            {
    //                Id = 2,
    //                KeyGroup = "Customer",
    //                Key = "LastName",
    //                Value = "last name 1",
    //                EntityId = 1
    //            },

    //            new GenericAttribute()
    //            {
    //                Id = 3,
    //                KeyGroup = "Customer",
    //                Key = "FirstName",
    //                Value = "first name 2",
    //                EntityId = 2
    //            },
    //            new GenericAttribute()
    //            {
    //                Id = 3,
    //                KeyGroup = "Customer",
    //                Key = "LastName",
    //                Value = "last name 2",
    //                EntityId = 2
    //            }

    //        }).AsQueryable());

    //        _customerApiService = new CustomerApiService(customerRepositoryStub, genericAttributeRepositoryStub);

    //        // Needed because the tests don't invoke the dependency register and the type maps are never registered.
    //        Mapper.CreateMap<Customer, CustomerDto>();
    //    }

    //    [Test]
    //    public void Get_customers_call_with_default_parameters()
    //    {
    //        var customersResult = _customerApiService.GetCustomersDtos();

    //        ClassicAssert.IsNotNull(customersResult);
    //        ClassicAssert.IsNotEmpty(customersResult);
    //        ClassicAssert.AreEqual(2, customersResult.Count);
    //        ClassicAssert.AreEqual("test@customer1.com", customersResult[0].Email);            
    //        ClassicAssert.AreEqual("test@customer2.com", customersResult[1].Email);
    //    }

    //    [Test]
    //    public void Get_customers_limit_set_to_1()
    //    {
    //        var customersResult = _customerApiService.GetCustomersDtos(limit: 1);

    //        ClassicAssert.IsNotNull(customersResult);
    //        ClassicAssert.IsNotEmpty(customersResult);
    //        ClassicAssert.AreEqual(1, customersResult.Count);
    //        ClassicAssert.AreEqual("test@customer1.com", customersResult[0].Email);
    //    }
    //}
}