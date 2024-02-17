namespace NZ.RdpMaid.App.SerializationModels.AtomRss;

internal static class ExampleData
{
    public const string Feed =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom" xmlns:media="http://search.yahoo.com/mrss/" xml:lang="en-US">
            <id>tag:github.com,2008:https://github.com/nikitazu/NZ.RdpMaid/releases</id>
            <link type="text/html" rel="alternate" href="https://github.com/nikitazu/NZ.RdpMaid/releases"/>
            <link type="application/atom+xml" rel="self" href="https://github.com/nikitazu/NZ.RdpMaid/releases.atom"/>
            <title>Release notes from NZ.RdpMaid</title>
            <updated>2024-02-16T20:08:54+03:00</updated>
            <entry>
            <id>tag:github.com,2008:Repository/743240146/0.7.3</id>
            <updated>2024-02-16T20:11:19+03:00</updated>
            <link rel="alternate" type="text/html" href="https://github.com/nikitazu/NZ.RdpMaid/releases/tag/0.7.3"/>
            <title>0.7.3</title>
            <content type="html">&lt;h3&gt;Добавлено&lt;/h3&gt;
        &lt;ul&gt;
        &lt;li&gt;[rdpm025] Добавлена кнопка доступа к выпадающему меню в правом-нижнем углу.&lt;/li&gt;
        &lt;li&gt;[rdpm025] Добавлена возможность проверить наличие обновлений на github.&lt;/li&gt;
        &lt;li&gt;[rdpm025] Проверка обновлений запускается из выпадающего меню в правом-нижнем углу.&lt;/li&gt;
        &lt;li&gt;[rdpm025] На экране обновлений можно пройти по ссылке на страницу релиза.&lt;/li&gt;
        &lt;/ul&gt;
        &lt;h3&gt;Изменено&lt;/h3&gt;
        &lt;ul&gt;
        &lt;li&gt;Слегка уменьшен размер панели ввода пинкода за счет внутренних отступов.&lt;/li&gt;
        &lt;li&gt;Доступ к каталогу настроек перенесён в выпадающее меню.&lt;/li&gt;
        &lt;/ul&gt;</content>
            <author>
                <name>nikitazu</name>
            </author>
            <media:thumbnail height="30" width="30" url="https://avatars.githubusercontent.com/u/271185?s=60&amp;v=4"/>
            </entry>
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
}