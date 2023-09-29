using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;

public class ServerListService
{
    public async Task<int> GetTotalCountAsync(string searchText)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync("https://api.scpsecretlab.pl/lobbylist");
                var data = JsonConvert.DeserializeObject<List<ServerInfo>>(response);

                if (data != null && data.Count > 0)
                {
                    int totalCount = 0;
                    foreach (var server in data)
                    {
                        var infoBase64 = server.Info;
                        var infoBytes = Convert.FromBase64String(infoBase64);
                        var decodedInfo = Encoding.UTF8.GetString(infoBytes);

                        if (decodedInfo.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        {
                            totalCount++;
                        }
                    }

                    Console.WriteLine($"Total Server Count: {totalCount}");

                    return totalCount;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error fetching data: {e}");
        }

        return -1;
    }


    public int CountOccurrences(string text, string searchText)
    {
        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += searchText.Length;
        }

        return count;
    }

    public class ServerInfo
    {
        public string Info { get; set; }
    }
}
