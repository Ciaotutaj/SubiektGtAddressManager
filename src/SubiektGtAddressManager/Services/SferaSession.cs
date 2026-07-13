using System.Runtime.InteropServices;
using InsERT;

namespace SubiektGtAddressManager.Services;

public sealed class SferaSession : IDisposable
{
    private GTClass? _gt;
    private Subiekt? _subiekt;

    public Subiekt Subiekt => _subiekt ?? throw new InvalidOperationException("Brak aktywnego połączenia z Subiektem GT.");

    public bool IsConnected => _subiekt is not null;

    public void Connect(ConnectionParameters parameters)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Sesja jest już połączona.");
        }

        var gt = new GTClass
        {
            Produkt = ProduktEnum.gtaProduktSubiekt,
            Serwer = parameters.Server,
            Baza = parameters.Database,
            Autentykacja = parameters.UseWindowsAuthentication
                ? AutentykacjaEnum.gtaAutentykacjaWindows
                : AutentykacjaEnum.gtaAutentykacjaMieszana,
            Operator = parameters.OperatorLogin,
            OperatorHaslo = parameters.OperatorPassword ?? string.Empty,
        };

        if (!parameters.UseWindowsAuthentication)
        {
            gt.Uzytkownik = parameters.SqlUsername ?? string.Empty;
            gt.UzytkownikHaslo = parameters.SqlPassword ?? string.Empty;
        }

        try
        {
            var application = gt.Uruchom(0, 0);
            var session = application as Subiekt
                ?? throw new InvalidOperationException("Połączono, ale otrzymany obiekt aplikacji nie jest sesją Subiekta GT.");

            _gt = gt;
            _subiekt = session;
        }
        catch (COMException ex)
        {
            if (gt is not null)
            {
                Marshal.ReleaseComObject(gt);
            }

            throw new InvalidOperationException($"Nie udało się połączyć z Subiektem GT: {ex.Message}", ex);
        }
    }

    public void Disconnect()
    {
        if (_subiekt is not null)
        {
            try
            {
                _subiekt.Zakoncz();
            }
            catch (COMException)
            {
            }

            Marshal.ReleaseComObject(_subiekt);
            _subiekt = null;
        }

        if (_gt is not null)
        {
            Marshal.ReleaseComObject(_gt);
            _gt = null;
        }
    }

    public void Dispose() => Disconnect();
}
