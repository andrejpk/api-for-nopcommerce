using Nop.Plugin.Api.Converters;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Nop.Plugin.Api.Tests.ConvertersTests.ApiTypeConverter
{
    [TestFixture]
    public class ApiTypeConverterTests_ToStatus
    {
        private IApiTypeConverter _apiTypeConverter;

        [SetUp]
        public void SetUp()
        {
            _apiTypeConverter = new Converters.ApiTypeConverter();
        }

        [Test]
        [TestCase("invalid status")]
        [TestCase("publicshed")]
        [TestCase("un-published")]
        [TestCase("322345")]
        [TestCase("%^)@*%&*@_!+=")]
        [TestCase("1")]
        public void WhenInvalidStatusPassed_ShouldReturnNull(string invalidStatus)
        {
            //Arange

            //Act
            bool? result = _apiTypeConverter.ToStatus(invalidStatus);

            //Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void WhenNullOrEmptyStringPassed_ShouldReturnNull(string nullOrEmpty)
        {
            //Arange

            //Act
            bool? result = _apiTypeConverter.ToStatus(nullOrEmpty);

            //Assert
            ClassicAssert.IsNull(result);
        }

        [Test]
        [TestCase("published")]
        [TestCase("Published")]
        [TestCase("PublisheD")]
        public void WhenValidPublishedStatusPassed_ShouldReturnTrue(string validPublishedStatus)
        {
            //Arange

            //Act
            bool? result = _apiTypeConverter.ToStatus(validPublishedStatus);

            //Assert
            ClassicAssert.IsTrue(result.Value);
        }

        [Test]
        [TestCase("unpublished")]
        [TestCase("Unpublished")]
        [TestCase("UnPubLished")]
        public void WhenValidUnpublishedStatusPassed_ShouldReturnFalse(string validUnpublishedStatus)
        {
            //Arange

            //Act
            bool? result = _apiTypeConverter.ToStatus(validUnpublishedStatus);

            //Assert
            ClassicAssert.IsFalse(result.Value);
        }
    }
}