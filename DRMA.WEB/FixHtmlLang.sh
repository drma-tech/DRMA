#!/usr/bin/env bash

set -e

WWWROOT="$1"

if [ ! -d "$WWWROOT" ]; then
    echo "Directory not found: $WWWROOT"
    exit 1
fi

# Mudança para 'for' evita problemas de escopo de variáveis e subshell do 'while'
EOF_PATTERN=$(export LC_ALL=C; prename --version >/dev/null 2>&1 && echo "yes" || echo "no")

# Usando process substitution ou loop direto para garantir consistência
while IFS= read -r -d '' FILE; do
    DIRECTORY="$(dirname "$FILE")"
    RELATIVE="${DIRECTORY#"$WWWROOT"}"
    RELATIVE="${RELATIVE#/}"

    if [ -z "$RELATIVE" ]; then
        LANG="en"
    else
        LANG="${RELATIVE%%/*}"
        case "$LANG" in
            en|pt|es|fr|it|de) ;;
            *) LANG="en" ;;
        esac
    fi

    # 1. Altera o HTML (que você confirmou que já funciona)
    sed -i -E \
        "s/(<html[^>]*[[:space:]])lang=[\"'][^\"']*[\"']/\1lang=\"${LANG}\"/I" \
        "$FILE"

    if ! grep -qi '<html[^>]*[[:space:]]lang=' "$FILE"; then
        sed -i -E \
            "s/<html>/<html lang=\"${LANG}\">/I" \
            "$FILE"
    fi

    echo "Verified HTML text: $FILE -> lang=$LANG"

    # 2. Mata os arquivos antigos para não haver cache ou conflito
    rm -f "${FILE}.br"
    rm -f "${FILE}.gz"

    # 3. Força a compressão via STDIN apontando explicitamente para o arquivo modificado
    if command -v brotli >/dev/null 2>&1; then
        # Lendo o arquivo alterado diretamente e cuspindo no .br correspondente
        brotli -9 < "$FILE" > "${FILE}.br"
        echo "Brotli generated from modified file: ${FILE}.br"
    fi

    if command -v gzip >/dev/null 2>&1; then
        gzip -9 < "$FILE" > "${FILE}.gz"
        echo "Gzip generated from modified file: ${FILE}.gz"
    fi

done < <(find "$WWWROOT" -type f -name "index.html" -print0)
