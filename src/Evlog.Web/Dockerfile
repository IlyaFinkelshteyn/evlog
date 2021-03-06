#
# Stage 0A
# Base
FROM microsoft/dotnet:2.1.2-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80


#
# Stage 1
# Build
FROM microsoft/dotnet:2.1.400-sdk-alpine AS build

# Install yarn
RUN echo -e 'http://dl-cdn.alpinelinux.org/alpine/edge/main\nhttp://dl-cdn.alpinelinux.org/alpine/edge/community\nhttp://dl-cdn.alpinelinux.org/alpine/edge/testing' > /etc/apk/repositories
RUN apk add --no-cache \
    yarn \
    mongodb
RUN yarn global add parcel-bundler

# Copy the project files over & restore dependencies
WORKDIR /src

COPY evlog.sln ./
COPY docker-compose.dcproj .

COPY tests/Evlog.UnitTests/Evlog.UnitTests.csproj tests/Evlog.UnitTests/
COPY tests/Evlog.IntegrationTests/Evlog.IntegrationTests.csproj tests/Evlog.IntegrationTests/

COPY src/Evlog.Domain/Evlog.Domain.csproj src/Evlog.Domain/
COPY src/Evlog.Infrastructure/Evlog.Infrastructure.csproj src/Evlog.Infrastructure/
COPY src/Evlog.Web/Evlog.Web.csproj src/Evlog.Web/

# RUN dotnet sln evlog.sln remove docker-compose.dcproj
RUN dotnet restore evlog.sln

# Copy the package.json file & restore dependencies
WORKDIR /src/src/Evlog.Web
COPY src/Evlog.Web/package.json .
COPY src/Evlog.Web/yarn.lock .
RUN yarn

# Copy the remaining files over
WORKDIR /src
COPY . .

# Run tests
RUN mkdir -p /data/db \
    && chown -R mongodb:mongodb /data/db \
    && mkdir -p /var/log \
    && mongod --fork --logpath /var/log/mongodb.log --dbpath /data/db \
    && dotnet test tests/evlog-tests.sln \
    && mongod --dbpath /data/db --shutdown

# Build
WORKDIR /src/src/Evlog.Web
RUN yarn build
RUN dotnet build -c Release -o /app


#
# Stage 2
# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app

#
# Stage 0B
# Self contained base
FROM base As self-contained-base
RUN apk add --no-cache \
    mongodb
RUN mkdir -p /data/db && chown -R mongodb:mongodb /data/db
ENV Mongo__UseAuthentication false

#
# Stage 3B Final (self-contained)
# The self-contained container
FROM self-contained-base AS self-contained
WORKDIR /app
COPY --from=publish /app .

# Setup the run script
RUN echo "#!/bin/sh" >> run.sh \
    && echo "mongod --fork --logpath /var/log/mongodb.log --dbpath /data/db" >> run.sh \
    && echo "dotnet /app/Evlog.Web.dll" >> run.sh \
    && chmod +x run.sh

# Seed the mongodb
COPY scripts/seed.js .
RUN mkdir -p /var/log \
    && mongod --fork --logpath /var/log/mongodb.log --dbpath /data/db \
    && mongo seed.js \
    && mongod --dbpath /data/db --shutdown \
    && rm -rf scripts
VOLUME /data/db
ENTRYPOINT ["./run.sh"]

#
# Stage 3 Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Evlog.Web.dll"]
