using System.Threading.Tasks;
using FluentAssertions;
using OpenServiceBroker.Errors;
using TypedRest;
using Xunit;

namespace OpenServiceBroker.Bindings
{
    public class ServiceBindingBlockingFacts : FactsBase<IServiceBindingBlocking>
    {
        [Fact]
        public async Task Fetch()
        {
            var response = new ServiceBindingResource();

            SetupMock(x => x.FetchAsync("123", "456"), response);
            var result = await Client.ServiceInstancesBlocking["123"].ServiceBindings["456"].FetchAsync();
            result.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Bind()
        {
            var request = new ServiceBindingRequest
            {
                ServiceId = "abc",
                PlanId = "xyz",
                BindResource = new ServiceBindingResourceObject
                {
                    AppGuid = "123-456"
                }
            };
            var response = new ServiceBinding();

            SetupMock(x => x.BindAsync(new ServiceBindingContext("123", "456"), request), response);
            var result = await Client.ServiceInstancesBlocking["123"].ServiceBindings["456"].BindAsync(request);
            result.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task BindUnchanged()
        {
            var request = new ServiceBindingRequest
            {
                ServiceId = "abc",
                PlanId = "xyz"
            };
            var response = new ServiceBinding();

            SetupMock(x => x.BindAsync(new ServiceBindingContext("123", "456"), request), response);
            var result = await Client.ServiceInstancesBlocking["123"].ServiceBindings["456"].BindAsync(request);
            result.Should().BeEquivalentTo(response);
        }

        [Fact]
        public void BindConflict()
        {
            var request = new ServiceBindingRequest
            {
                ServiceId = "abc",
                PlanId = "xyz"
            };

            SetupMock(x => x.BindAsync(new ServiceBindingContext("123", "456"), request), new ConflictException());
            Client.ServiceInstancesBlocking["123"].ServiceBindings["456"].Awaiting(x => x.BindAsync(request)).Should().Throw<ConflictException>();
        }

        [Fact]
        public async Task Unbind()
        {
            SetupMock(x => x.UnbindAsync(new ServiceBindingContext("123", "456"), "abc", "xyz"));
            await Client.ServiceInstancesBlocking["123"].ServiceBindings["456"].UnbindAsync("abc", "xyz");
        }

        [Fact]
        public async Task UnbindBody()
        {
            SetupMock(x => x.UnbindAsync(new ServiceBindingContext("123", "456"), "abc", "xyz"));

            var result = await Client.HttpClient.DeleteAsync(Client.ServiceInstancesBlocking["123"].ServiceBindings["456"].Uri.Join("?service_id=abc&plan_id=xyz"));
            string resultString = await result.Content.ReadAsStringAsync();
            resultString.Should().Be("{}");
        }

        [Fact]
        public void UnbindGone()
        {
            SetupMock(x => x.UnbindAsync(new ServiceBindingContext("123", "456"), "abc", "xyz"), new GoneException());
            Client.ServiceInstancesBlocking["123"].ServiceBindings["456"].Awaiting(x => x.UnbindAsync("abc", "xyz")).Should().Throw<GoneException>();
        }
    }
}
