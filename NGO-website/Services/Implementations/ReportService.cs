using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.Admin;
using System.Text;

namespace NGO_website.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportResultViewModel> GenerateReportAsync(ReportsFilterViewModel filter)
        {
            var query = _context.Donations
                .Include(d => d.NGO)
                .Include(d => d.Cause)
                .Include(d => d.Payment)
                .AsQueryable();

            if (filter.FromDate.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(filter.FromDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(d => d.CreatedAt >= fromUtc);
            }

            if (filter.ToDate.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(filter.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                query = query.Where(d => d.CreatedAt <= toUtc);
            }

            if (filter.NGOId.HasValue && filter.NGOId.Value > 0)
            {
                query = query.Where(d => d.NGOId == filter.NGOId.Value);
            }

            if (filter.CauseId.HasValue && filter.CauseId.Value > 0)
            {
                query = query.Where(d => d.CauseId == filter.CauseId.Value);
            }

            if (filter.PaymentMethod.HasValue)
            {
                query = query.Where(d => d.PaymentMethod == filter.PaymentMethod.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(d => d.Status == filter.Status.Value);
            }

            var donations = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

            var rows = donations.Select(d => new ReportRowViewModel
            {
                ReferenceNumber = d.DonationNumber,
                DonorName = d.DonorName,
                DonorEmail = d.DonorEmail,
                NGOName = d.NGO?.Name ?? "N/A",
                CauseName = d.Cause?.Name ?? "N/A",
                Amount = d.Amount,
                PaymentMethod = d.PaymentMethod.ToString(),
                TransactionId = d.Payment?.TransactionId ?? "N/A",
                Status = d.Status.ToString(),
                Date = d.CreatedAt
            }).ToList();

            return new ReportResultViewModel
            {
                Filter = filter,
                TotalRecords = rows.Count,
                TotalAmount = rows.Where(r => r.Status == "Approved").Sum(r => r.Amount),
                Rows = rows
            };
        }

        public byte[] GenerateCsvExport(ReportResultViewModel report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Donation Number,Donor Name,Donor Email,NGO,Cause,Amount (PKR),Payment Method,Transaction ID,Status,Date");

            foreach (var row in report.Rows)
            {
                var safeDonor = EscapeCsv(row.DonorName);
                var safeEmail = EscapeCsv(row.DonorEmail);
                var safeNgo = EscapeCsv(row.NGOName);
                var safeCause = EscapeCsv(row.CauseName);
                var safeTx = EscapeCsv(row.TransactionId);

                sb.AppendLine($"{row.ReferenceNumber},{safeDonor},{safeEmail},{safeNgo},{safeCause},{row.Amount:F2},{row.PaymentMethod},{safeTx},{row.Status},{row.Date:yyyy-MM-dd HH:mm:ss}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string EscapeCsv(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return $"\"{field}\"";
        }
    }
}
