using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookEater.Data;
using BookEater.Models;

namespace BookEater.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _db;

        public BooksController(ApplicationDbContext context)
        {
            _db = context;
        }

        // GET: Books
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var myUserId = GetCurrentUserId();
            if (myUserId == null) return RedirectToAction("Login", "Account");

            var libraryEntries = await _db.UserBooks
                .Include(x => x.Book)
                .Where(x => x.UserId == myUserId)
                .OrderBy(x => x.ListName)
                .ToListAsync();

            return View(libraryEntries);
        }

        // GET: Books/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bookDetail = await _db.Books
                .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (bookDetail == null) return NotFound();

            return View(bookDetail);
        }

        // GET: Books/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,Title,Genre,Author,Rating")] Book newBook, string? ListName)
        {
            if (ModelState.IsValid)
            {
                _db.Add(newBook);
                await _db.SaveChangesAsync();

                var entry = new LibraryEntry
                {
                    UserId = GetCurrentUserId(), 
                    BookId = newBook.BookId,
                    ListName = string.IsNullOrEmpty(ListName) ? "General" : ListName
                };

                _db.UserBooks.Add(entry);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(newBook);
        }

        // GET: Books/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bookToEdit = await _db.Books.FindAsync(id);
            if (bookToEdit == null) return NotFound();

            var currentEntry = await _db.UserBooks
                .FirstOrDefaultAsync(x => x.BookId == id && x.UserId == GetCurrentUserId());

            if (currentEntry != null)
            {
                ViewData["ListName"] = currentEntry.ListName;
            }

            return View(bookToEdit);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Title,Genre,Author,Rating")] Book bookData, string? ListName)
        {
            if (id != bookData.BookId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(bookData);

                    var entryToUpdate = await _db.UserBooks
                        .FirstOrDefaultAsync(x => x.BookId == id && x.UserId == GetCurrentUserId());

                    if (entryToUpdate != null)
                    {
                        entryToUpdate.ListName = string.IsNullOrEmpty(ListName) ? "General" : ListName;
                        _db.Update(entryToUpdate);
                    }

                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(bookData.BookId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bookData);
        }

        // GET: Books/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bookToDelete = await _db.Books
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (bookToDelete == null) return NotFound();

            return View(bookToDelete);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book != null)
            {
                var links = _db.UserBooks.Where(x => x.BookId == id);
                _db.UserBooks.RemoveRange(links);

                var reviews = _db.Reviews.Where(x => x.BookId == id);
                _db.Reviews.RemoveRange(reviews);

                _db.Books.Remove(book);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Books/WriteReview/5
        [HttpGet]
        public async Task<IActionResult> WriteReview(int? id)
        {
            if (id == null) return NotFound();

            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/WriteReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WriteReview(int BookId, int Rating, string Content)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var review = new Review
            {
                BookId = BookId,
                UserId = userId,
                Rating = Rating, 
                Content = Content,
                DatePosted = DateTime.Now
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Books/MyReviews
        [HttpGet]
        public async Task<IActionResult> MyReviews()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            var myReviews = await _db.Reviews
                .Include(r => r.Book) 
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.DatePosted) 
                .ToListAsync();

            return View(myReviews);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteReview(int? id)
        {
            if (id == null) return NotFound();

            var review = await _db.Reviews
                .Include(r => r.Book) 
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null) return NotFound();

            if (review.UserId != GetCurrentUserId())
            {
                return RedirectToAction(nameof(Index)); 
            }

            return View(review);
        }

        // POST: Books/DeleteReview/5
        [HttpPost, ActionName("DeleteReview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReviewConfirmed(int id)
        {
            var review = await _db.Reviews.FindAsync(id);

            if (review != null && review.UserId == GetCurrentUserId())
            {
                _db.Reviews.Remove(review);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyReviews));
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private bool BookExists(int id)
        {
            return _db.Books.Any(e => e.BookId == id);
        }
    }
}