.DEFAULT_GOAL := help
.PHONY: build db-migrate test test-watch rebuild-docker log log-watch install-tools help 

build: ## builds the solution
	dotnet build

db-migrate: ## runs all db migrations against local MySQL DB. Make sure to run 'make install-tools' first
	dotnet fm migrate -p mysql -c "server=0.0.0.0;port=3306;database=sut;user=root;password=root" -a "./db/bin/Debug/netstandard2.0/db.dll"

db-rollback: ## rolls back all db migrations on local MySQL DB to produce clean db schema. Make sure to run 'make install-tools' first
	dotnet fm rollback -p mysql -c "server=0.0.0.0;port=3306;database=sut;user=root;password=root" -a "./db/bin/Debug/netstandard2.0/db.dll"

test: ## runs all tests in solution
	dotnet test ./test/test.csproj

test-watch: ## starts the watch and runs all tests in solution every time any file changes
	dotnet watch test ./test/test.csproj

rebuild-docker: ## rebuilds and restarts docker containers
	docker-compose down
	docker-compose build --no-cache
	docker-compose up -d

log: ## shows sut docker logs
	docker logs sut --tail 50

log-watch: ## watches sut docker logs
	docker logs sut --tail 50 -f

install-tools: ## installs required dev tools
	dotnet tool install -g FluentMigrator.DotNet.Cli

help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-30s\033[0m %s\n", $$1, $$2}'