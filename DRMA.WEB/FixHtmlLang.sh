#!/usr/bin/env bash

set -e

WWWROOT="$1"

if [ ! -d "$WWWROOT" ]; then
    echo "Directory not found: $WWWROOT"
    exit 1
fi

while IFS= read -r -d '' FILE; do
    DIRECTORY="$(dirname "$FILE")"
    RELATIVE="${DIRECTORY#"$WWWROOT"}"
    RELATIVE="${RELATIVE#/}"

    if [ -z "$RELATIVE" ]; then
        PAGE_LANG="en"
    else
        PAGE_LANG="${RELATIVE%%/*}"

        case "$PAGE_LANG" in
            en|pt|es|fr|it|de)
                ;;
            *)
                PAGE_LANG="en"
                ;;
        esac
    fi

    # Update the HTML
    sed -i -E \
        "s/(<html[^>]*[[:space:]])lang=[\"'][^\"']*[\"']/\1lang=\"${PAGE_LANG}\"/I" \
        "$FILE"

    if ! grep -qi '<html[^>]*[[:space:]]lang=' "$FILE"; then
        sed -i -E \
            "s/<html>/<html lang=\"${PAGE_LANG}\">/I" \
            "$FILE"
    fi

    echo "Updated HTML: $FILE -> lang=$PAGE_LANG"

    # Remove the old compressed versions
    rm -f "${FILE}.br"
    rm -f "${FILE}.gz"

    # Generate Brotli from the modified HTML
    if command -v brotli >/dev/null 2>&1; then
        brotli -9 < "$FILE" > "${FILE}.br"
        echo "Updated Brotli: ${FILE}.br"
    else
        echo "Warning: brotli is not installed"
    fi

    # Generate Gzip from the modified HTML
    if command -v gzip >/dev/null 2>&1; then
        gzip -9 < "$FILE" > "${FILE}.gz"
        echo "Updated Gzip: ${FILE}.gz"
    else
        echo "Warning: gzip is not installed"
    fi

done < <(find "$WWWROOT" -type f -name "index.html" -print0)