#define GITHUB_UPDATE_TEST_MODE__OFF

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NZ.RdpMaid.App.Core.Utils;
using NZ.RdpMaid.App.Extensions.Versioning;
using NZ.RdpMaid.App.SerializationModels.AtomRss;

namespace NZ.RdpMaid.App.Core.Services.HttpClients
{
    internal class GithubUpdateClient(HttpClient client)
    {
        public static class Urls
        {
            public const string ReleaseFeed = "https://github.com/nikitazu/NZ.RdpMaid/releases.atom";
            public const string ReleaseDownload = "https://github.com/nikitazu/NZ.RdpMaid/releases/download/";
        }

        public enum CheckStatus
        {
            UpToDate,
            UpdateFound,
            Failed,
        }

        public record CheckResponse(
            CheckStatus Status,
            UpdateModel? FoundUpdate = null,
            string? Error = null
        );

        public enum DownloadStatus
        {
            Ok,
            Failed,
        }

        public record DownloadResponse(
            DownloadStatus Status,
            byte[] Data,
            string? Error = null
        );

        private readonly HttpClient _client = client ?? throw new ArgumentNullException(nameof(client));

        public async Task<CheckResponse> CheckForUpdates(Version current, CancellationToken ct = default)
        {
            var content = ExampleData.Feed;

#if GITHUB_UPDATE_TEST_MODE__ON
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
#else
            var response = await _client.GetAsync(Urls.ReleaseFeed, ct);
            if (!response.IsSuccessStatusCode)
            {
                return new CheckResponse(Status: CheckStatus.Failed, Error: $"Плохой ответ {response.StatusCode}");
            }

            content = await response.Content.ReadAsStringAsync();
#endif

            if (string.IsNullOrWhiteSpace(content))
            {
                return new CheckResponse(Status: CheckStatus.Failed, Error: "Пустой ответ");
            }

            if (!XDocumentUtil.TryParse(content, out var xml))
            {
                return new CheckResponse(Status: CheckStatus.Failed, Error: "Не удалось разобрать XML ответ");
            }

            if (xml.Root is null)
            {
                return new CheckResponse(Status: CheckStatus.Failed, Error: "В ответе пустой XML");
            }

            List<UpdateModel>? updates = null;

            try
            {
                updates = AtomRssParser.ParseFeed(xml.Root)
                    .Select(UpdateModelFactory.CreateFromAtomRss)
                    .OrderByDescending(entry => entry.Version)
                    .ToList();
            }
            catch (Exception ex)
            {
                return new CheckResponse(Status: CheckStatus.Failed, Error: $"Некорректный XML: {ex.Message}");
            }

            if (!updates.Any())
            {
                return new CheckResponse(Status: CheckStatus.UpToDate);
            }

            var latestUpdate = updates.First();

            if (latestUpdate.Version <= current)
            {
                return new CheckResponse(Status: CheckStatus.UpToDate);
            }

            if (latestUpdate.Author != "nikitazu")
            {
                return new CheckResponse(
                    Status: CheckStatus.Failed,
                    Error: $"Неизвестный автор обновления: {latestUpdate.Author}!"
                );
            }

            return new CheckResponse(Status: CheckStatus.UpdateFound, FoundUpdate: latestUpdate);
        }

        public async Task<DownloadResponse> DownloadUpdate(
            UpdateModel update,
            IProgress<float> progress,
            CancellationToken ct = default)
        {
            var downloadUrl = MakeDownloadUrl(update.Version.ToXyzString());
            var response = await _client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!response.IsSuccessStatusCode)
            {
                return new DownloadResponse(DownloadStatus.Failed, Data: [], Error: $"Ошибка загрузки: {response.StatusCode}");
            }

            var contentLength = response.Content.Headers.ContentLength;

            using var downstream = await response.Content.ReadAsStreamAsync(ct);
            using var buffer = new MemoryStream();

            if (!contentLength.HasValue || contentLength.Value == 0)
            {
                await downstream.CopyToAsync(buffer);
                progress.Report(100);
            }
            else
            {
                var relativeProgress = new Progress<long>(
                    byteCount => progress.Report((float)byteCount / contentLength.Value)
                );

                var buf = new byte[2048];
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await downstream.ReadAsync(buf, ct)) != 0)
                {
                    await buffer.WriteAsync(buf.AsMemory(0, bytesRead), ct);
                    totalBytesRead += bytesRead;
                    ((IProgress<long>)relativeProgress).Report(totalBytesRead);
                }
            }

            return new DownloadResponse(DownloadStatus.Ok, buffer.ToArray());
        }

        private static string MakeDownloadUrl(string version) =>
            $"{Urls.ReleaseDownload}{version}/NZ.RdpMaid.App-v{version}.zip";
    }
}