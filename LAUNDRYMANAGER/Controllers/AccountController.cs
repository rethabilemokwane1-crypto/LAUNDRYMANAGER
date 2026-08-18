using Microsoft.AspNetCore.Mvc;
using LaundryManager.Data;
using LaundryManager.Models;
using System.Linq;
using BCrypt.Net;

namespace LaundryManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        // This connects our controller directly to the database
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. SHOW LOGIN SCREEN
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 2. PROCESS LOGIN SUBMISSION
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Step 1: find the account by email only (we can't search by hashed password)
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            // Step 2: if found, verify the typed password against the stored hash
            if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                HttpContext.Session.SetString("UserEmail", user.Email);

                // Designating a specific admin email account dynamically
                if (user.Email.StartsWith("000000000"))
                {
                    HttpContext.Session.SetString("UserRole", "Admin");
                    return RedirectToAction("AdminDashboard", "Home");
                }
                else
                {
                    HttpContext.Session.SetString("UserRole", "Student");
                    return RedirectToAction("Index", "Home");
                }
            }

            // If no matching record was found, or the password didn't match
            ModelState.AddModelError("", "Invalid email address or incorrect password.");
            return View(model);
        }

        // 3. SHOW REGISTRATION SCREEN
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 4. PROCESS REGISTRATION SUBMISSION
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verify if this student has already signed up
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            // Create a clean record profile and add it to our database table
            var newUser = new User
            {
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                SecurityAnswer = BCrypt.Net.BCrypt.HashPassword(model.SecurityAnswer.Trim().ToLower())
            };

            _context.Users.Add(newUser);
            _context.SaveChanges(); // Saves changes to the physical file

            // Automatically forward them straight to login page upon success
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    
    // 5. SHOW "FORGOT PASSWORD" - STEP 1: ENTER EMAIL
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // 6. PROCESS EMAIL SUBMISSION -> MOVE TO SECURITY QUESTION
        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check the account actually exists before moving forward
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "No account found with that email address.");
                return View(model);
            }

            // Carry the email forward into the next step's form
            var verifyModel = new VerifyAnswerViewModel { Email = model.Email };
            return View("VerifyAnswer", verifyModel);
        }

        // 7. PROCESS SECURITY ANSWER SUBMISSION -> MOVE TO RESET PASSWORD
        [HttpPost]
        public IActionResult VerifyAnswer(VerifyAnswerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            // Re-check the answer using the same trim/lowercase normalization as Register
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.SecurityAnswer.Trim().ToLower(), user.SecurityAnswer))
            {
                ModelState.AddModelError("SecurityAnswer", "That answer doesn't match our records.");
                return View(model);
            }

            // Correct answer - carry the email forward into the final step
            var resetModel = new ResetPasswordViewModel { Email = model.Email };
            return View("ResetPassword", resetModel);
        }

        // 8. PROCESS NEW PASSWORD SUBMISSION -> SAVE AND REDIRECT TO LOGIN
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Something went wrong. Please start the reset process again.");
                return View(model);
            }

            // Overwrite the old hash with a new one
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }
    }
}