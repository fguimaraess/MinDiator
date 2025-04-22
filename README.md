# 🧠 MinDiator

**MinDiator** é uma biblioteca **leve**, **moderna** e **sem dependências externas** que implementa o padrão **Mediator**, sendo ideal para aplicações baseadas em **CQRS** e arquitetura **Vertical Slice**.

> Inspirado no padrão [Mediator](https://dev.to/moh_moh701/understanding-the-mediator-pattern-in-net-52do), mas com foco em simplicidade, extensibilidade e performance.

---

## 🚀 Por que usar o MinDiator?

- ✅ **Zero dependências externas**
- 🧱 **Pensado para CQRS** – separa claramente Commands e Queries
- 🧩 **Ideal para Vertical Slice Architecture** – organização de features por funcionalidade
- 🧪 **Pipeline Behavior embutido** – suporte a logging, validação, autenticação, etc.
- 🧠 **Interfaces simples**: `IRequest`, `IRequestHandler`, `IPipelineBehavior`, `IRequestExceptionHandler`
- 🧰 **Fácil de extender e customizar**
- 🧵 **Mais controle do que entra no seu container** – sem reflexão mágica

---

## 📦 Instalação

```bash
dotnet add package MinDiator
```

---

## ⚙️ Como configurar

### 1. **Setup com DI**

Você pode registrar o MinDiator diretamente no `Startup.cs` ou no seu `Program.cs`:

```csharp
// MinDiator setup
services.AddMinDiator(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(SampleRequest).Assembly);

    cfg.AddBehavior(
        typeof(MinDiator.Interfaces.IPipelineBehavior<,>),
        typeof(MinDiatorPerformanceBehavior<,>)
    );
});
```

- `RegisterServicesFromAssembly(...)`: Registra todos os `IRequestHandler`, `IPipelineBehavior` e `IRequestExceptionHandler` presentes no assembly.
- `AddBehavior(...)`: Permite adicionar behaviors customizados ao pipeline (como logging, validação, etc).

---

### 2. **Exemplo simples**

```csharp
public class SampleRequest : IRequest<string>
{
}

public class SampleHandler : IRequestHandler<SampleRequest, string>
{
    public Task<string> Handle(SampleRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("ok");
    }
}
```

---

## 🧭 Entendendo o padrão CQRS

**CQRS (Command Query Responsibility Segregation)** divide as responsabilidades da aplicação em:

| Tipo    | Interface         | Retorno      | Responsabilidade                      |
|---------|-------------------|--------------|----------------------------------------|
| Command | `IRequest<Unit>`  | `Unit`       | Executar uma ação (ex: criar, atualizar, deletar) |
| Query   | `IRequest<T>`     | `T`          | Buscar um dado (ex: listar, detalhar) |

---

## 🧱 Vertical Slice + MinDiator

Com a arquitetura Vertical Slice, sua aplicação é dividida por **feature**, e não por camada (ex: Controllers, Services, Repositories). Isso ajuda a manter cada funcionalidade **autocontida** e de fácil manutenção.

### 💡 Como MinDiator ajuda:

- Cada requisição (`IRequest`) representa uma **única ação** da aplicação.
- Cada handler (`IRequestHandler`) representa **a lógica isolada** daquela ação.
- Os Behaviors permitem adicionar funcionalidades transversais (cross-cutting) de forma organizada.

### 📁 Exemplo de organização com Vertical Slice

```
Features/
└── Usuarios/
    ├── Create/
    │   ├── CreateUsuarioCommand.cs
    │   ├── CreateUsuarioHandler.cs
    │   └── CreateUsuarioValidator.cs
    └── Get/
        ├── GetUsuarioQuery.cs
        ├── GetUsuarioHandler.cs
```

---

## 🧪 Pipeline Behavior

Você pode criar middlewares para interceptar qualquer request:

```csharp
public class MinDiatorPerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<MinDiatorPerformanceBehavior<TRequest, TResponse>> _logger;

    public MinDiatorPerformanceBehavior(ILogger<MinDiatorPerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        var stopwatch = Stopwatch.StartNew();

        var response = await next();
        stopwatch.Stop();

        _logger.LogInformation("[MinDiator] {Request} executed in {Elapsed}ms", request.GetType().Name, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

---

## ✅ Roadmap

- [x] Pipeline Behaviors
- [] Exception Handlers
- [x] IPublisher and INotification
- [x] Test helpers

---

## 🤝 Contribuições

Pull requests são super bem-vindos. Se quiser contribuir com uma ideia, melhoria ou bugfix, fique à vontade para abrir uma issue ou PR.

---

## 📄 Licença

MIT © [fguimaraess](https://github.com/fguimaraess)
