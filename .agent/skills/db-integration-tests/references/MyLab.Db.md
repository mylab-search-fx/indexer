# MyLab.Db

[![NuGet](https://img.shields.io/nuget/v/MyLab.Db.svg)](https://www.nuget.org/packages/MyLab.Db/)

Предоставляет инструменты для работы с БД. Базируется на [linq2db](https://github.com/linq2db/linq2db).

## Обзор 

В конфигурационном файле приложения указываем строку подключения:

```json
{
  "DB": "Data Source=c:\\mydb.db;Version=3;"
}
```

В стартапе приложения интегрируем инструменты для работы с БД:

```c#
public class Startup
{
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddDbTools(new SQLiteDataProvider())
            .ConfigureDbTools(Configuration);
    }
}
```

Получаем менеджер БД в сервисе и используем:

```c#
public class TestService
{
    IDbManager _db;
    
    public TestService(IDbManager db)
    {
        _db = db;
    }
    
    public Task<string> GetValue(int id)
    {
        await using var dc = dbManager.Connect();
        var res = await dc
            .GetTable<TestDbEntity>()
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
```

## Конфигурация

Узел конфигурации инструментов для работы с БД имеет фиксированное имя `DB`.

#### Строка подключения по умолчанию

Случай, когда приложение работает с единственной базой данных.

```json
{
  "DB": "Data Source=c:\\mydb.db;Version=3;"
}
```

#### Именованные строки подключения

Случай, когда приложение работает с несколькими базами данных. Правил именования строк подключения нет.

```json
{
  "DB": {
    "Cs1": "Data Source=c:\\mydb-1.db;Version=3;",
    "Cs2": "Data Source=c:\\mydb-2.db;Version=3;"
  }
}
```

#### Детальное определение

В случае, если строку подключения необходимо определить по компонентам, имеется возможность детального определения.

При этом строка подключения определяется как узел. В этом узле обязательно должно быть поле `ConnectionString`, содержащее шаблон строки подключения. Шаблон строки подключения представляет собой строку подключения, где в качестве некоторых значений указаны тэги (в формате `{TagName}`). При получении строки подключения вместо этих тэгов будут подставляться значения из одноимённых полей узла  конфигурации этой строки подключения.

Пример строки подключения по умолчанию:

```json
{
  "DB": {
    "User": "foo",
    "Password": "bar",
    "ConnectionString": "Server=myServerAddress;Database=myDataBase;Uid={User};Pwd={Password};"
  }
}
```

Пример именованных строк подключения:

```json
{
  "DB": {
    "Cs1": {
      "User": "foo1",
      "Password": "bar1",
      "ConnectionString": "Server=myServerAddress;Database=myDataBase;Uid={User};Pwd={Password};"
    },
    "Cs2": {
      "User": "foo2",
      "Password": "bar2",
      "ServiceName": "MyService",
      "ConnectionString": "SERVER=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=MyPort))(CONNECT_DATA=(SERVICE_NAME={ServiceName})));uid={User};pwd={Password};"
    }
  }
}
```

Правил именования тэгов подключения нет. В тэги могут быть вынесены любые части строки подключения.

#### Классический конфиг

Поддерживаются классические именованные строки подключения:

```json
"ConnectionStrings": {
    "Default": "Data Source=c:\\mydb-1.db;Version=3;",
    "Custom": "Data Source=c:\\mydb-2.db;Version=3;"
  }
```

`Default` - строка подключения, которая будет использоваться по умолчанию.

## Интеграция 

Для работы инструментов БД необходимо добавить их в сервисы приложения. Для этого необходимо использовать методы расширения для `IServiceCollection`:

```C#
public class Startup
{
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddDbTools(new SQLiteDataProvider())
            .ConfigureDbTools(Configuration);
    }
}
```

В данном методе осуществляется загрузка конфигурации и определение используемых провайдеров БД. 

В примере выше регистрируется провайдер `SQLite` в качестве провайдера по умолчанию. Такой вариант подходит для случая, когда всеми БД, с которыми работает приложение, управляет одна СУБД.

Если приложение работает с разными СУБД, необходимо указать соответствие имён строк подключений и поставщиков данных:

```C#
public class Startup
{
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var map = new Dictionary<string, IDataProvider>
        {
            { "Cs1", new SQLiteDataProvider() },
            { "Cs2", new MySqlDataProvider() }
        };
        
        services
            .AddDbTools(new MapDbProviderSource(map))
            .ConfigureDbTools(Configuration);
    }
}
```

## Использование

### DbManager

Инструменты БД доступны через интерфейс менеджера БД `IDbManager`. Этот менеджер можно получить в конструкторе сервиса через DI контейнер. 

Пример использования менеджера:

```C#
public class TestService
{
    IDbManager _db;
    
	public TestService(IDbManager db)
    {
        _db = db;
    }
    
    public async Task<TestDbEntity> Get(int id)
    {
        _db.....
    }
}
```

#### Метод `Use`

Основной задачей менеджера `IDbManager` является предоставление подключения БД. Подключение создаётся типа из `linq2db`: `LinqToDB.Data.DataConnection`.

Пример использования метода:

```C#
public async Task<TestDbEntity> Get(int id)
{
    await using var dc = _db.Use();
    return await dc
        .GetTable<TestDbEntity>()
        .FirstOrDefaultAsync(e => e.Id == id);
}
```

#### Метод `DoOnce`

Этот метод предоставляет объект контекста выполнения запроса в БД типа `LinqToDB.DataContext`. `DataContext` позволяет выполнить запросы в БД, каждый раз создавая новое подключение. Поэтому не нужно беспокоиться об освобождении ресурсов после выполнения запроса. Это полезно, если нужно сделать один запрос.

Пример использование метода:

```C#
public async Task<TestDbEntity> Get(int id)
{
    return await _db.DoOnce()
        .GetTable<TestDbEntity>()
        .FirstOrDefaultAsync(e => e.Id == id);
}
```

### Работа с подключениями

Поставщики данных БД под капотом используют пулы подключения. На время существования инициализированного объекта `DataConnection`, подключения пула считается занятым. Если своевременно не уничтожать их (`Dispose`), это может привести к исчерпанию пула подключений и невозможностью подключиться в БД или превышения допустимого кол-ва подключений к БД.

#### Правильная работа с подключениями

 ##### Подключение в using

Следует использовать подключение в конструкции `using`:

```C#
public async Task<TestDbEntity> Get(int id)
{
    await using var dc = _db.Use();
    return await dc
        .GetTable<TestDbEntity>()
        .FirstOrDefaultAsync(e => e.Id == id);
}
```

##### Запрос в using

Следует выполнять запросы в пределах `using`:

```C#
public async Task<IEnumerable<TestDbEntity>> Get()
{
    await using var dc = _db.Use();
    return await dc
        .GetTable<TestDbEntity>()
        .ToArrayAsync();
}
```

#### Неправильная работа с подключениями

##### Запрос вне using

Выдача запроса на выполнение вне области действия `using`:

```C#
public async Task<IEnumerable<TestDbEntity>> Get()
{
    await using var dc = _db.Use();
    return dc
        .GetTable<TestDbEntity>();
	    //.ToArrayAsync();
}
```

Запрос будет выполнен вне метода - в пользовательском коде при попытке перечисления:

 ```C#
var persons = Get().ToList();

// or

foreach(var itm in var persons = Get().ToList())
{
}
 ```

Подробнее - [тут](https://github.com/linq2db/linq2db/wiki/Managing-data-connection).

### Автотранзакции

`Автотранзакция` - это абстракция, определяющаяся как транзакция, которая автоматически фиксируется, если не было отката.

В `MyLab.Db` такой функционал реализуется методом расширения для `DataConnection`: `PerformAutoTransactionAsync`.

Ниже показан пример использования этого метода:

```C#
await using var dataConn = _db.Use();
await dataConn.PerformAutoTransactionAsync(dc =>
{
    return dc.GetTable<TestDbEntity>().InsertAsync(() => 
        new TestDbEntity
        {
            Id = entId,
            Value = "foo"
        });
});
```

Откат транзакции осуществляется только в случае, если в процессе выполнения переданного выражения возникает необработанное исключение.

### Короткие методы

#### Метод Tab<>()

Коротки метод получения доступа к таблице. Расширение для `DataConnection` и `DataContext`. Аналог метода `GetTable<>()`.