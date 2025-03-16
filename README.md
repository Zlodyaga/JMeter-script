# JMeter script program

## Описание

JMeter Script Program — это инструмент для обработки JMX-файлов, позволяющий заменять базовый URL в запросах HTTP Sampler на request type запроса (GET, POST, etc...) и восстанавливать его при необходимости.

## Возможности

- Добавить request type запроса без удаления BaseURL
- Заменить базовый URL на request type запроса
- Восстановление базового URL посредством замены request type запроса на вписанный базовый URL
- Удаление request type запросов из названия
- Отображение прогресса обработки

## Установка и запуск

### Требования

- .NET Framework 6.0 или выше
- Windows OS

## Использование

### Выбор JMX-файла

1. Нажмите кнопку **"Choose file"**.
2. Выберите `.jmx` файл.
3. Поле `txtFilePath` заполнится путем к файлу.
4. Поле `txtFilePath` можно заменить вручную, вписав путь к файлу.

### Удаление базового URL с заменой на request type

1. Введите базовый URL в поле `txtUserInput`.
2. Установите флажок **"Remove base URL from test name"**.
3. Нажмите **"Start script"**.
4. Программа изменит запросы, совпадающие с введённым base URL, заменив введенный URL на request type.

### Добавление request type перед базовым URL

1. Введите базовый URL в поле `txtUserInput`.
2. Уберите флажок **"Remove base URL from test name"** (Если был установлен ранее).
3. Нажмите **"Start script"**.
4. Программа изменит запросы, совпадающие с введённым base URL, добавив request type перед введённым URL.

### Восстановление базового URL

1. Введите базовый URL в поле `txtUserInput`.
2. Нажмите **"Restore base URL"**.
3. Программа восстановит базовый URL вместо request type.
#### Осторожно!
##### Если вы ранее заменили несколько базовых URL, то восстановление пройдёт некорректно, так как везде заменит на указанный вами базовый URL.

### Восстановление без базового URL

1. Очистите базовый URL в поле `txtUserInput` (Если был введён ранее).
2. Нажмите **"Restore base URL"**.
3. Программа восстановит базовый URL (пустой) вместо request type (Пример - "[GET] google.com" -> "google.com").

## Основные компоненты

- `MainForm` — основной класс, управляющий UI и обработкой файлов.
- `UIController` — вспомогательный класс для управления UI элементами.
- `ProcessFileAsync` — метод обработки JMX-файлов с изменением request type запроса.
- `RestoreFileAsync` — метод восстановления исходного названия перед работой скрипта.
- `ProcessFileWithRegexAsync` — метод работы с регулярными выражениями.

## Регулярные выражения

- `(<HTTPSamplerProxy.*?>.*?</HTTPSamplerProxy>)` — извлекает HTTP-запросы.
- `testname="([^" ]+)"` — находит `testname`.
- `testname="\[(GET|POST|HEAD|PUT|OPTIONS|TRACE|DELETE|PATCH|PROPFIND|PROPPATCH|MKCOL|COPY|MOVE|LOCK|UNLOCK|REPORT|MKCALENDAR|SEARCH)\] (.*?)"` — восстанавливает `testname` к первоначальному виду.

## Ошибки и отладка

- Убедитесь, что файл `.jmx` имеет правильную структуру.
- Проверьте права доступа к файлу.
- Ошибки выводятся в `MessageBox` всплывающие окна.
