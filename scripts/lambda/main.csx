#!/home/vscode/.dotnet/tools/dotnet-script

Func<DateTime, bool> canDrive = dob => dob.AddYears(18) <= DateTime.Today;

DateTime dob = new DateTime(2000, 12, 25);
bool result = canDrive(dob);

Console.WriteLine(result);

Action<DateTime> printDate = date => Console.WriteLine(date);

printDate(DateTime.Today);

// Una lambda che prende due parametri stringa (nome e cognome) e restituisce la loro concatenazione
Func<string, string, string> concatFirstAndLastName = (nome, cognome) => $"{nome} {cognome}";
Console.WriteLine(concatFirstAndLastName("Alessandro", "Marchino"));

// Una lamnda che prende tre parametri interi (tre numeri) e restituisce il maggiore dei tre
Func<int, int, int, int> getMaximum = (a, b, c) => Math.Max(a, Math.Max(b, c));
Console.WriteLine(getMaximum(5, 9, 7));

// Una lambda che prende due parametri DateTime e non restituisce nulla, ma stampa la minore delle due date in console con un Console.WriteLine
Action<DateTime, DateTime> printLowerDate = (a, b) => Console.WriteLine(a.CompareTo(b) < 0 ? a : b);
printLowerDate(dob, DateTime.Today);
