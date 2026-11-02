FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TrainBooking.sln", "./"]
COPY ["TrainBooking.Api/TrainBooking.Api.csproj", "TrainBooking.Api/"]
COPY ["TrainBooking.Application/TrainBooking.Application.csproj", "TestBooking.Application/"]
COPY ["TrainBooking.Domain/TrainBooking.Domain.csproj", "TrainBooking.Domain/"]
COPY ["TrainBooking.Infrastructure/TrainBooking.Infrastructure.csproj", "TrainBooking.Infrastructure/"]
RUN dotnet restore "TrainBooking.Api/TrainBooking.Api.csproj"
COPY . .
WORKDIR "/src/TrainBooking.Api"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TrainBooking.Api.dll"]