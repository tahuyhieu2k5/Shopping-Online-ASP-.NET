using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/User")]
	//[Authorize(Roles = "Admin")]
	public class UserController : Controller
	{
		private readonly UserManager<AppUserModel> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly DataContext _dataContext;



		public UserController(DataContext context,
			UserManager<AppUserModel> userManager,
			RoleManager<IdentityRole> roleManager
			)
		{

			_userManager = userManager;
			_roleManager = roleManager;
			_dataContext = context;

		}
		[HttpGet]
		[Route("Index")]
		public async Task<IActionResult> Index()
		{
			var usersWithRoles = await (from u in _dataContext.Users
										join ur in _dataContext.UserRoles on u.Id equals ur.UserId
										join r in _dataContext.Roles on ur.RoleId equals r.Id
										select new { User = u, RoleName = r.Name })
							   .ToListAsync();

			return View(usersWithRoles);
		}
		[HttpGet]
		[Route("Create")]
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(new AppUserModel());
		}
		[HttpGet]
		[Route("Edit")]
		public async Task<IActionResult> Edit(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");

			return View(user);
		}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Cập nhật các thông tin khác
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                // ⚠️ Xử lý cập nhật Role
                var currentRoles = await _userManager.GetRolesAsync(existingUser);
                await _userManager.RemoveFromRolesAsync(existingUser, currentRoles); // Xóa role cũ

                var newRole = await _roleManager.FindByIdAsync(user.RoleId); // Lấy role mới
                if (newRole != null)
                {
                    await _userManager.AddToRoleAsync(existingUser, newRole.Name); // Gán role mới
                }

                var updateUserResult = await _userManager.UpdateAsync(existingUser); // Cập nhật user

                if (updateUserResult.Succeeded)
                {
                    TempData["success"] = "User updated successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(existingUser);
        }

        private void AddIdentityErrors(IdentityResult identityResult)
		{
			foreach (var error in identityResult.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Create")]
		public async Task<IActionResult> Create(AppUserModel user)
		{
			if (ModelState.IsValid)
			{
				var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash); //tạo user
				if (createUserResult.Succeeded)
				{
					var createUser = await _userManager.FindByEmailAsync(user.Email); //tìm user dựa vào email
					var userId = createUser.Id; // lấy user Id
					var role = _roleManager.FindByIdAsync(user.RoleId); //lấy RoleId
																		//gán quyền
					var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
					if (!addToRoleResult.Succeeded)
					{
						foreach (var error in createUserResult.Errors)
						{
							ModelState.AddModelError(string.Empty, error.Description);
						}
					}

					return RedirectToAction("Index", "User");
				}
				else
				{

					foreach (var error in createUserResult.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
					return View(user);
				}

			}
			else
			{
				TempData["error"] = "Model có một vài thứ đang lỗi";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				return BadRequest(errorMessage);
			}
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");
			return View(user);

		}

		[HttpGet]
		[Route("Delete")]
		public async Task<IActionResult> Delete(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			var deleteResult = await _userManager.DeleteAsync(user);
			if (!deleteResult.Succeeded)
			{
				return View("Error");
			}
			TempData["success"] = "User đã được xóa thành công";
			return RedirectToAction("Index");
		}

	}
}
