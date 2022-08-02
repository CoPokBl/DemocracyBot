using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using GeneralPurposeLib;

namespace DemocracyBot.Data; 

public static class LinkScannerService {
    private static int _submitRequestsRemaining = 1;
    private static DateTime _nextRatelimitReset = DateTime.Now;
    
    private const int ScoreLinkRetrievalTimeout = 600;

    public static async Task<int> GetLinkScore(string uuid) {
        int secondsWaited = 0;
        int secondsToNextCheck = 10;

        while (secondsWaited < ScoreLinkRetrievalTimeout) {
            Thread.Sleep(1000);
            secondsWaited++;
            secondsToNextCheck--;

            if (secondsToNextCheck >= 1) continue;
            
            // Check to see if the results are in
            HttpClient http = new();
            http.DefaultRequestHeaders.Add("API-Key", Program.Config!["urlscan-api-key"]);
        
            string content = "";
            HttpResponseMessage? response = null;
            HttpStatusCode? statusCode = null;
            while (statusCode is null or HttpStatusCode.Redirect or HttpStatusCode.Moved) {
                try {
            
                    response = await http.GetAsync("https://urlscan.io/api/v1/result/" + uuid);
                    content = await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException e) {
                    throw new LinkScanHttpRequestFailed(e.Message);
                }
                catch (TaskCanceledException e) {
                    throw new LinkScanHttpRequestFailed(e.Message);
                }

                statusCode = response.StatusCode;
            }

            if (response == null) {
                throw new LinkScanHttpRequestFailed("No response from server");
            }

            if (statusCode == HttpStatusCode.NotFound) {
                // The link has not been scanned yet
                continue;
            }
            
            JsonDocument json = JsonDocument.Parse(content);
            int score = json.RootElement
                .GetProperty("verdicts")
                .GetProperty("overall")
                .GetProperty("score")
                .GetInt32();

            return score;
        }

        // Timeout
        throw new LinkScanTimeoutException("Timeout while waiting for link score");
    }

    public static async Task<string> SubmitLink(string url) {
        if (_submitRequestsRemaining < 1) {
            if (_nextRatelimitReset < DateTime.Now) {
                _submitRequestsRemaining = 1;
            }
            else {
                throw new LinkScanRateLimitException("Ratelimit was exceeded");
            }
        }
        
        // Build the request body
        var requestBody = new {
            url,
            visibility = "unlisted",
            tags = new[] {
                "SerbleBot"
            }
        };
        string requestBodyJsonString = JsonSerializer.Serialize(requestBody);
        
        HttpClient http = new();
        http.DefaultRequestHeaders.Add("API-Key", Program.Config!["urlscan-api-key"]);
        StringContent reqContent = new StringContent(requestBodyJsonString);
        reqContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        string content = "";
        HttpResponseMessage? response = null;
        HttpStatusCode? statusCode = null;
        while (statusCode is null or HttpStatusCode.Redirect or HttpStatusCode.Moved) {
            try {
            
                response = await http.PostAsync("https://urlscan.io/api/v1/scan/", reqContent);
                content = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e) {
                throw new LinkScanHttpRequestFailed(e.Message);
            }
            catch (TaskCanceledException e) {
                throw new LinkScanHttpRequestFailed(e.Message);
            }

            statusCode = response.StatusCode;
        }

        if (response == null) {
            throw new LinkScanHttpRequestFailed("No response from server");
        }


        if (response.StatusCode == HttpStatusCode.TooManyRequests) {
            // Ratelimit was exceeded
            _submitRequestsRemaining = 0;
            throw new LinkScanRateLimitException("Ratelimit was exceeded");
        }
        
        
        if (!response.IsSuccessStatusCode) {
            Logger.Error("Link Scan request failed with status code " + response.StatusCode + " and content " + content);
            Logger.Error("Request body: " + requestBodyJsonString);
            throw new LinkScanHttpRequestFailed(response.StatusCode.ToString());
        }
        
        // Parse the response
        JsonDocument? responseBody = JsonDocument.Parse(content);
        if (responseBody == null) {
            throw new LinkScanHttpRequestFailed("Response body was null");
        }
        
        // Get remaining requests
        if (!response.Headers.Contains("X-Rate-Limit-Remaining")) {
            // No ratelimit header
            Logger.Warn("No ratelimit header in link scan response (Submit)");
        }
        else {
            string? remainingRequests = response.Headers.GetValues("X-Rate-Limit-Remaining").FirstOrDefault();
            _submitRequestsRemaining = int.Parse(remainingRequests!);
        }
        
        // Get ratelimit reset time
        if (!response.Headers.Contains("X-Rate-Limit-Reset-After")) {
            // No ratelimit header
            Logger.Warn("No ratelimit header in link scan response (Submit)");
        }
        else {
            string? remainingRequests = response.Headers.GetValues("X-Rate-Limit-Reset-After").FirstOrDefault();
            _nextRatelimitReset = DateTime.Now.AddSeconds(int.Parse(remainingRequests!));
        }
        
        JsonElement uuidElement = responseBody.RootElement.GetProperty("uuid");
        string? uuid = uuidElement.GetString();
        if (uuid == null) {
            throw new LinkScanUnknownLinkScanFailException("UUID was null");
        }

        return uuid;
    }
    
}

public class LinkScanUnknownLinkScanFailException : Exception {
    public LinkScanUnknownLinkScanFailException(string message) : base(message) { }
}

public class LinkScanHttpRequestFailed : Exception {
    public LinkScanHttpRequestFailed(string message) : base(message) { }
}

public class LinkScanRateLimitException : Exception {
    public LinkScanRateLimitException(string message) : base(message) { }
}

public class LinkScanTimeoutException : Exception {
    public LinkScanTimeoutException(string message) : base(message) { }
}