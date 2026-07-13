using System.Runtime.InteropServices;
using InsERT;
using Microsoft.Data.SqlClient;
using SubiektGtAddressManager.Models;

namespace SubiektGtAddressManager.Services;

public sealed class ContractorGlnRepository
{
    public void SaveContractor(SferaSession session, ContractorGlnRow row)
    {
        var loaded = session.Subiekt.Kontrahenci.Wczytaj(row.ContractorId);

        if (loaded is not Kontrahent contractor)
        {
            throw new InvalidOperationException($"Nie udało się wczytać kontrahenta '{row.Code}' z Subiekta GT.");
        }

        try
        {
            contractor.GLN = NullIfBlank(row.Gln);
            contractor.Ulica = NullIfBlank(row.HeadOffice.Street);
            contractor.NrDomu = NullIfBlank(row.HeadOffice.HouseNumber);
            contractor.KodPocztowy = NullIfBlank(row.HeadOffice.PostalCode);
            contractor.Miejscowosc = NullIfBlank(row.HeadOffice.City);

            contractor.CrmGLN = NullIfBlank(row.CorrespondenceGln);
            contractor.CrmAdresKorespondencyjny = row.UseCorrespondenceAddress;
            contractor.CrmUlica = NullIfBlank(row.Correspondence.Street);
            contractor.CrmNrDomu = NullIfBlank(row.Correspondence.HouseNumber);
            contractor.CrmKodPocztowy = NullIfBlank(row.Correspondence.PostalCode);
            contractor.CrmMiejscowosc = NullIfBlank(row.Correspondence.City);

            contractor.AdrDostGLN = NullIfBlank(row.DeliveryGln);
            contractor.AdrDostDodawajDoEFaktury = SferaEnumMapper.ToSfera(row.EInvoiceMode);
            contractor.AdrDostUlica = NullIfBlank(row.Delivery.Street);
            contractor.AdrDostNrDomu = NullIfBlank(row.Delivery.HouseNumber);
            contractor.AdrDostKodPocztowy = NullIfBlank(row.Delivery.PostalCode);
            contractor.AdrDostMiejscowosc = NullIfBlank(row.Delivery.City);

            if (row.EInvoiceMode == EInvoiceMode.AsThirdParty)
            {
                contractor.AdrDostRola = SferaEnumMapper.ToSfera(row.DeliveryRole);
            }

            contractor.Zapisz();
        }
        finally
        {
            Marshal.ReleaseComObject(contractor);
        }
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string BuildConnectionString(ConnectionParameters parameters)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = parameters.Server,
            InitialCatalog = parameters.Database,
            TrustServerCertificate = true,
            ConnectTimeout = 15,
        };

        if (parameters.UseWindowsAuthentication)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = parameters.SqlUsername ?? string.Empty;
            builder.Password = parameters.SqlPassword ?? string.Empty;
        }

        return builder.ConnectionString;
    }

    public async Task<List<ContractorGlnRow>> LoadListAsync(string connectionString, CancellationToken ct = default)
    {
        const string sql = """
            SELECT kh.kh_Id      AS ContractorId,
                   kh.kh_Symbol  AS Code,
                   a.adr_Nazwa   AS Name,
                   a.adr_NIP     AS Nip,
                   a.adr_GLN     AS Gln,
                   a.adr_Ulica   AS Street,
                   a.adr_NrDomu  AS HouseNumber,
                   a.adr_Kod     AS PostalCode,
                   a.adr_Miejscowosc AS City,
                   d.adr_GLN     AS DeliveryGln,
                   d.adr_DodawajDoEFakturyJako AS EInvoiceMode,
                   d.adr_Podmiot3Rola AS DeliveryRole,
                   d.adr_Ulica   AS DeliveryStreet,
                   d.adr_NrDomu  AS DeliveryHouseNumber,
                   d.adr_Kod     AS DeliveryPostalCode,
                   d.adr_Miejscowosc AS DeliveryCity,
                   c.adr_GLN     AS CorrespondenceGln,
                   kh.kh_AdresKoresp AS UseCorrespondenceAddress,
                   c.adr_Ulica   AS CorrespondenceStreet,
                   c.adr_NrDomu  AS CorrespondenceHouseNumber,
                   c.adr_Kod     AS CorrespondencePostalCode,
                   c.adr_Miejscowosc AS CorrespondenceCity
            FROM kh__Kontrahent kh
            LEFT JOIN adr__Ewid a ON a.adr_TypAdresu = 1 AND a.adr_IdObiektu = kh.kh_Id
            LEFT JOIN adr__Ewid d ON d.adr_TypAdresu = 11 AND d.adr_IdObiektu = kh.kh_Id
            LEFT JOIN adr__Ewid c ON c.adr_TypAdresu = 2 AND c.adr_IdObiektu = kh.kh_Id
            ORDER BY kh.kh_Symbol
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(ct);

        var result = new List<ContractorGlnRow>();
        while (await reader.ReadAsync(ct))
        {
            string? Text(string column) => reader[column] as string;

            var row = new ContractorGlnRow
            {
                ContractorId = reader.GetInt32(reader.GetOrdinal("ContractorId")),
                Code = reader["Code"] as string ?? string.Empty,
                Name = reader["Name"] as string ?? string.Empty,
                Nip = Text("Nip"),

                Gln = Text("Gln"),
                CorrespondenceGln = Text("CorrespondenceGln"),
                UseCorrespondenceAddress = (reader["UseCorrespondenceAddress"] as bool?) ?? false,

                DeliveryGln = Text("DeliveryGln"),
                EInvoiceMode = (EInvoiceMode)((reader["EInvoiceMode"] as int?) ?? 0),
                DeliveryRole = (DeliveryRole)((reader["DeliveryRole"] as int?) ?? (int)DeliveryRole.Recipient),
            };

            row.HeadOffice.Street = Text("Street");
            row.HeadOffice.HouseNumber = Text("HouseNumber");
            row.HeadOffice.PostalCode = Text("PostalCode");
            row.HeadOffice.City = Text("City");

            row.Correspondence.Street = Text("CorrespondenceStreet");
            row.Correspondence.HouseNumber = Text("CorrespondenceHouseNumber");
            row.Correspondence.PostalCode = Text("CorrespondencePostalCode");
            row.Correspondence.City = Text("CorrespondenceCity");

            row.Delivery.Street = Text("DeliveryStreet");
            row.Delivery.HouseNumber = Text("DeliveryHouseNumber");
            row.Delivery.PostalCode = Text("DeliveryPostalCode");
            row.Delivery.City = Text("DeliveryCity");

            row.MarkSaved();

            result.Add(row);
        }

        return result;
    }
}
