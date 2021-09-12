## UniversityBot.ASP

Для смены источника базы данных, необходимо подменить реализацию IContextFactory.

Смотреть код в Strurtup.cs

```c#
var dbFilePath = Configuration["DBFilePath"];
var connectionString = SqliteContextFactory.BuildConnectionString(dbFilePath);
var sqlDbFactory = new SqliteContextFactory(connectionString, true);
using var db = sqlDbFactory.Create(optionConfigurator: null);
services.ConfigureUniversityBotCore(db, sqlDbFactory);
```



Все сообщения из бота приходят в QuizBot.cs (см OnMessageActivityAsync, OnMembersAddedAsync)

После чего он передаёт сообщение в CommandRouter и он маршрутизует запрос в необходимы обработчик

