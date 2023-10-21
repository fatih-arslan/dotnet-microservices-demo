using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionServiceHttpClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public AuctionServiceHttpClient(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        public async Task<List<Item>> GetItemsForSearchDb()
        {
            var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();

            var url = _config["AuctionServiceUrl"];

            return await _client.GetFromJsonAsync<List<Item>>($"{url}/api/auctions?date={lastUpdated}") ?? new List<Item>();
        }
    }
}
