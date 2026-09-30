using NGO_website.ViewModels.Admin;

namespace NGO_website.Services.Interfaces
{
    public interface IReportService
    {
        Task<ReportResultViewModel> GenerateReportAsync(ReportsFilterViewModel filter);
        byte[] GenerateCsvExport(ReportResultViewModel report);
    }
}
