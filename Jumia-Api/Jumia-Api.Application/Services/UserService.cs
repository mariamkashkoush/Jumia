
﻿using Jumia_Api.Application.Dtos.AuthDtos;


﻿using AutoMapper;
using Jumia_Api.Application.Dtos.UserDtos;
using Jumia_Api.Application.Interfaces;
using Jumia_Api.Domain.Interfaces.UnitOfWork;



using Jumia_Api.Domain.Models;
using Microsoft.AspNetCore.Identity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jumia_Api.Application.Dtos.CustomerDtos;

namespace Jumia_Api.Application.Services
{

    public class UserService : IUserService
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;


        public UserService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _roleManager = roleManager;
        }
        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            return _mapper.Map<UserProfileDto>(user);
        }

        public async Task<IdentityResult> CreateUserAsync(string email, string password, string firstName, string lastName, DateTime birthDate, string gender, string address)
        {
            var user = new AppUser
            {
                Email = email,
                UserName = firstName + Guid.NewGuid().ToString(),
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = birthDate,
                Address = address,
                Gender = gender // or "Male", "Other", etc.
            };
            return await _userManager.CreateAsync(user, password);

        }

        public Task<AppUser> FindByEmailAsync(string email)
        {
            return _userManager.FindByEmailAsync(email);
        }
        public async Task<string?> GetUserRoleAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;

        }
        public async Task UpdateUserProfileAsync(string userId, UpdateUserDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            _mapper.Map(updateDto, user);
            user.Id = userId;
            await _userManager.UpdateAsync(user);

        }
        public async Task<AppUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateUserAsync(AppUser user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task AddUserToRoleAsync(AppUser user, string role)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
          var customers =  await _unitOfWork.CustomerRepo.GetAllCustomersAsync();

            var customersDto = _mapper.Map<IEnumerable<CustomerDTO>>(customers);
            return customersDto;

        }

        public async Task<IEnumerable<UserProfileDto>> GetAllSellersAsync()
        {
            var sellerRole = "seller";
            var role = await _roleManager.FindByNameAsync(sellerRole);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role {sellerRole} not found.");
            }

            var userInRole = await _userManager.GetUsersInRoleAsync(sellerRole);

            return userInRole.Select(user => _mapper.Map<UserProfileDto>(user));

        }

        public async Task<IEnumerable<UserProfileDto>> GetAllAdminAsync()
        {
            var adminRole = "admin";
            var role = await _roleManager.FindByNameAsync(adminRole);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role {adminRole} not found.");
            }

            var userInRole = await _userManager.GetUsersInRoleAsync(adminRole);

            return userInRole.Select(user => _mapper.Map<UserProfileDto>(user));

        }

        public async Task<string> GeneratePasswordResetTokenAsync(AppUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return token;
            
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }


        public async Task<bool> ToogleBlockStatusAsync(int customerId)
        {
            var isBlocked = await _unitOfWork.CustomerRepo.ToggleBlockStatusAsync(customerId);
            
            return isBlocked;
        }
    }
}
