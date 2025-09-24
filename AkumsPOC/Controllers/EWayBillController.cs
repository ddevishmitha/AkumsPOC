using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;

namespace AkumsPOC.Controllers
{
    [Authorize] //  Restrict entire controller to logged-in users
    public class EWayBillController : Controller
    {
        // Fake sample data (replace with DB later)
        private List<EWayBillRecord> GetSampleData(int month, int year)
        {
            var records = new List<EWayBillRecord>();
            for (int i = 1; i <= 10; i++)
            {
                records.Add(new EWayBillRecord
                {
                    BillNo = $"EB{year}{month:D2}{i:000}",
                    Date = new DateTime(year, month, i),
                    Supplier = $"Supplier {i}",
                    Receiver = $"Receiver {i}",
                    Amount = 1000 + i * 50
                });
            }
            return records;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(int month, int year)
        {
            var records = GetSampleData(month, year);
            TempData["Message"] = $"Found {records.Count} E-Waybills for {System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}.";
            TempData["Month"] = month;
            TempData["Year"] = year;
            return View();
        }

        [HttpPost]
        public IActionResult ExportToExcel(int month, int year)
        {
            var records = GetSampleData(month, year);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("EWayBill");
                worksheet.Cell(1, 1).Value = "Bill No";
                worksheet.Cell(1, 2).Value = "Date";
                worksheet.Cell(1, 3).Value = "Supplier";
                worksheet.Cell(1, 4).Value = "Receiver";
                worksheet.Cell(1, 5).Value = "Amount";

                int row = 2;
                foreach (var r in records)
                {
                    worksheet.Cell(row, 1).Value = r.BillNo;
                    worksheet.Cell(row, 2).Value = r.Date.ToString("dd-MM-yyyy");
                    worksheet.Cell(row, 3).Value = r.Supplier;
                    worksheet.Cell(row, 4).Value = r.Receiver;
                    worksheet.Cell(row, 5).Value = r.Amount;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"EWayBill-{year}-{month:D2}.xlsx";
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
        }
    }

    // Model
    public class EWayBillRecord
    {
        public string BillNo { get; set; }
        public DateTime Date { get; set; }
        public string Supplier { get; set; }
        public string Receiver { get; set; }
        public decimal Amount { get; set; }
    }
}
