using System;
using Autofac.Extras.Moq;
using Moq;

namespace Nop.Plugin.Api.Tests.Helpers
{
    /// <summary>
    /// Compatibility adapter to replace the old RhinoAutoMocker from AutoMock package.
    /// Wraps Autofac.Extras.Moq.AutoMock to provide similar API.
    /// </summary>
    /// <typeparam name="T">The class under test</typeparam>
    public class RhinoAutoMocker<T> : IDisposable where T : class
    {
        private readonly AutoMock _autoMock;
        private T _classUnderTest;

        public RhinoAutoMocker()
        {
            _autoMock = AutoMock.GetLoose();
        }

        /// <summary>
        /// Gets the class under test with all dependencies auto-mocked.
        /// </summary>
        public T ClassUnderTest
        {
            get
            {
                if (_classUnderTest == null)
                {
                    _classUnderTest = _autoMock.Create<T>();
                }
                return _classUnderTest;
            }
        }

        /// <summary>
        /// Gets the mock for a dependency of type TService.
        /// Returns a Moq.Mock wrapper that provides .Setup() method.
        /// </summary>
        public MockWrapper<TService> Get<TService>() where TService : class
        {
            return new MockWrapper<TService>(_autoMock.Mock<TService>());
        }

        public void Dispose()
        {
            _autoMock?.Dispose();
        }
    }

    /// <summary>
    /// Wrapper around Moq.Mock to provide RhinoMocks-style .Stub() syntax.
    /// </summary>
    public class MockWrapper<T> where T : class
    {
        private readonly Mock<T> _mock;

        public MockWrapper(Mock<T> mock)
        {
            _mock = mock;
        }

        /// <summary>
        /// Provides stub setup - converts to Moq's Setup().Returns() pattern.
        /// Usage: mock.Stub(x => x.Method(args)).Return(value)
        /// </summary>
        public MockSetup<T, TResult> Stub<TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> expression)
        {
            return new MockSetup<T, TResult>(_mock.Setup(expression));
        }
    }

    /// <summary>
    /// Wrapper to provide .Return() and .IgnoreArguments() methods in RhinoMocks style.
    /// </summary>
    public class MockSetup<T, TResult> where T : class
    {
        private readonly Moq.Language.Flow.ISetup<T, TResult> _setup;
        private bool _ignoreArguments;

        public MockSetup(Moq.Language.Flow.ISetup<T, TResult> setup)
        {
            _setup = setup;
        }

        public MockSetup<T, TResult> IgnoreArguments()
        {
            _ignoreArguments = true;
            return this;
        }

        public void Return(TResult value)
        {
            _setup.Returns(value);
        }
    }

    /// <summary>
    /// Argument matchers to replace RhinoMocks Arg<T>.Is.Anything pattern.
    /// In Moq, we use It.IsAny<T>() instead.
    /// </summary>
    public static class Arg<T>
    {
        public static class Is
        {
            public static T Anything => It.IsAny<T>();
        }
    }
}
