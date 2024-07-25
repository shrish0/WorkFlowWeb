using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkFlowWeb.Data.DataAccess;
using WorkFlowWeb.Models;

namespace WorkFlowWeb.Areas.admin.Controllers
{
    [Authorize(Roles = "Admin")] // Restrict access to Admin role only
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public CategoryController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View(_db.Categories.ToList());
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                // Check if a category with the same code already exists
                bool codeExists = await _db.Categories.AnyAsync(c => c.Code == category.Code);

                if (codeExists)
                {
                    ModelState.AddModelError("Code", "The Code must be unique.");
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);

                    if (user != null)
                    {
                        category.CreatedBy = user.ApplicationUserId;
                        category.CreatedAt = DateTime.UtcNow;
                        category.ModifiedAt = DateTime.UtcNow;
                        if (!category.IsActive)
                        {
                            category.InactivatedBy = user.ApplicationUserId;
                        }

                        _db.Categories.Add(category);
                        await _db.SaveChangesAsync();

                        TempData["Success"] = "Category has been added successfully.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            // If ModelState is not valid, capture all errors and store in TempData
            TempData["Error"] = "category not created";

            return View(category);
        }



        public IActionResult Edit(int id) 
        {
            Category obj = _db.Categories.Find(id);
            if (obj == null)
            {
                TempData["Error"] = "cant find category";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category obj)
        {
            if (id != obj.CategoryId)
            {
                TempData["Error"] = "Category ID mismatch.";
                return RedirectToAction("Index");
            }

            // Retrieve the original category from the database
            var originalCategory = await _db.Categories.FindAsync(id);

            if (originalCategory == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Index");
            }

            // Update only fields that are editable
            originalCategory.Description = obj.Description;
            originalCategory.IsActive = obj.IsActive;
            originalCategory.ModifiedAt = DateTime.Now;

            if (!originalCategory.IsActive)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);
                originalCategory.InactivatedBy = user.ApplicationUserId;
            }
            else
            {
                originalCategory.InactivatedBy = null;
            }

            // Save changes
            try
            {
                _db.Categories.Update(originalCategory);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Categories.Any(e => e.CategoryId == id))
                {
                    TempData["Error"] = "Category not found.";
                    return RedirectToAction("Index");
                }
                else
                {
                    throw;
                }
            }
        }



        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories
                .Include(c => c.SubCategories) // Load related subcategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _db.Categories
                .Include(c => c.SubCategories) // Load related subcategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction("Index");
            }

            // Remove related subcategories
            _db.SubCategories.RemoveRange(category.SubCategories);

            // Remove the category
            _db.Categories.Remove(category);

            await _db.SaveChangesAsync();
            TempData["Success"] = "Category and related subcategories deleted successfully.";

            return RedirectToAction("Index");
        }


        public async Task<IActionResult> ExportToExcel()
        {
            var categories = await _db.Categories.ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Categories");

                // Add headers
                worksheet.Cell(1, 1).Value = "Code";
                worksheet.Cell(1, 2).Value = "Description";
                worksheet.Cell(1, 3).Value = "Created By";
                worksheet.Cell(1, 4).Value = "Inactivated By";
                worksheet.Cell(1, 5).Value = "Status";
                worksheet.Cell(1, 6).Value = "Created At";
                worksheet.Cell(1, 7).Value = "Modified At";

                // Add data
                for (int i = 0; i < categories.Count; i++)
                {
                    var category = categories[i];
                    worksheet.Cell(i + 2, 1).Value = category.Code;
                    worksheet.Cell(i + 2, 2).Value = category.Description;
                    worksheet.Cell(i + 2, 3).Value = category.CreatedBy;
                    worksheet.Cell(i + 2, 4).Value = category.InactivatedBy ?? "none";
                    worksheet.Cell(i + 2, 5).Value = category.IsActive?"Active":"Blocked" ;
                    worksheet.Cell(i + 2, 6).Value = category.CreatedAt;
                    worksheet.Cell(i + 2, 7).Value = category.ModifiedAt;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Categories.xlsx");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a valid Excel file.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var firstRow = worksheet.Row(1);

                        // Validate headers
                        if (!IsValidHeader(firstRow))
                        {
                            TempData["Error"] = "The Excel file is not in the correct format. Please ensure the headers are: Code, Description, Created By, Inactivated By, Status.";
                            return RedirectToAction(nameof(Index));
                        }

                        var rows = worksheet.RowsUsed().Skip(1); // Skip the header row

                        foreach (var row in rows)
                        {
                            var code = row.Cell(1).GetValue<string>();
                            var description = row.Cell(2).GetValue<string>();
                            var inactivatedBy = row.Cell(4).GetValue<string>();
                            var status = row.Cell(5).GetValue<string>();

                            // Check if the category already exists
                            var existingCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Code == code);
                            if (existingCategory != null)
                            {
                                // Update existing category
                                existingCategory.Description = description;

                                // Only update InactivatedBy if status changes to inactive
                                bool isBecomingInactive = !existingCategory.IsActive && status == "Blocked";
                                if (isBecomingInactive)
                                {
                                    existingCategory.InactivatedBy = inactivatedBy != "none" ? inactivatedBy : null;
                                }

                                existingCategory.IsActive = status == "Active";
                                existingCategory.ModifiedAt = DateTime.UtcNow; // Update the modified date
                            }
                            else
                            {
                                // Add new category
                                var createdBy = row.Cell(3).GetValue<string>();
                                var category = new Category
                                {
                                    Code = code,
                                    Description = description,
                                    CreatedBy = createdBy,
                                    InactivatedBy = inactivatedBy != "none" ? inactivatedBy : null,
                                    IsActive = status == "Active",
                                    CreatedAt = DateTime.UtcNow,
                                    ModifiedAt = DateTime.UtcNow
                                };
                                _db.Categories.Add(category);
                            }
                        }

                        await _db.SaveChangesAsync();
                    }
                }

                TempData["Success"] = "Categories imported successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while importing data: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool IsValidHeader(IXLRow headerRow)
        {
            return headerRow.Cell(1).GetValue<string>().Trim() == "Code"
                && headerRow.Cell(2).GetValue<string>().Trim() == "Description"
                && headerRow.Cell(3).GetValue<string>().Trim() == "Created By"
                && headerRow.Cell(4).GetValue<string>().Trim() == "Inactivated By"
                && headerRow.Cell(5).GetValue<string>().Trim() == "Status";
        }


    }
}
