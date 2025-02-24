namespace YourBot.Utils;

public static partial class YourBotUtil {
    /// <summary>
    /// Downloads a file from the specified URL and saves it to the local path.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="savePath">The local path where the file will be saved.</param>
    /// <param name="bufferSize">The size of the buffer (in bytes) used for streaming. Default is 8192 (8KB).</param>
    /// <param name="timeoutMinutes">The timeout for the download operation in minutes. Default is 30 minutes.</param>
    /// <param name="progressCallback">An optional callback to report download progress (downloaded bytes, total bytes).</param>
    /// <returns>A Task that can be awaited until the download completes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when url or savePath is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when bufferSize or timeoutMinutes is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when a network request fails.</exception>
    /// <exception cref="IOException">Thrown when file writing fails.</exception>
    public static async Task SaveFileWithStream(
        string url,
        string savePath,
        int bufferSize = 8192,
        double timeoutMinutes = 30,
        Action<long, long?> progressCallback = null)
    {
        // Validate parameters
        if (string.IsNullOrEmpty(url))
            throw new ArgumentNullException(nameof(url), "URL cannot be null or empty.");
        if (string.IsNullOrEmpty(savePath))
            throw new ArgumentNullException(nameof(savePath), "Save path cannot be null or empty.");
        if (bufferSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be greater than 0.");
        if (timeoutMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutMinutes), "Timeout must be greater than 0.");

        // Use HttpClient to initiate the request
        using (HttpClient client = new HttpClient())
        {
            // Set timeout
            client.Timeout = TimeSpan.FromMinutes(timeoutMinutes);

            try
            {
                // Send GET request and start processing the stream after headers are read
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode(); // Ensure the request is successful

                    // Get total file size for progress reporting (if available)
                    long? totalBytes = response.Content.Headers.ContentLength;
                    long downloadedBytes = 0;

                    // Create directory if it doesn't exist
                    string directory = Path.GetDirectoryName(savePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Create a file stream for writing
                    using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        // Get the response content as a stream
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            byte[] buffer = new byte[bufferSize]; // Use provided buffer size
                            int bytesRead;

                            // Read and write the file in chunks
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                downloadedBytes += bytesRead;

                                // Report progress if callback is provided
                                progressCallback?.Invoke(downloadedBytes, totalBytes);
                            }
                        }
                    }
                }
                Console.WriteLine($"File successfully downloaded to: {savePath}");
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Network error occurred while downloading the file: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to write the file: {ex.Message}", ex);
            }
        }
    }
}