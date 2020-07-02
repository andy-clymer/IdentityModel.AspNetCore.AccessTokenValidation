using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Tests.Infrastructure;
using Xunit;

namespace Tests
{
    public class DispatchingTests
    {
        [Fact]
        public async Task no_handlers_and_no_token_should_401()
        {
            var server = new Server();
            var client = server.CreateClient();
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task no_handlers_default_selector_and_unknown_token_should_401()
        {
            var server = new Server();
            var client = server.CreateClient();
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
            request.Headers.Authorization = new AuthenticationHeaderValue("unknown", "token");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task no_handlers_default_selector_and_JWT_token_should_throw()
        {
            var server = new Server();
            // server.AccessTokenOptions = o =>
            // {
            //     o.SchemeSelector = context => "test";
            // };
            
            var client = server.CreateClient();
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", "header.payload.signature");

            Func<Task> act = async () => { await client.SendAsync(request); };
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        
        [Fact]
        public async Task handlers_and_token_should_200()
        {
            var server = new Server();
            server.AddTestHandler = true;
            
            server.AccessTokenOptions = o =>
            {
                o.SchemeSelector = context => "test";
            };
            
            var client = server.CreateClient();
            
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api");
            request.Headers.Authorization = new AuthenticationHeaderValue("prefix", "token");

            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}