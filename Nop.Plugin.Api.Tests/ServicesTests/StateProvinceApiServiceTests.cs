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
    //public class StateProvinceApiServiceTests
    //{
    //    private IStateProvinceApiService _stateProvinceApiService;

    //    [SetUp]
    //    public new void SetUp()
    //    {
    //        var stateProvinceRepositoryStub = Substitute.For<IRepository<StateProvince>>();

    //        stateProvinceRepositoryStub.Stub(x => x.Table).Return((new List<StateProvince>()
    //        {
    //            new StateProvince()
    //            {
    //                Name = "test state 1"
    //            },
    //            new StateProvince()
    //            {
    //                Name = "test state 2"
    //            }
    //        }).AsQueryable());

    //        _stateProvinceApiService = new StateProvinceApiService(stateProvinceRepositoryStub);
    //    }

    //    [Test]
    //    public void Get_state_by_existing_name()
    //    {
    //        StateProvince state = _stateProvinceApiService.GetStateProvinceByName("test state 1");

    //        ClassicAssert.IsNotNull(state);
    //        ClassicAssert.AreEqual("test state 1", state.Name);
    //    }

    //    [Test]
    //    public void Get_state_by_non_existing_name()
    //    {
    //        StateProvince state = _stateProvinceApiService.GetStateProvinceByName("non existing state name");

    //        ClassicAssert.IsNull(state);
    //    }
    //}
}