using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace CafeSystem.API.IntegrationTests
{
    internal static class IntegrationTestHelpers
    {
        public static async Task<string> LoginAsAdminAsync(HttpClient client)
        {
            object request = new
            {
                email = "admin@admin.com",
                password = "ABC123*"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            JsonElement body = await ReadJsonBodyAsync(response);
            string accessToken = body.GetProperty("accessToken").GetString()!;
            accessToken.Should().NotBeNullOrWhiteSpace();

            return accessToken;
        }

        public static async Task AuthenticateAsAdminAsync(HttpClient client)
        {
            string accessToken = await LoginAsAdminAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        public static async Task<JsonElement> ReadJsonBodyAsync(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync();
            using JsonDocument jsonDocument = JsonDocument.Parse(content);
            return jsonDocument.RootElement.Clone();
        }
    }
}
