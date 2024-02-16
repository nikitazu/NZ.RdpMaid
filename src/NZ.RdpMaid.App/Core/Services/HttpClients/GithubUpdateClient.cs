﻿#define GITHUB_UPDATE_TEST_MODE__ON

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NZ.RdpMaid.App.Core.Services.HttpClients
{
    internal class GithubUpdateClient(HttpClient client)
    {
#if GITHUB_UPDATE_TEST_MODE__ON
        private static readonly TimeSpan _testModeDelay = TimeSpan.FromSeconds(1);

        private const string _exampleResponseContent =
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
              <id>tag:github.com,2008:https://github.com/nikitazu/NZ.RdpMaid/releases</id>
              <link type="text/html" rel="alternate" href="https://github.com/nikitazu/NZ.RdpMaid/releases"/>
              <link type="application/atom+xml" rel="self" href="https://github.com/nikitazu/NZ.RdpMaid/releases.atom"/>
              <title>Release notes from NZ.RdpMaid</title>
              <updated>2024-02-14T23:29:10+03:00</updated>
              <entry>
                <id>tag:github.com,2008:Repository/743240146/0.7.2</id>
                <updated>2024-02-14T23:31:27+03:00</updated>
                <link rel="alternate" type="text/html" href="https://github.com/nikitazu/NZ.RdpMaid/releases/tag/0.7.2"/>
                <title>0.7.2</title>
                <content type="html">&lt;h2&gt;[v0.7.2]&lt;/h2&gt;
            &lt;h3&gt;Добавлено&lt;/h3&gt;
            &lt;ul&gt;
            &lt;li&gt;[rdpm022] Очищать пароль из буфера обмена через 10 секунд.&lt;/li&gt;
            &lt;li&gt;[rdpm023] Тема оформления &quot;Неко Арк&quot;.&lt;/li&gt;
            &lt;li&gt;[rdpm024] Запуск в единственном экземпляре.&lt;/li&gt;
            &lt;/ul&gt;</content>
                <author>
                  <name>nikitazu</name>
                </author>
                <media:thumbnail height="30" width="30" url="https://avatars.githubusercontent.com/u/271185?s=60&amp;v=4"/>
              </entry>
              <entry>
                <id>tag:github.com,2008:Repository/743240146/0.7.1</id>
                <updated>2024-01-14T21:59:09+03:00</updated>
                <link rel="alternate" type="text/html" href="https://github.com/nikitazu/NZ.RdpMaid/releases/tag/0.7.1"/>
                <title>0.7.1</title>
                <content>No content.</content>
                <author>
                  <name>nikitazu</name>
                </author>
                <media:thumbnail height="30" width="30" url="https://avatars.githubusercontent.com/u/271185?s=60&amp;v=4"/>
              </entry>
            </feed>
            """;

#endif

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

        private static class Atom
        {
            public static readonly XNamespace Ns = "http://www.w3.org/2005/Atom";
            public static readonly XName Entry = Ns + "entry";
            public static readonly XName Updated = Ns + "updated";
            public static readonly XName Title = Ns + "title";
            public static readonly XName Link = Ns + "link";
            public static readonly XName Content = Ns + "content";
            public static readonly XName Author = Ns + "author";
        }

        private readonly HttpClient _client = client ?? throw new ArgumentNullException(nameof(client));

        public async Task<CheckResponse> CheckForUpdates(Version current, CancellationToken ct = default)
        {
#if GITHUB_UPDATE_TEST_MODE__ON
            var content = _exampleResponseContent;
            await Task.Delay(_testModeDelay, ct);
#else
            var response = await _client.GetAsync("https://github.com/nikitazu/NZ.RdpMaid/releases.atom", ct);
            if (!response.IsSuccessStatusCode)
            {
                return new CheckResponse(Status: CheckStatus.Failed, Error: $"Плохой ответ {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
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
                updates = xml.Root.Elements(Atom.Entry)
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

        private static UpdateModel ParseUpdateEntry(XElement xentry)
        {
            var xupdated = xentry.Element(Atom.Updated) ?? throw new FormatException("Отсутствует элемент <updated>");
            var xlink = xentry.Element(Atom.Link) ?? throw new FormatException("Отсутствует элемент <link>");
            var xtitle = xentry.Element(Atom.Title) ?? throw new FormatException("Отсутствует элемент <title>");
            var xauthor = xentry.Element(Atom.Author) ?? throw new FormatException("Отсутствует элемент <author>");
            var xcontent = xentry.Element(Atom.Content) ?? throw new FormatException("Отсутствует элемент <content>");

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