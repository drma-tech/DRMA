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

    sed -i -E \
        "s/(<html[^>]*[[:space:]])lang=[\"'][^\"']*[\"']/\1lang=\"${LANG}\"/I" \
        "$FILE"

    if ! grep -qi '<html[^>]*[[:space:]]lang=' "$FILE"; then
        sed -i -E \
            "s/<html>/<html lang=\"${LANG}\">/I" \
            "$FILE"
    fi

    echo "Updated: $FILE -> lang=$LANG"
done