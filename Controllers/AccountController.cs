using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
	public class AccountController : Controller
	{
		private UserManager<AppUserModel> _userManage;
		private SignInManager<AppUserModel> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly DataContext _dataContext;
		public AccountController(IEmailSender emailSender, UserManager<AppUserModel> userManage,
			SignInManager<AppUserModel> signInManager, DataContext context)
		{
			_userManage = userManage;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_dataContext = context;

		}
		public IActionResult Login(string returnUrl)
		{
			return View(new LoginViewModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
		{
			var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

			if (checkMail == null)
			{
				TempData["error"] = "Email not found";
				return RedirectToAction("ForgotPass", "Account");
			}
			else
			{
				string token = Guid.NewGuid().ToString();
				//update token to user
				checkMail.Token = token;
				_dataContext.Update(checkMail);
				await _dataContext.SaveChangesAsync();
				var receiver = checkMail.Email;
				var subject = "Change password for user " + checkMail.Email;
				var message = "Click on link to change password " +
					"<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "&token=" + token + "'>";

				await _emailSender.SendEmailAsync(receiver, subject, message);
			}


			TempData["success"] = "An email has been sent to your registered email address with password reset instructions.";
			return RedirectToAction("ForgotPass", "Account");
		}
		public IActionResult ForgotPass()
		{
			return View();
		}
		public async Task<IActionResult> NewPass(AppUserModel user, string token)
		{
			var checkuser = await _userManage.Users
				.Where(u => u.Email == user.Email)
				.Where(u => u.Token == user.Token).FirstOrDefaultAsync();

			if (checkuser != null)
			{
				ViewBag.Email = checkuser.Email;
				ViewBag.Token = token;
			}
			else
			{
				TempData["error"] = "Email not found or token is not right";
				return RedirectToAction("ForgotPass", "Account");
			}
			return View();
		}
		public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
		{
			var checkuser = await _userManage.Users
				.Where(u => u.Email == user.Email)
				.Where(u => u.Token == user.Token).FirstOrDefaultAsync();

			if (checkuser != null)
			{
				//update user with new password and token
				string newtoken = Guid.NewGuid().ToString();
				// Hash the new password
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

				checkuser.PasswordHash = passwordHash;
				checkuser.Token = newtoken;

				await _userManage.UpdateAsync(checkuser);
				TempData["success"] = "Password updated successfully.";
				return RedirectToAction("Login", "Account");
			}
			else
			{
				TempData["error"] = "Email not found or token is not right";
				return RedirectToAction("ForgotPass", "Account");
			}
			return View();
		}
		public async Task<IActionResult> History()
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account"); // Replace "Account" with your controller name
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var userEmail = User.FindFirstValue(ClaimTypes.Email);

			var Orders = await _dataContext.Orders
				.Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
			ViewBag.UserEmail = userEmail;
			return View(Orders);
		}

		public async Task<IActionResult> CancelOrder(string ordercode)
		{
			if ((bool)!User.Identity?.IsAuthenticated)
			{
				// User is not logged in, redirect to login
				return RedirectToAction("Login", "Account");
			}
			try
			{
				var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
				order.Status = 3;
				_dataContext.Update(order);
				await _dataContext.SaveChangesAsync();
			}
			catch (Exception ex)
			{

				return BadRequest("An error occurred while canceling the order.");
			}


			return RedirectToAction("History", "Account");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel loginVM)
		{
			if (ModelState.IsValid)
			{
				Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
				if (result.Succeeded)
				{
					//TempData["success"] = "Đăng nhập thành công";
					//var receiver = "demologin979@gmail.com";
					//var subject = "Đăng nhập trên thiết bị thành công.";
					//var message = "Đăng nhập thành công, trải nghiệm dịch vụ nhé.";

					//await _emailSender.SendEmailAsync(receiver, subject, message);
					return Redirect(loginVM.ReturnUrl ?? "/");
				}
				ModelState.AddModelError("", "Sai tài khoản hặc mật khẩu");
			}
			return View(loginVM);
		}


		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(UserModel user)
		{
			if (ModelState.IsValid)
			{
				AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email };
				IdentityResult result = await _userManage.CreateAsync(newUser, user.Password);
				if (result.Succeeded)
				{
                    await _userManage.AddToRoleAsync(newUser, "User");

                    TempData["success"] = "Tạo thành viên thành công";
					return Redirect("/account/login");
				}
				foreach (IdentityError error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			return View(user);
		}


		public async Task<IActionResult> Logout(string returnUrl = "/")
		{
			await _signInManager.SignOutAsync();
			await HttpContext.SignOutAsync();
			return Redirect(returnUrl);
		}

		public async Task LoginByGoogle()
		{
			// Use Google authentication scheme for challenge
			await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
				new AuthenticationProperties
				{
					RedirectUri = Url.Action("GoogleResponse")
				});
		}

		public async Task<IActionResult>
		 GoogleResponse()
		{
			// Authenticate using Google scheme
			var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

			if (!result.Succeeded)
			{
				//Nếu xác thực ko thành công quay về trang Login
				return RedirectToAction("Login");
			}

			var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
			{
				claim.Issuer,
				claim.OriginalIssuer,
				claim.Type,
				claim.Value
			});

			var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			//var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
			string emailName = email.Split('@')[0];
			//return Json(claims);
			// Check user có tồn tại không
			var existingUser = await _userManage.FindByEmailAsync(email);

			if (existingUser == null)
			{
				//nếu user ko tồn tại trong db thì tạo user mới với password hashed mặc định 1-9
				var passwordHasher = new PasswordHasher<AppUserModel>();
				var hashedPassword = passwordHasher.HashPassword(null, "123456789");
				//username thay khoảng cách bằng dấu "-" và chữ thường hết
				var newUser = new AppUserModel { UserName = emailName, Email = email };
				newUser.PasswordHash = hashedPassword; // Set the hashed password cho user

				var createUserResult = await _userManage.CreateAsync(newUser);
				if (!createUserResult.Succeeded)
				{
					TempData["error"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
					return RedirectToAction("Login", "Account"); // Trả về trang đăng ký nếu fail

				}
				else
				{
                    // Nếu user tạo user thành công thì đăng nhập luôn 
                    await _userManage.AddToRoleAsync(newUser, "USER");
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
					TempData["success"] = "Đăng ký tài khoản thành công.";
					return RedirectToAction("Index", "Home");
				}

			}
			else
			{
				//Còn user đã tồn tại thì đăng nhập luôn với existingUser
				await _signInManager.SignInAsync(existingUser, isPersistent: false);
			}

			return RedirectToAction("Login");

		}


	}
}
