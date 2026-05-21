# ─────────────────────────────────────────────────────────────
#  Dockerfile
#  ─────────────────────────────────────────────────────────────
#  Multi-stage build: 2 estágios para reduzir tamanho final.
#  1º estágio (build):   compila e publica a aplicação
#  2º estágio (runtime): apenas o runtime .NET + binários
#  VANTAGEM: Imagem final pequena (~200MB vs ~1.5GB com SDK)
# ─────────────────────────────────────────────────────────────

# ── 1º ESTÁGIO: BUILD ──────────────────────────────────────
# Usa a imagem do SDK .NET 10 (mais pesada) para compilar.
# O nome "build" é um alias referenciado no COPY --from=build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Define o diretório de trabalho dentro do container
WORKDIR /app

# Copia o arquivo de solução e os projetos
# COPY separado do restore para aproveitar cache de camadas Docker
# Se apenas os .cs arquivos mudarem, as camadas de restore/copy são reusadas
COPY FirstWebApi.slnx ./
COPY src/FirstWebApi.Domain/*.csproj src/FirstWebApi.Domain/
COPY src/FirstWebApi.Application/*.csproj src/FirstWebApi.Application/
COPY src/FirstWebApi.Infrastructure/*.csproj src/FirstWebApi.Infrastructure/
COPY src/FirstWebApi.WebApi/*.csproj src/FirstWebApi.WebApi/

    # Restaura pacotes NuGet (usa cache do Docker)
RUN dotnet restore src/FirstWebApi.WebApi/FirstWebApi.WebApi.csproj

# Copia todo o código fonte restante
COPY . .

# Publica a aplicação (compila + copia binários para /app/publish)
# --no-restore: não precisa restaurar de novo (já foi feito acima)
# -c Release:   modo release (otimizado)
# -o /app/publish: diretório de saída
RUN dotnet publish src/FirstWebApi.WebApi/FirstWebApi.WebApi.csproj \
    -c Release -o /app/publish --no-restore

# ── 2º ESTÁGIO: RUNTIME ────────────────────────────────────
# Usa a imagem ASP.NET Runtime (bem menor que o SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Define o ambiente como Produção por padrão
ENV ASPNETCORE_ENVIRONMENT=Production

# Instala ferramentas de diagnóstico (opcional, útil para debug)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copia os binários publicados do estágio "build"
COPY --from=build /app/publish .

# Healthcheck HTTP para o Docker verificar se a API está saudável
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Porta exposta pelo container
EXPOSE 8080

# Comando de entrada: executa a aplicação .NET
ENTRYPOINT ["dotnet", "FirstWebApi.WebApi.dll"]
