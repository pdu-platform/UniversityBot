## UniversityBot.Core

Основные классы



Все сущности реализуют интерфейс **IBotEntity**



Для избежания Boilerplate кода. Рекомендуется наследовать все сущности от BotEntity передавая в качестве Generic параметра этот же класс. (см https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern)

Т.е добавляем класс MyClass. тогда его декларация будет выглядит следующим образом

```c#
public class MyClass : BotEntity<MyClass>
{
    ....
}
```



BotEntity: умеет себя преобразовывать в читабельный вид и заставляет наследников реализовать 2 метода:

1. EqualsCore для сравнения всех полей
2. GetHashCodeCore для получения Хэш кода(причем Id объекта уже включется в хэш код и нужно использовать оставшиеся поля)



**IBotMessage**:

Интерфейс, который нужно реализовать для всех сообщений которые возвращаются из команд и передаются дальше для обработки. В качестве примера смотреть BotMessageMarker который содержит информацию о прошлом вопросе и благодаря этому находятся дочерние вопросы