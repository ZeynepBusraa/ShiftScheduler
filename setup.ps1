mkdir src
cd src

# Solution ve projeleri oluştur
dotnet new sln -n ShiftScheduler
dotnet new webapi -n ShiftScheduler.Api
dotnet new classlib -n ShiftScheduler.Application
dotnet new classlib -n ShiftScheduler.Domain
dotnet new classlib -n ShiftScheduler.Infrastructure

# Projeleri solution'a ekle
dotnet sln ShiftScheduler.sln add ShiftScheduler.Api/ShiftScheduler.Api.csproj
dotnet sln ShiftScheduler.sln add ShiftScheduler.Application/ShiftScheduler.Application.csproj
dotnet sln ShiftScheduler.sln add ShiftScheduler.Domain/ShiftScheduler.Domain.csproj
dotnet sln ShiftScheduler.sln add ShiftScheduler.Infrastructure/ShiftScheduler.Infrastructure.csproj

# Referansları ekle
dotnet add ShiftScheduler.Api/ShiftScheduler.Api.csproj reference ShiftScheduler.Application/ShiftScheduler.Application.csproj ShiftScheduler.Infrastructure/ShiftScheduler.Infrastructure.csproj
dotnet add ShiftScheduler.Infrastructure/ShiftScheduler.Infrastructure.csproj reference ShiftScheduler.Application/ShiftScheduler.Application.csproj ShiftScheduler.Domain/ShiftScheduler.Domain.csproj
dotnet add ShiftScheduler.Application/ShiftScheduler.Application.csproj reference ShiftScheduler.Domain/ShiftScheduler.Domain.csproj

# NuGet paketlerini kur
dotnet add ShiftScheduler.Infrastructure/ShiftScheduler.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add ShiftScheduler.Infrastructure/ShiftScheduler.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools
dotnet add ShiftScheduler.Application/ShiftScheduler.Application.csproj package FluentValidation.AspNetCore
dotnet add ShiftScheduler.Api/ShiftScheduler.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer

echo "Setup completed successfully."
