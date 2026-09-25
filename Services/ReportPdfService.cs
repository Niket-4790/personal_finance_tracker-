using PersonalFinanceTracker.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PersonalFinanceTracker.Services;

public class ReportPdfService : IReportPdfService
{
    public byte[] GenerateReportPdf(
        List<MonthlyReport> monthlyReports,
        List<CategoryBreakdown> categoryBreakdown,
        decimal savingsRate,
        decimal avgExpense,
        MonthlyReport bestMonth,
        bool isAdmin)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Header()
                    .Text("FinanceTrack Report")
                    .FontSize(24)
                    .Bold();

                page.Content()
                    .PaddingTop(20)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        column.Item()
                            .Text(isAdmin
                                ? "Financial Report - All Users"
                                : "Financial Report")
                            .FontSize(16)
                            .Bold();

                        // Summary
                        column.Item()
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Border(1)
                                    .Padding(10)
                                    .Column(c =>
                                    {
                                        c.Item()
                                            .Text("Savings Rate")
                                            .Bold();

                                        c.Item()
                                            .Text($"{savingsRate:F1}%");
                                    });

                                row.RelativeItem()
                                    .Border(1)
                                    .Padding(10)
                                    .Column(c =>
                                    {
                                        c.Item()
                                            .Text("Best Net Month")
                                            .Bold();

                                        c.Item()
                                            .Text(
                                                $"{bestMonth.MonthName} {bestMonth.Year}");
                                    });

                                row.RelativeItem()
                                    .Border(1)
                                    .Padding(10)
                                    .Column(c =>
                                    {
                                        c.Item()
                                            .Text("Avg Monthly Expense")
                                            .Bold();

                                        c.Item()
                                            .Text(
                                                avgExpense.ToString("C"));
                                    });
                            });

                        // Monthly breakdown
                        column.Item()
                            .Text("Monthly Breakdown")
                            .FontSize(16)
                            .Bold();

                        column.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Month");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Income");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Expenses");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Net Savings");

                                    header.Cell()
                                        .Element(HeaderCell)
                                        .Text("Savings Rate");
                                });

                                foreach (var row in monthlyReports)
                                {
                                    var rate = row.TotalIncome > 0
                                        ? (row.NetSavings / row.TotalIncome) * 100
                                        : 0;

                                    table.Cell()
                                        .Element(BodyCell)
                                        .Text($"{row.MonthName} {row.Year}");

                                    table.Cell()
                                        .Element(BodyCell)
                                        .Text(row.TotalIncome.ToString("C"));

                                    table.Cell()
                                        .Element(BodyCell)
                                        .Text(row.TotalExpenses.ToString("C"));

                                    table.Cell()
                                        .Element(BodyCell)
                                        .Text(row.NetSavings.ToString("C"));

                                    table.Cell()
                                        .Element(BodyCell)
                                        .Text($"{rate:F1}%");
                                }
                            });

                        // Category breakdown
                        column.Item()
                            .Text("Category Breakdown")
                            .FontSize(16)
                            .Bold();

                        if (categoryBreakdown.Any())
                        {
                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Element(HeaderCell)
                                            .Text("Category");

                                        header.Cell()
                                            .Element(HeaderCell)
                                            .Text("Type");

                                        header.Cell()
                                            .Element(HeaderCell)
                                            .Text("Total");
                                    });

                                    foreach (var item in categoryBreakdown)
                                    {
                                        table.Cell()
                                            .Element(BodyCell)
                                            .Text(item.CategoryName);

                                        table.Cell()
                                            .Element(BodyCell)
                                            .Text(item.CategoryType);

                                        table.Cell()
                                            .Element(BodyCell)
                                            .Text(
                                                item.TotalAmount.ToString("C"));
                                    }
                                });
                        }
                        else
                        {
                            column.Item()
                                .Text("No category data available.");
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Generated by FinanceTrack");
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .Border(1)
            .Padding(5);
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .Border(1)
            .Padding(5);
    }
}
