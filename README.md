# JWT-With-Polly
[English version](https://github.com/JFRode/JWT-With-Polly/blob/master/README.md)

[Visualize apenas este arquivo se estiver com pressa](https://github.com/JFRode/JWT-With-Polly/blob/master/APIClient/Clients/APIWhoSayNiClient.cs)

Este repositório serve como um artigo de como aproveitar a funcionalidade de Retry do Polly para atualizar automaticamente o Token do JWT, quando necessário. Isso não significa que seja uma boa prática, mas um experimento que fiz.

**O que é o Polly?:** é uma biblioteca de resiliência para .NET onde o desenvolvedor pode implementar políticas para tratamento de falhas na comunicação com uma API por exemplo. Entre as politicas principais estão Retry, Circuit breaker, Fallback etc. Você pode estar verificando a implementação de uma delas [neste artigo da Microsoft](https://docs.microsoft.com/pt-br/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly).

**O que é o JWT?:** JSON Web Token é um padrão aberto para comunicação segura entre aplicações. As partes são denominadas Cliente e Servidor, onde o cliente deseja acessar alguma informação que o servidor fornece, por exemplo o servidor sendo uma API de usuários e o cliente necessitando fazer um GET de todos os usuários. A comunicação é iniciada com o Cliente enviando uma chave/dado que ambos conhecem, em seguida o servidor valida e assina devolvendo um Token. Este token será utilizado pelo Cliente para toda comunicação que for realizar com o Servidor, de maneira que o servidor tem conhecimento que o cliente pode acessar os dados (GET todos os usuários) porque tem um Token válido/assinado. Você pode ler mais sobre isso [clicando aqui](https://docs.microsoft.com/pt-br/dotnet/architecture/microservices/secure-net-microservices-web-applications/).

# Funcionamento

Para fins didáticos, estão dispostas duas API's na mesma solution. 
* API-01 é o Servidor, denominada APIWhoSayNi pois a unica coisa que ela faz é retornar "Ni!" ao realizar um GET;
* API-02 é o Cliente, denominada APIClient pois é a API que irá consumir a API-01.

O código principal deste repositório está na classe [APIWhoSayNiClient.cs](https://github.com/JFRode/JWT-With-Polly/blob/master/APIClient/Clients/APIWhoSayNiClient.cs)

Com o JWT configurado na API-01, configuro uma Policy de Retry na classe de comunicação da API-02, para quando houver um erro do tipo `HttpStatusCode.Unauthorized` realizar o Refresh do Token. **Unauthorized** é resultado de um token inválido que é exatamente o que desejamos saber. 

Dentro da function da Policy configurada, realizamos a atualização (Refresh) do Token, e em seguida o Polly segue seu curso natural realizando o Retry da requisição, porém desta vez com o Token válido.

![](https://github.com/JFRode/JWT-With-Polly/blob/master/CommunicationExample.png)

# Tentativa falha e motivo

Você deve se perguntar, por que não configurei a Policy diretamente na classe **startup** como as demais politicas?
Infelizmente não é tão simples assim e o código abaixo apesar de belo não funciona. No começo acreditava que o Retry executava antes do que estava dentro da function, e realmente pode ocorrer se você não explicitar **async** antes dela (Ref01). Entretanto o código abaixo não funciona porque mesmo que o token sejam atualizado o método que monta a requisição não é reexecutado, o retry é feito exatamente com a mesma requisição que havia sido feita antes da falha, ou seja o token ainda continua o mesmo desatualizado, fazendo com que esse código seja inútil.

```
private static IAsyncPolicy<HttpResponseMessage> GetUnauthorizedPolicy(IServiceCollection services) =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(1, async (response, retryCount, context) =>
                {
                    var provider = services.BuildServiceProvider();
                    var client = provider.GetRequiredService<IAPIWhoSayNiClient>();
                    var token = await client.RefreshAuthenticationToken(new CancellationToken());
                });
```

Dessa forma ainda acreditando que seria possível unir Polly com atualização de Token, acabei achando o artigo do Jerrie Pelser (Ref02), com a implementação da Policy sendo feita diretamente no Client. Realizei uma implementação semelhante e fiz o teste, constatando que o método que monta a requisição é reexecutado após a function da Policy e assim realizando a mágica **JWT-With-Polly**.

# Referências

- Ref01: [Discussão no repositório do Polly no Github](https://github.com/App-vNext/Polly/issues/107)

- Ref02: [Refresh a Google Access Token with Polly - Blog do Jerrie Pelser](https://www.jerriepelser.com/blog/refresh-google-access-token-with-polly/)
