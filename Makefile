-include .env
export

GIT_SHA := $(shell git rev-parse --short HEAD 2>/dev/null || echo dev)
IMAGE_TAG ?= $(GIT_SHA)

BACKEND := backend
FRONTEND := frontend
SOLUTION := $(BACKEND)/TaskManager.sln
API_PROJECT := $(BACKEND)/src/TaskManager.Api
INFRA_PROJECT := $(BACKEND)/src/TaskManager.Infrastructure
EF := dotnet ef --project $(INFRA_PROJECT) --startup-project $(API_PROJECT)

API_PORT ?= 5080
DB_PORT ?= 5433
LOCAL_DB_CONNECTION := Host=localhost;Port=$(DB_PORT);Database=$(POSTGRES_DB);Username=$(POSTGRES_USER);Password=$(POSTGRES_PASSWORD)

LOCAL_TARGETS := backend-run db-update db-rollback purge-tokens-local
$(LOCAL_TARGETS): export ConnectionStrings__Default = $(LOCAL_DB_CONNECTION)
$(LOCAL_TARGETS): export Jwt__Secret = $(JWT_SECRET)
$(LOCAL_TARGETS): export Jwt__Issuer = $(JWT_ISSUER)
$(LOCAL_TARGETS): export Jwt__Audience = $(JWT_AUDIENCE)
$(LOCAL_TARGETS): export Jwt__LifetimeMinutes = $(JWT_LIFETIME_MINUTES)
backend-run: export ASPNETCORE_ENVIRONMENT = Development
backend-run: export ASPNETCORE_HTTP_PORTS = $(API_PORT)
frontend-dev: export API_PROXY_TARGET = http://localhost:$(API_PORT)

.DEFAULT_GOAL := help

.PHONY: help env \
    build up down clean restart logs ps scale-api db-up migrate purge-tokens \
    tools migration-add migration-remove migration-list migration-script db-update db-rollback purge-tokens-local \
    backend-restore backend-build backend-run backend-test \
    frontend-install frontend-dev frontend-build frontend-typecheck \
    test

help:
	@echo "Environment"
	@echo "  env                 create .env from .env.example"
	@echo ""
	@echo "Containers (build -> release -> run)"
	@echo "  build               build api and web images tagged IMAGE_TAG=$(IMAGE_TAG)"
	@echo "  up                  build images, run migrations and start the whole stack"
	@echo "  down                stop the stack"
	@echo "  clean               stop the stack and remove the database volume"
	@echo "  restart             restart api and web"
	@echo "  logs                follow logs of all services"
	@echo "  ps                  list running services"
	@echo "  scale-api N=3       run N api replicas behind nginx"
	@echo "  db-up               start only PostgreSQL"
	@echo "  migrate             apply migrations with the one-off migrator container"
	@echo "  purge-tokens        remove expired revoked tokens with a one-off container"
	@echo ""
	@echo "Migrations (local, via dotnet-ef)"
	@echo "  migration-add NAME=AddSomething   create a new migration"
	@echo "  migration-remove                  remove the last unapplied migration"
	@echo "  migration-list                    list migrations"
	@echo "  migration-script                  generate idempotent SQL script"
	@echo "  db-update                         apply all migrations to the local DB"
	@echo "  db-rollback TO=MigrationName      revert DB to a migration (TO=0 reverts all)"
	@echo ""
	@echo "Local development"
	@echo "  backend-build       build the .NET solution"
	@echo "  backend-run         run the API on http://localhost:$(API_PORT)"
	@echo "  backend-test        run unit and integration tests (Docker required)"
	@echo "  frontend-install    install npm dependencies"
	@echo "  frontend-dev        run Vite dev server with /api proxy"
	@echo "  frontend-build      build the frontend bundle"
	@echo "  test                run all checks"

env:
	@test -f .env && echo ".env already exists" || (cp .env.example .env && echo ".env created")

build:
	docker compose build

up:
	docker compose up -d --build

down:
	docker compose down

clean:
	docker compose down -v --remove-orphans

restart:
	docker compose restart api web

logs:
	docker compose logs -f

ps:
	docker compose ps

scale-api:
	docker compose up -d --no-recreate --scale api=$(or $(N),2)

db-up:
	docker compose up -d --wait db

migrate:
	docker compose run --rm migrator

purge-tokens:
	docker compose run --rm migrator purge-revoked-tokens

tools:
	dotnet tool restore

migration-add: tools
	@test -n "$(NAME)" || (echo "Usage: make migration-add NAME=MigrationName" && exit 1)
	$(EF) migrations add $(NAME) --output-dir Persistence/Migrations

migration-remove: tools
	$(EF) migrations remove

migration-list: tools
	$(EF) migrations list --no-connect

migration-script: tools
	$(EF) migrations script --idempotent --output $(BACKEND)/migrations.sql

db-update: tools
	$(EF) database update

db-rollback: tools
	@test -n "$(TO)" || (echo "Usage: make db-rollback TO=MigrationName (or TO=0)" && exit 1)
	$(EF) database update $(TO)

purge-tokens-local:
	dotnet run --project $(API_PROJECT) -- purge-revoked-tokens

backend-restore:
	dotnet restore $(SOLUTION)

backend-build:
	dotnet build $(SOLUTION)

backend-run:
	dotnet run --project $(API_PROJECT)

backend-test:
	dotnet test $(SOLUTION)

frontend-install:
	cd $(FRONTEND) && npm ci

frontend-dev:
	cd $(FRONTEND) && npm run dev

frontend-build:
	cd $(FRONTEND) && npm run build

frontend-typecheck:
	cd $(FRONTEND) && npm run typecheck

test: backend-test frontend-typecheck
