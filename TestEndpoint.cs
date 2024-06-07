using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace TestEndpoint;

public class User {
    public int id { get; set; }
    public string username { get; set; }
    public string passwordHash { get; set; }
    public string role { get; set; } // "Admin" or "User"
    public string ttoken { get; set; }
    public object matches { get; set; }
    public object comments { get; set; }
    public object ratings { get; set; }
}

public class Match
{
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string youTubeLink { get; set; }
    public DateTime dateAdded { get; set; }
    public int userId { get; set; }
    public object user { get; set; }
    public object comments { get; set; }
    public object ratings { get; set; }
    public int likesCount { get; set; }
    public int dislikesCount { get; set; }
}

public class TestEndpoint
{
    public static async Task Main(string[] args)
    {
        var baseAddress = "https://localhost:5001/api/MatchesApi/";
        var username = "admin";
        var token = "ABCDEFGH";
        var client = new HttpClient { BaseAddress = new Uri(baseAddress) };

        client.DefaultRequestHeaders.Add("username", username);
        client.DefaultRequestHeaders.Add("token", token);

        //await GetMatches(client);
        //await GetMatchById(client, 3);
        await PostMatch(client);
        await PutMatch(client, 3);
        //await DeleteMatch(client, 3);
    }

    private static async Task GetMatches(HttpClient client)
    {
        try
        {
            var response = await client.GetAsync("");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var matches = JsonSerializer.Deserialize<IEnumerable<Match>>(jsonString);

            Console.WriteLine("Get all matches:");
            if (matches != null)
            {
                foreach (var match in matches)
                {
                    Console.WriteLine($"Match Title: {match.title ?? "N/A"}, Link: {match.youTubeLink ?? "N/A"}");
                }
            }
            else
            {
                Console.WriteLine("No matches found.");
            }
            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
        }
    }

    private static async Task GetMatchById(HttpClient client, int id)
    {
        try
        {
            var response = await client.GetAsync($"{id}");
            response.EnsureSuccessStatusCode();

            var match = await response.Content.ReadFromJsonAsync<Match>();
            if (match != null)
            {
                Console.WriteLine($"Get match by ID ({id}):");
                Console.WriteLine($"Match Title: {match.title ?? "N/A"}, Link: {match.youTubeLink ?? "N/A"}");
            }
            else
            {
                Console.WriteLine($"Match with ID ({id}) not found.");
            }
            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
        }
    }

    private static async Task<int> GetUserId(HttpClient client)
    {
        var response = await client.GetAsync("Users/GetByUsername");
        response.EnsureSuccessStatusCode(); // Throw if not a success code.
        var user = await response.Content.ReadFromJsonAsync<User>();
        return user.id;
    }

    private static async Task PostMatch(HttpClient client)
    {
        int userId = await GetUserId(client); // Retrieve the user ID based on the username and token.

        var newMatch = new Match
        {
            title = "New Match",
            description = "New Description",
            youTubeLink = "https://www.youtube.com/embed/P0vKj-sRiH0?si=R8zgXTnOpNlyskSt",
            dateAdded = DateTime.Now,
            userId = userId // Set the userId to the retrieved user ID.
        };

        try
        {
            Console.WriteLine("Posting new match:");
            Console.WriteLine(JsonSerializer.Serialize(newMatch));

            var response = await client.PostAsJsonAsync("", newMatch);
            response.EnsureSuccessStatusCode(); // Throw if not a success code.

            var createdMatch = await response.Content.ReadFromJsonAsync<Match>();
            Console.WriteLine("Posted a new match:");
            Console.WriteLine($"Match Title: {createdMatch?.title ?? "N/A"}, Description: {createdMatch?.description ?? "N/A"}");
            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
        }
    }

    private static async Task PutMatch(HttpClient client, int id)
    {
        int userId = await GetUserId(client); // Retrieve the user ID based on the username and token.

        var updatedMatch = new Match
        {
            id = id,
            title = "Updated Match",
            description = "Updated Description",
            youTubeLink = "https://www.youtube.com/embed/P0vKj-sRiH0?si=R8zgXTnOpNlyskSt",
            dateAdded = DateTime.Now,
            userId = userId // Set the userId to the retrieved user ID.
        };

        try
        {
            Console.WriteLine($"Updating match with ID {id}:");
            Console.WriteLine(JsonSerializer.Serialize(updatedMatch));

            var response = await client.PutAsJsonAsync($"{id}", updatedMatch);
            response.EnsureSuccessStatusCode(); // Throw if not a success code.

            Console.WriteLine($"Updated match with ID ({id}):");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
    }

    private static async Task DeleteMatch(HttpClient client, int id)
    {
        try
        {
            var response = await client.DeleteAsync($"{id}");
            response.EnsureSuccessStatusCode();

            Console.WriteLine($"Deleted match with ID ({id}):");
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
        }
    }
}
