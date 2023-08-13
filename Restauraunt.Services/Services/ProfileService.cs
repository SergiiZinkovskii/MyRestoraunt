﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Restaurant.DAL.Interfaces;
using Restaurant.Domain.Entity;
using Restaurant.Domain.Enum;
using Restaurant.Domain.Response;
using Restaurant.Domain.ViewModel;
using Restaurant.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Services.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ILogger<ProfileService> _logger;
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository,
            ILogger<ProfileService> logger)
        {
            _profileRepository = profileRepository;
            _logger = logger;
        }

        public async Task<Response<ProfileViewModel>> GetProfile(string userName)
        {
            try
            {
                var profile = await _profileRepository.GetAll()
                    .Select(x => new ProfileViewModel()
                    {
                        Id = x.Id,
                        Address = x.Address,
                        Age = x.Age,
                        UserName = x.User.Name
                    })
                    .FirstOrDefaultAsync(x => x.UserName == userName);

                return new Response<ProfileViewModel>()
                {
                    Data = profile,
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ProfileService.GetProfile] error: {ex.Message}");
                return new Response<ProfileViewModel>()
                {
                    StatusCode = StatusCode.InternalServerError,
                    Description = $"Internal error: {ex.Message}"
                };
            }
        }

        public async Task<Response<Profile>> Save(ProfileViewModel model)
        {
            try
            {
                var profile = await _profileRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                profile.Address = model.Address;
                profile.Age = model.Age;

                await _profileRepository.Update(profile);

                return new Response<Profile>()
                {
                    Data = profile,
                    Description = "The data has been updated",
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[ProfileService.Save] error: {ex.Message}");
                return new Response<Profile>()
                {
                    StatusCode = StatusCode.InternalServerError,
                    Description = $"internal error: {ex.Message}"
                };
            }
        }
    }
}
