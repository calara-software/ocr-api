FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

WORKDIR /app

## WORKDIR /src
COPY *.csproj  ./
RUN dotnet restore
## RUN apt update && apt install tesseract-ocr libtesseract-dev -y

## WORKDIR "/src/WinService.api"
COPY . ./
RUN dotnet publish -c Release -o out

##FROM build AS publish
##RUN dotnet publish "WinService.api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
## FROM base AS final
WORKDIR /app
COPY --from=build-env /app/out .
RUN apt update && apt install tesseract-ocr -y
##ENTRYPOINT [ "dotnet", "WinService.api.dll" ]

RUN useradd -m myactivebiuser
USER myactivebiuser

CMD ASPNETCORE_URLS="http://*:$PORT" dotnet TesseractApi.dll