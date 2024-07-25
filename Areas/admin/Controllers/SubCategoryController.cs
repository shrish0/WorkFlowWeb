using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkFlowWeb.Data.DataAccess;
using WorkFlowWeb.Migrations;
using WorkFlowWeb.Models;
using WorkFlowWeb.ViewModels;

namespace WorkFlowWeb.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubCategoryController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: SubCategory
        public async Task<IActionResult> Index()
        {
            var subCategories = await _db.SubCategories
                .Include(sc => sc.Category) // Include the Category entity
                .Where(sc => sc.Category.IsActive) // Filter for active categories
                .Select(sc => new SubCategoryViewModel
                {
                    SubCategoryId = sc.SubCategoryId,
                    Code = sc.Code,
                    Description = sc.Description,
                    CreatedBy = sc.CreatedBy,
                    CreatedAt = sc.CreatedAt,
                    ModifiedAt = sc.ModifiedAt,
                    IsActive = sc.IsActive,
                    InactivatedBy = sc.InactivatedBy,
                    CategoryId = sc.CategoryId,
                    CategoryCode = sc.Category.Code // Include Category code
                })
                .ToListAsync();

            // Debugging: Log or inspect the result
            foreach (var subCategory in subCategories)
            {
                Console.WriteLine($"SubCategory: {subCategory.Code}, Category Code: {subCategory.CategoryCode}");
            }

            return View(subCategories);
        }


        // GET: SubCategory/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_db.Categories.Where(c => c.IsActive), "CategoryId", "Code");
            return View();
        }

        // POST: SubCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategory subCategory)
        {
            if (ModelState.IsValid)
            {
                var category = await _db.Categories.SingleOrDefaultAsync(c => c.CategoryId == subCategory.CategoryId && c.IsActive);

                if (category == null)
                {
                    ModelState.AddModelError("CategoryId", "Selected category is not active.");
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);

                    if (user != null)
                    {
                        subCategory.CreatedBy = user.ApplicationUserId;
                        subCategory.CreatedAt = DateTime.UtcNow;

                        _db.SubCategories.Add(subCategory);
                        await _db.SaveChangesAsync();
                        TempData["Success"] = "SubCategory has been added successfully.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "User not found.");
                }
            }

            ViewData["CategoryId"] = new SelectList(_db.Categories.Where(c => c.IsActive), "CategoryId", "Code", subCategory.CategoryId);
            return View(subCategory);
        }


        // GET: SubCategory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var subCategory = await _db.SubCategories.FindAsync(id);
            if (subCategory == null)
            {
                TempData["Error"] = "SubCategory not found.";
                return RedirectToAction(nameof(Index));
            }
             var categories = _db.Categories
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Code + " - " + c.Description,
                    Selected = c.CategoryId == subCategory.CategoryId // Set the selected item
                })
                .ToList();

            ViewData["Categories"] = categories;
            return View(subCategory);
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategory subCategory)
        {
            if (id != subCategory.SubCategoryId)
            {
                TempData["Error"] = "ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

          

            if (ModelState.IsValid)
            {
                var category = await _db.Categories
                    .AsNoTracking()
                    .SingleOrDefaultAsync(c => c.CategoryId == subCategory.CategoryId && c.IsActive);

                if (category == null)
                {
                    ModelState.AddModelError("CategoryId", "Selected category is not active.");
                }
                else
                {
                    var originalSubCategory = await _db.SubCategories.FindAsync(id);
                  

                    if (originalSubCategory == null)
                    {
                        TempData["Error"] = "SubCategory not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    try
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Id == userId);

                        if (user != null)
                        {
                            // Update only the fields that are allowed to be modified
                            originalSubCategory.Description = subCategory.Description;
                            originalSubCategory.IsActive = subCategory.IsActive;
                            originalSubCategory.ModifiedAt = DateTime.UtcNow;
                            originalSubCategory.Code = subCategory.Code;
                            originalSubCategory.CategoryId = subCategory.CategoryId;

                            if (!subCategory.IsActive)
                            {
                                originalSubCategory.InactivatedBy = user.ApplicationUserId;
                            }

                            await _db.SaveChangesAsync();
                            TempData["Success"] = "SubCategory updated successfully.";
                            return RedirectToAction(nameof(Index));
                        }

                        ModelState.AddModelError(string.Empty, "User not found.");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SubCategoryExists(subCategory.SubCategoryId))
                        {
                            TempData["Error"] = "SubCategory not found.";
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            var categories = _db.Categories
               .Where(c => c.IsActive)
               .Select(c => new SelectListItem
               {
                   Value = c.CategoryId.ToString(),
                   Text = c.Code + " - " + c.Description,
                   Selected = c.CategoryId == subCategory.CategoryId // Set the selected item
               })
               .ToList();

            ViewData["Categories"] = categories;
            return View(subCategory);
        }




        // GET: SubCategory/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var subCategory = await _db.SubCategories.Include(sc => sc.Category).FirstOrDefaultAsync(sc => sc.SubCategoryId == id);
            if (subCategory == null)
            {
                TempData["Error"] = "SubCategory not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(subCategory);
        }

        // POST: SubCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCategory = await _db.SubCategories.FindAsync(id);
            if (subCategory != null)
            {
                _db.SubCategories.Remove(subCategory);
                await _db.SaveChangesAsync();
                TempData["Success"] = "SubCategory deleted successfully.";
            }
            else
            {
                TempData["Error"] = "SubCategory not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SubCategoryExists(int id)
        {
            return _db.SubCategories.Any(e => e.SubCategoryId == id);
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var subCategories = await _db.SubCategories
                                 .Include(sc => sc.Category) // Include the Category entity
                                 .Select(sc => new SubCategoryViewModel
                                 {
                                     SubCategoryId = sc.SubCategoryId,
                                     Code = sc.Code,
                                     Description = sc.Description,
                                     CreatedBy = sc.CreatedBy,
                                     CreatedAt = sc.CreatedAt,
                                     ModifiedAt = sc.ModifiedAt,
                                     IsActive = sc.IsActive,
                                     InactivatedBy = sc.InactivatedBy,
                                     CategoryId = sc.CategoryId,
                                     CategoryCode = sc.Category.Code // Include Category code
                                 })
                                 .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SubCategories");

                // Add headers
                worksheet.Cell(1, 1).Value = "Category Code";
                worksheet.Cell(1, 2).Value = "SubCategory Code";
                worksheet.Cell(1, 3).Value = "Description";
                worksheet.Cell(1, 4).Value = "Created By";
                worksheet.Cell(1, 5).Value = "Inactivated By";
                worksheet.Cell(1, 6).Value = "Is Active";
                worksheet.Cell(1, 7).Value = "Created At";
                worksheet.Cell(1, 8).Value = "Modified At";

                // Add data
                for (int i = 0; i < subCategories.Count; i++)
                {
                    var subCategory = subCategories[i];
                    worksheet.Cell(i + 2, 1).Value = subCategory.CategoryCode;
                    worksheet.Cell(i + 2, 2).Value = subCategory.Code;
                    worksheet.Cell(i + 2, 3).Value = subCategory.Description;
                    worksheet.Cell(i + 2, 4).Value = subCategory.CreatedBy;
                    worksheet.Cell(i + 2, 5).Value = subCategory.InactivatedBy ?? "none";
                    worksheet.Cell(i + 2, 6).Value = subCategory.IsActive;
                    worksheet.Cell(i + 2, 7).Value = subCategory.CreatedAt;
                    worksheet.Cell(i + 2, 8).Value = subCategory.ModifiedAt;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SubCategories.xlsx");
                }
            }
        }
    }
}
