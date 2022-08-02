# Studia-Picture-Binner-CSharp-Desktop-App

Projekt wykonywany na studiach (w 2019), opis działania niżej.


--------------


Folder Binner zawiera pliki projektu; same źródła napisane przezemnie są w "Binner/Src"
Folder Release zawiera zbuildowaną aplikację.

W aplikacji na początku należy wybrać foldery między którymi będą przenoszone obrazy.
Można pominąć wpisywanie nazwy, będzie wtedy dynamicznie wypełniana nazwą folderu braną z wybieranej ścieżki.
Ścieżkę można wpisać ręcznie/wkleić, lub wybrać z okna dialogowego otwieranego przyciskiem obok.
Checkbox źródło oznacza że z danego folderu będą brane obrazy do sortowania.
Wiersze na wybór kolejnych ścieżek tworzone są dynamicznie, a gdy usunie się nazwę i ścieżkę w danym wierszu jest on automatycznie usuwany.
Można też usunąć dany wiersz przyciskiem na samym końcu wiersza.

Obrazy ładowane są rekursywnie; obsługiwane rozszerzenia: PNG, JPG, JPEG, BMP.

Warunki:
	Muszą zostać wybrane co najmniej dwie lokalizacje, w tym przynajmniej jedno źródło.
	Aplikacja nie pozwoli by jedna ścieżka źródłowa zawierała się w drugiej lub była identyczna. Nie dotyczy to ścieżek nie-źródłowych.
	Nazwy muszą być unikalne, wśród źródeł musi być chociaż jeden obraz.
	Jeśli dana lokacja ma wypełnioną nazwę lub ścieżkę, to musi mieć wypełnione OBA te pola.
	
Po kliknięciu "Załaduj" i pomyślnej walidacji, na górze okna będą przyciski odpowiadające wybranym lokalizacją, z wypisanymi nazwami.
Po najechaniu na nie można też sprawdzić ścieżkę. Klik powoduje skopiowanie obecnie wyświetlanego obrazu do danej lokalizacji.
Można strzałką w lewo na klawiaturze wrócić do poprzedniego obrazu i skopiować go gdzie indziej niż pierwotnie wybrano, czy nawet spowrotem.
Klik przycisku który odpowiada lokalizacji w której obraz już jest zrobi to samo co strzałka w prawo; przejście dalej bez kopiowania.
Przejście do poprzedniego obrazu w przypadku gdy wyświetlany obraz jest pierwszy na liście spowoduje przejście do obrazu ostatniego. 
Przejście do następnego obrazu w przypadku gdy wyświetlany jest obraz ostatni poinformuje o tym użytkownika i wróci do pierwszego obrazu.

Nad obrazem wyświetlana jest jego aktualna (na moment ładowania listy lub kopiowania za pomocą aplikacji) ścieżka i obecna pozycja na liście obrazów, liczba załadowanych obrazów.
Pod obrazem jest przycisk "Powrót", umożliwiający przejście spowrotem na listę lokalizacji.

W przypadku wystąpienia błędu/wyjątku, zostanie wyświetlony komunikat z dostępnymi informacjami o błędzie; w miarę możliwości aplikacja będzie działać dalej.
