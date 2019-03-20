FROM microsoft/dotnet:2.2.104-sdk-alpine AS build

ARG BUILDCONFIG=RELEASE
ARG VERSION=1.0.0

# copy csproj and restore as distinct layers
COPY ./src/Producer/Producer.csproj ./Producer/
COPY ./src/Queueing/Queueing.csproj ./Queueing/
COPY ./src/Shared/Shared.csproj ./Shared/
RUN dotnet restore Producer/Producer.csproj

# copy everything else and build
COPY ./src/ .
WORKDIR /Producer/
RUN dotnet publish -c $BUILDCONFIG -o out /p:Version=$VERSION

# build runtime image
FROM microsoft/dotnet:2.2-runtime-alpine
WORKDIR /app
COPY --from=build Producer/out ./

ENTRYPOINT ["dotnet", "Producer.dll"]