#define GITHUB_UPDATE_TEST_MODE__OFF

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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

            XDocument xml;

            try
            {
                xml = XDocument.Parse(content);
            }
            catch (Exception ex)
            {
                return new CheckResponse(Status: CheckStatus.Failed, Error: $"Не удалось разобрать ответ: {ex.Message}");
            }

            if (xml.Root is null)
            {
                return new CheckResponse(Status: CheckStatus.Failed, Error: $"В ответе пустой XML");
            }

            List<UpdateModel>? updates = null;

            try
            {
                updates = xml.Root.Elements(Schema.Entry)
                    .Select(ParseUpdateEntry)
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

        private static UpdateModel ParseUpdateEntry(XElement xentry)
        {
            var xupdated = xentry.Element(Schema.Updated) ?? throw new FormatException("Отсутствует элемент <updated>");
            var xlink = xentry.Element(Schema.Link) ?? throw new FormatException("Отсутствует элемент <link>");
            var xtitle = xentry.Element(Schema.Title) ?? throw new FormatException("Отсутствует элемент <title>");
            var xauthor = xentry.Element(Schema.Author) ?? throw new FormatException("Отсутствует элемент <author>");
            var xcontent = xentry.Element(Schema.Content) ?? throw new FormatException("Отсутствует элемент <content>");

            var updated = DateTimeOffset.Parse(xupdated.Value);
            var link = xlink.Attribute("href")?.Value ?? string.Empty;
            var title = xtitle.Value ?? throw new FormatException("Элемент <title> пуст");
            var author = xauthor.Value ?? throw new FormatException("Элемент <author> пуст");
            var content = xcontent.Value ?? string.Empty;

            content = content
                .Replace("<h2>", string.Empty)
                .Replace("</h2>", string.Empty)
                .Replace("<h3>", string.Empty)
                .Replace("</h3>", string.Empty)
                .Replace("<ul>", string.Empty)
                .Replace("</ul>", string.Empty)
                .Replace("<li>", string.Empty)
                .Replace("</li>", string.Empty);

            var version = Version.Parse(title);

            return new UpdateModel(
                Updated: updated,
                Link: link,
                Version: version,
                Content: content,
                Author: author);
        }
    }
}