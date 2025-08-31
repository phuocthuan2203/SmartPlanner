using Moq;

namespace SmartPlanner.Tests.Unit
{
    public abstract class TestBase : IDisposable
    {
        protected MockRepository MockRepository { get; }

        protected TestBase()
        {
            MockRepository = new MockRepository(MockBehavior.Strict);
        }

        public virtual void Dispose()
        {
            MockRepository.VerifyAll();
        }
    }
}