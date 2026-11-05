FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PetMap/PetMap.csproj PetMap/
RUN dotnet restore PetMap/PetMap.csproj

COPY . .
RUN dotnet publish PetMap/PetMap.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build --chown=app:app /app/publish .
RUN mkdir -p /app/wwwroot/uploads /var/lib/petmap/keys \
    && chown -R app:app /app/wwwroot/uploads /var/lib/petmap

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "PetMap.dll"]
