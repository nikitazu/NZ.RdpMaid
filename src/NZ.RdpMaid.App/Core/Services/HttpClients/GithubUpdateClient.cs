#define GITHUB_UPDATE_TEST_MODE__OFF

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NZ.RdpMaid.App.Core.Utils;
using NZ.RdpMaid.App.SerializationModels.AtomRss;

namespace NZ.RdpMaid.App.Core.Services.HttpClients
{
    internal class GithubUpdateClient(HttpClient client)
    {
        public enum CheckStatus
        {
            UpToDate,
            UpdateFound,
            Failed,
        }

        public record CheckResponse(
            CheckStatus Status,
            string? DownloadUrl = null,
            UpdateModel? FoundUpdate = null,
            string? Error = null
        );

        private readonly HttpClient _client = client ?? throw new ArgumentNullException(nameof(client));

        public async Task<CheckResponse> CheckForUpdates(Version current, CancellationToken ct = default)
        {
            string content;

#if GITHUB_UPDATE_TEST_MODE__ON
            content = ExampleData.Feed;
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
#else
            var response = await _client.GetAsync("https://github.com/nikitazu/NZ.RdpMaid/releases.atom", ct);
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

            var version = latestUpdate.Version;
            var versionString = $"{version.Major}.{version.Minor}.{version.Build}";
            var downloadUrl = $"https://github.com/nikitazu/NZ.RdpMaid/releases/download/{versionString}/NZ.RdpMaid.App-v{versionString}.zip";

            return new CheckResponse(
                Status: CheckStatus.UpdateFound,
                DownloadUrl: downloadUrl,
                FoundUpdate: latestUpdate);
        }

        public async Task<byte[]> DownloadUpdate(UpdateModel update, CancellationToken ct = default)
        {
            var version = update.Version;
            var versionString = $"{version.Major}.{version.Minor}.{version.Build}";
            var downloadUrl = $"https://github.com/nikitazu/NZ.RdpMaid/releases/download/{versionString}/NZ.RdpMaid.App-v{versionString}.zip";

            var response = await _client.GetAsync(downloadUrl, ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("fuck");
            }

            var data = await response.Content.ReadAsByteArrayAsync(ct);

            return data;
        }
    }
}