﻿## UniversityBot.EF

Если необходимо использовать новую базу данных. Отличную от Sqlite, то достаточно реализовать ContextFactoryBase или IContextFactory. Она создаёт и конфигурирует контекст базы данных для работы с определенной базой данных.



Для примера можно использовать UniversityBot.Sqlite