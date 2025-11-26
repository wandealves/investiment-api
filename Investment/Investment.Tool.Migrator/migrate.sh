#!/bin/bash

# Script para executar migrations do Investment.Tool.Migrator

# Cores para output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Diretório do script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo -e "${YELLOW}=== Investment Database Migrator ===${NC}\n"

# Verifica se foi passado argumento para reverter migrations
if [ "$1" == "down" ] || [ "$1" == "rollback" ]; then
    echo -e "${YELLOW}Revertendo todas as migrations...${NC}\n"
    cd "$SCRIPT_DIR"
    dotnet run down

    if [ $? -eq 0 ]; then
        echo -e "\n${GREEN}✓ Migrations revertidas com sucesso!${NC}"
    else
        echo -e "\n${RED}✗ Erro ao reverter migrations.${NC}"
        exit 1
    fi
elif [ "$1" == "help" ] || [ "$1" == "-h" ] || [ "$1" == "--help" ]; then
    echo "Uso: ./migrate.sh [opção]"
    echo ""
    echo "Opções:"
    echo "  (sem argumentos)  Aplica todas as migrations pendentes"
    echo "  down, rollback    Reverte todas as migrations"
    echo "  help, -h, --help  Mostra esta mensagem de ajuda"
    echo ""
    exit 0
else
    echo -e "${YELLOW}Aplicando migrations...${NC}\n"
    cd "$SCRIPT_DIR"
    dotnet run

    if [ $? -eq 0 ]; then
        echo -e "\n${GREEN}✓ Migrations aplicadas com sucesso!${NC}"
    else
        echo -e "\n${RED}✗ Erro ao aplicar migrations.${NC}"
        exit 1
    fi
fi
