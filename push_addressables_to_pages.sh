#!/bin/bash

# Пути
SOURCE_PATH="Library/com.unity.addressables/aa/WebGL"
TARGET_PATH="docs/RemoteAssets"

# Проверка
if [ ! -d "$SOURCE_PATH" ]; then
  echo "❌ Addressables папка не найдена по пути: $SOURCE_PATH"
  exit 1
fi

echo "🧹 Очищаю старые файлы в $TARGET_PATH..."
rm -rf "$TARGET_PATH"
mkdir -p "$TARGET_PATH"

echo "📦 Копирую Addressables..."
cp -R "$SOURCE_PATH"/* "$TARGET_PATH/"

echo "🔁 Коммитим и пушим..."
git add "$TARGET_PATH"
git commit -m "🚀 Обновление Addressables для GitHub Pages"
git push

echo "✅ Готово! Проверяй:"
echo "🌐 https://maxai312.github.io/yandex-addressables-test/RemoteAssets/catalog.json"
