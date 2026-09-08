#!/usr/bin/env bash

set -e

WWWROOT="$1"

if [ ! -d "$WWWROOT" ]; then
    echo "Directory not found: $WWWROOT"
    exit 1
fi

find "$WWWROOT" -type f -name "index.html" | while read -r FILE
do
    DIRECTORY="$(dirname "$FILE")"
    RELATIVE="${DIRECTORY#"$WWWROOT"}"
    RELATIVE="${RELATIVE#/}"

    if [ -z "$RELATIVE" ]; then
        LANG="en"
    else
        LANG="${RELATIVE%%/*}"

        case "$LANG" in
            en|pt|es|fr|it|de)
                ;;
            *)
                LANG="en"
                ;;
        esac
    fi

    # Altera o HTML
    sed -i -E \
        "s/(<html[^>]*[[:space:]])lang=[\"'][^\"']*[\"']/\1lang=\"${LANG}\"/I" \
        "$FILE"

    if ! grep -qi '<html[^>]*[[:space:]]lang=' "$FILE"; then
        sed -i -E \
            "s/<html>/<html lang=\"${LANG}\">/I" \
            "$FILE"
    fi

    echo "Updated HTML: $FILE -> lang=$LANG"

    # --- NOVO: Atualiza as versões comprimidas (.br e .gz) ---
    
    # Remove as versões antigas geradas pelo componente de prerender
    rm -f "${FILE}.br"
    rm -f "${FILE}.gz"

    # Recria o arquivo .br (Brotli) se a ferramenta estiver instalada
    if command -v brotli >/dev/null 2>&1; then
        brotli -f -k -9 "$FILE"
        echo "Updated Brotli: ${FILE}.br"
    else
        echo "Warning: 'brotli' command not found. Skipping .br generation."
    fi

    # Recria o arquivo .gz (Gzip)
    if command -v gzip >/dev/null 2>&1; then
        gzip -f -k -9 "$FILE"
        echo "Updated Gzip: ${FILE}.gz"
    fi
done