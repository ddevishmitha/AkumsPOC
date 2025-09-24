using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AkumsPOC.Controllers


{
    [Authorize]
    public class GstController : Controller
    {
        private readonly string pdfDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/PlanPDFs");

        // Map each GST record by GSTIN to a PDF
        private readonly Dictionary<string, string> gstPdfFiles = new Dictionary<string, string>
        {
            { "22AAAAA0000A1Z5", "ABC_Pvt_Ltd.pdf" },
            { "27BBBBB1111B2Z6", "XYZ_Enterprises.pdf" },
            { "07CCCCC2222C3Z7", "Demo_Traders.pdf" }
        };

        private static List<GstRecord> gstRecords = new List<GstRecord>
        {
            new GstRecord { Gstin = "22AAAAA0000A1Z5", Name = "ABC Pvt Ltd", State = "Chattisgarh", Status = "Active" },
            new GstRecord { Gstin = "27BBBBB1111B2Z6", Name = "XYZ Enterprises", State = "Maharashtra", Status = "Active" },
            new GstRecord { Gstin = "07CCCCC2222C3Z7", Name = "Demo Traders", State = "Delhi", Status = "Cancelled" }
        };

        public IActionResult Index()
        {
            return View(gstRecords);
        }

        [HttpPost]
        public IActionResult Search(string searchQuery)
        {
            var filtered = gstRecords.Where(g =>
                g.Gstin.Contains(searchQuery ?? "") || g.Name.Contains(searchQuery ?? "")
            ).ToList();

            if (!filtered.Any())
                return Json(new { success = false, message = "GSTIN or Name not found!" });

            return Json(new { success = true, gstRecords = filtered });
        }

        // Download PDF for a specific GST record
        public IActionResult DownloadPdf(string gstin)
        {
            if (string.IsNullOrEmpty(gstin) || !gstPdfFiles.ContainsKey(gstin))
                return BadRequest("Invalid GSTIN.");

            string fileName = gstPdfFiles[gstin];
            var filePath = Path.Combine(pdfDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", fileName);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }

    public class GstRecord
    {
        public string Gstin { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
    }
}
