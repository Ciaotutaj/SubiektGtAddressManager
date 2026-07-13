# Zarządzanie adresami kontrahentów — Subiekt GT

Aplikacja desktopowa (WPF, .NET 8) do masowego przeglądania i uzupełniania danych kontrahentów w Subiekcie GT: kodów GLN (siedziba, adres korespondencyjny, adres dostawy) oraz pełnych adresów wymaganych m.in. przy e-fakturowaniu (KSeF). Łączy się bezpośrednio z bazą Subiekta GT przez Sferę (Interop.InsERT4).

## Wymagania

- Windows z zainstalowanym Subiektem GT w wersji **minimum 1.89** (biblioteka `Interop.InsERT4` wymaga zarejestrowanej Sfery na komputerze).
- .NET 8 SDK (do zbudowania) — do samego uruchomienia gotowego `.exe` wystarczy .NET 8 Desktop Runtime.
- Dostęp do serwera SQL, na którym stoi baza Subiekta GT (login Windows albo SQL) oraz login operatora Subiekta GT.

## Uruchomienie

1. Otwórz `SubiektGtAddressManager.sln` w Visual Studio (lub zbuduj z linii poleceń poniżej) i uruchom projekt `SubiektGtAddressManager`.

   Budowanie z linii poleceń:
   ```
   dotnet build SubiektGtAddressManager.sln
   ```
   albo od razu gotowy plik `.exe` do dystrybucji:
   ```
   powershell -File publish.ps1
   ```
   (wynik trafi do katalogu `publish/`).

2. Po uruchomieniu aplikacja pokaże okno logowania — podaj serwer SQL, bazę danych Subiekta GT, sposób uwierzytelnienia (Windows albo SQL) oraz login i hasło operatora Subiekta GT, po czym kliknij **Połącz**.
3. W głównym oknie wczytaj listę kontrahentów (**Odśwież**), uzupełnij kody GLN i adresy, a zmiany zapisz przyciskiem **Zapisz**.

## Bezpieczeństwo

- Zaznaczenie „Zapamiętaj dane logowania” zapisuje hasła **jawnym tekstem** w pliku `config/logowanie.json` obok pliku `.exe` (aplikacja wyraźnie o tym informuje w oknie logowania) — folder `config/` jest objęty `.gitignore`, więc nie trafi do repozytorium, ale mimo to warto mieć to na uwadze na współdzielonym komputerze.
- Folder `logs/` (dziennik zdarzeń aplikacji) jest analogicznie pominięty w Git.
