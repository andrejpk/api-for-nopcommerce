using System.Collections.Generic;
using System.Linq;
using Nop.Data;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.Services;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using NSubstitute;

namespace Nop.Plugin.Api.Tests.ServicesTests
{
    //[TestFixture]
    //public class CountryApiServiceTests
    //{
    //    private ICountryApiService _countryApiService;

    //    [SetUp]
    //    public new void SetUp()
    //    {
    //        var countryRepositoryStub = Substitute.For<IRepository<Country>>();

    //        countryRepositoryStub.Stub(x => x.Table).Return((new List<Country>()
    //        {

    //            new Country()
    //            {
    //                Name = "test country 1"
    //            },
    //            new Country()
    //            {
    //                Name = "test country 2"
    //            }

    //        }).AsQueryable());

    //        _countryApiService = new CountryApiService(countryRepositoryStub);
    //    }

    //    [Test]
    //    public void Get_country_by_existing_name()
    //    {
    //        var countryResult = _countryApiService.GetCountryByName("test country 1");

    //        ClassicAssert.IsNotNull(countryResult);
    //        ClassicAssert.AreEqual("test country 1", countryResult.Name);
    //    }

    //    [Test]
    //    public void Get_country_by_non_existing_name()
    //    {
    //        var countryResult = _countryApiService.GetCountryByName("non existing country name");

    //        ClassicAssert.IsNull(countryResult);
    //    }
    //}
}