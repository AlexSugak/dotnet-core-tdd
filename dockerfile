FROM microsoft/dotnet:2.1-sdk

ENV NUGET_XMLDOC_MODE=skip

COPY ./api/api.csproj /app/api/
COPY ./db/db.csproj /app/db/
COPY ./test/test.csproj /app/test/
COPY ./dotnet-core-tdd.sln /app/
WORKDIR /app

RUN dotnet restore dotnet-core-tdd.sln

COPY . /app

RUN dotnet build dotnet-core-tdd.sln

ENTRYPOINT cd /app/test && \
           dotnet watch test --no-restore