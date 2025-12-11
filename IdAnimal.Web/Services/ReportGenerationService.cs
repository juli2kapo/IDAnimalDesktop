using BoldReports.Writer;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using IdAnimal.Shared.DTOs;

namespace IdAnimal.Web.Services
{
    public class ReportGenerationService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportGenerationService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<byte[]> GenerateCattlePdfReportAsync(List<CattleDto> cattleList, string establishmentName)
        {
            // 1. Create DataTable (The "CSV" replacement)
            var dataTable = new DataTable("CattleDataSet");

            // We use String for everything to ensure the RDL accepts it.
            // The RDL handles the conversion to numbers for the calculations.
            dataTable.Columns.Add("Caravan", typeof(string));
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Weight", typeof(string)); 
            dataTable.Columns.Add("Origin", typeof(string));
            dataTable.Columns.Add("Age", typeof(string)); 
            dataTable.Columns.Add("Gender", typeof(string));
            dataTable.Columns.Add("GDM", typeof(string));

            foreach (var c in cattleList)
            {
                dataTable.Rows.Add(
                    c.Caravan ?? "-",
                    c.Name ?? "-", 
                    c.Weight.ToString(), // Pass as string
                    c.Origin ?? "-",
                    c.Age?.ToString() ?? "-",
                    c.Gender ?? "-",
                    c.GDM?.ToString() ?? "-"
                );
            }

            // 2. Setup Report Writer
            var reportWriter = new ReportWriter();
            reportWriter.ReportProcessingMode = ProcessingMode.Local;

            // Load the file
            string reportPath = Path.Combine(_webHostEnvironment.WebRootPath, "Reports", "FINALOJALA.rdl");
            using var fileStream = new FileStream(reportPath, FileMode.Open, FileAccess.Read);
            reportWriter.LoadReport(fileStream);

            // 3. Attach Data Source
            reportWriter.DataSources.Clear();
            reportWriter.DataSources.Add(new BoldReports.Web.ReportDataSource
            {
                Name = "CattleDataSet", // Matches RDL <DataSet Name="CattleDataSet">
                Value = dataTable
            });

            // 4. Generate PDF
            using var memoryStream = new MemoryStream();
            reportWriter.Save(memoryStream, WriterFormat.PDF);
            
            return memoryStream.ToArray();
        }
    }
}