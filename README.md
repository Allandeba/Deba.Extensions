# Deba.Extensions

![CI Status](https://github.com/allandeba/Deba.Extensions/actions/workflows/publish.yml/badge.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/allandeba/Deba.Extensions/blob/main/LICENSE)

Algumas extensões para utilizar entre os projetos

## Build, testes e pacote local

Na raiz do repositório:

```powershell
dotnet restore Deba.Extensions.sln
dotnet build Deba.Extensions.sln --configuration Release --no-restore -p:GeneratePackageOnBuild=false
dotnet test Deba.Extensions.sln --configuration Release --no-build --no-restore -p:GeneratePackageOnBuild=false
dotnet pack Deba.Extensions/Deba.Extensions.csproj --configuration Release --no-build --no-restore --output artifacts/packages -p:GeneratePackageOnBuild=false -p:PackageVersion=0.0.0-net10-validation
```

## Versionamento

Para adicionar minor ou major no package deve ser feito um commit com a seguinte nomenclatura:

- Major: `+semver: breaking` ou `+semver: major`
- Minor: `+semver: feature` ou `+semver: minor`

## Instalação

```bash
dotnet add package Deba.Extensions
```
