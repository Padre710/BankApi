using BankApi.DTO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace BankApi.Helpers
{
    public static class StatementPdfGenerator
    {
        static StatementPdfGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] Generate(StatementDto s)
        {
            var monthName = new DateTime(s.Year, s.Month, 1)
                .ToString("MMMM yyyy", CultureInfo.InvariantCulture);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("BankApi").Bold().FontSize(20);
                        col.Item().Text($"Account Statement - {monthName}").FontSize(12);
                    });

                    page.Content().PaddingVertical(12).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Text(t =>
                        {
                            t.Span("Account: ").SemiBold();
                            t.Span($"{s.AccountName} ({s.AccountNumber})");
                        });
                        col.Item().Text(t =>
                        {
                            t.Span("Currency: ").SemiBold();
                            t.Span(s.Currency);
                        });
                        col.Item().Text(t =>
                        {
                            t.Span("Period: ").SemiBold();
                            t.Span($"{s.PeriodStart:yyyy-MM-dd} to {s.PeriodEnd:yyyy-MM-dd}");
                        });

                        col.Item().LineHorizontal(1);

                        col.Item().Text(t =>
                        {
                            t.Span("Opening balance: ").SemiBold();
                            t.Span(Money(s.OpeningBalance));
                        });
                        col.Item().Text(t =>
                        {
                            t.Span("Total credits: ").SemiBold();
                            t.Span(Money(s.TotalCredits));
                        });
                        col.Item().Text(t =>
                        {
                            t.Span("Total debits: ").SemiBold();
                            t.Span(Money(s.TotalDebits));
                        });
                        col.Item().Text(t =>
                        {
                            t.Span("Closing balance: ").SemiBold();
                            t.Span(Money(s.ClosingBalance));
                        });

                        col.Item().LineHorizontal(1);

                        if (s.Lines.Count == 0)
                        {
                            col.Item().Text("No transactions in this period.");
                        }
                        else col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);   // Date
                                columns.RelativeColumn(6);   // Description
                                columns.RelativeColumn(2);   // Debit
                                columns.RelativeColumn(2);   // Credit
                                columns.RelativeColumn(2);   // Balance
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Date");
                                header.Cell().Element(HeaderCell).Text("Description");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Debit");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Credit");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Balance");
                            });

                            foreach (var line in s.Lines)
                            {
                                var isDebit = line.Direction == "Debit";

                                table.Cell().Element(BodyCell).Text(line.Date.ToString("yyyy-MM-dd HH:mm"));
                                table.Cell().Element(BodyCell).Text(line.Description);
                                table.Cell().Element(BodyCell).AlignRight().Text(isDebit ? Money(line.Amount) : string.Empty);
                                table.Cell().Element(BodyCell).AlignRight().Text(!isDebit ? Money(line.Amount) : string.Empty);
                                table.Cell().Element(BodyCell).AlignRight().Text(Money(line.BalanceAfter));
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Page ");
                        t.CurrentPageNumber();
                        t.Span(" of ");
                        t.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer HeaderCell(IContainer container) =>
            container.Background(Colors.Grey.Lighten3).Padding(4).DefaultTextStyle(x => x.SemiBold());

        private static IContainer BodyCell(IContainer container) =>
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);

        private static string Money(double value) =>
            value.ToString("N2", CultureInfo.InvariantCulture);

        private static string Ref(Guid? id) =>
            id.HasValue ? id.Value.ToString("N").Substring(0, 8) : "-";
    }
}