FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# ZScaler setup
RUN apt-get install ca-certificates
COPY zscaler.crt /usr/local/share/ca-certificates
RUN update-ca-certificates

# Get npm
RUN apt-get update -yq && apt-get upgrade -yq && apt-get install -yq curl git nano
RUN curl -sL https://deb.nodesource.com/setup_8.x | bash - && apt-get install -yq nodejs build-essential
RUN	npm config set strict-ssl false
RUN npm install -g npm --ignore-ssl

# Copy csproj and restore as distinct layers
COPY ./*.sln ./

# Copy the main source project files
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

RUN dotnet restore

# NPM Packages

COPY ARP/ClientApp/package.json /tmp/package.json
COPY ARP/ClientApp/package-lock.json /tmp/package-lock.json
RUN cd /tmp && npm install --ignore-ssl
RUN mkdir -p ./ARP/ClientApp && cp -a /tmp/node_modules ./ARP/ClientApp

# Copy everything else and build
COPY . ./

# Build tests

WORKDIR /app/ARP.ImageComparison.Test
RUN dotnet publish -c Release -o out
RUN dotnet vstest out/ARP.ImageComparison.Test.dll --logger:"trx;LogFileName=ARP.ImageComparison.Test.trx" --ResultsDirectory:/TestResults

WORKDIR /app/ARP.Tests
RUN dotnet publish -c Release -o out
RUN dotnet vstest out/ARP.Tests.dll --logger:"trx;LogFileName=ARP.Tests.trx" --ResultsDirectory:/TestResults

# Build Web Aplication

WORKDIR /app/ARP
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/ARP/out .
ENTRYPOINT ["dotnet", "ARP.dll"]