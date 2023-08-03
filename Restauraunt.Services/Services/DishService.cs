﻿using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Interfaces;
using Restaurant.DAL.Repositories;
using Restaurant.Domain.Entity;
using Restaurant.Domain.Enum;
using Restaurant.Domain.Extensions;
using Restaurant.Domain.Response;
using Restaurant.Domain.ViewModel;
using Restaurant.Services.Interfaces;

namespace Restaurant.Services.Services
{
    public class DishService : IDishService
    {
        private readonly IDishRepository _dishRepository;
        private readonly ICommentRepository _commentRepository;
        public DishService(IDishRepository dishRepository, ICommentRepository commentRepository)
        {
            _dishRepository = dishRepository;
            _commentRepository = commentRepository;
        }

        public BaseResponse<Dictionary<int, string>> GetTypes()
        {
            try
            {
                var types = ((Category[])Enum.GetValues(
                        typeof(Category)))
                    .ToDictionary(k => (int)k, t => t.GetDisplayName());

                return new BaseResponse<Dictionary<int, string>>()
                {
                    Data = types,
                    StatusCode = StatusCode.OK
                };
            }

            catch (Exception ex)
            {
                return new BaseResponse<Dictionary<int, string>>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public IBaseResponse<List<Dish>> GetDishes()
        {
            try
            {
                
                var dishes = _dishRepository.GetAll()
                    .Include(p => p.DishPhotos)
                    .ToList();

                if (!dishes.Any())
                {
                    return new BaseResponse<List<Dish>>()
                    {
                        Description = "We find 0 elements",
                        StatusCode = StatusCode.OK
                    };
                }

                return new BaseResponse<List<Dish>>()
                {
                    Data = dishes,
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<Dish>>()
                {
                    Description = $"[GetProducts] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<IBaseResponse<Dish>> Create(DishViewModel model, List<byte[]> imageDataList)
        {
            try
            {
                var dish = new Dish()
                {
                    Name = model.Name,
                    Description = model.Description,
                    DateCreate = DateTime.Now,
                    //Category = Enum.Parse<Category>(model.Category),
                    Price = model.Price,
                    DishPhotos = new List<DishPhoto>()
                };

                foreach (var imageData in imageDataList)
                {
                    var photo = new DishPhoto()
                    {
                        ImageData = imageData
                    };
                    dish.DishPhotos.Add(photo);
                }

                await _dishRepository.Create(dish);

                return new BaseResponse<Dish>()
                {
                    StatusCode = StatusCode.OK,
                    Data = dish
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<Dish>()
                {
                    Description = $"[CreateAsync] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

  

        public async Task<DishViewModel?> GetOneDishAsync(long id, CancellationToken cancellationToken)
        {
            var dish = await _dishRepository.Find(id, cancellationToken);

            if (dish == null)
            {
                return null;
            }

            return new DishViewModel()
            {
                Id = dish.Id,
                //DateCreate = dish.DateCreate.ToLongDateString(),
                Description = dish.Description,
                Name = dish.Name,
                Price = dish.Price,
                Comments = await _commentRepository.FindAsync(dish.Id),
                Photos = dish.DishPhotos.Select(p => p.ImageData).ToList()
            };
        }

        public async Task<BaseResponse<Dictionary<long, string>>> GetOneDishAsync(string term)
        {
            var baseResponse = new BaseResponse<Dictionary<long, string>>();
            try
            {
                var products = await _dishRepository.GetAll()
                    .Select(x => new DishViewModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        DateCreate = x.DateCreate.ToLongDateString(),
                        Price = x.Price,
                        Category = x.Category.GetDisplayName()
                    })
                    .Where(x => EF.Functions.Like(x.Name, $"%{term}%"))
                    .ToDictionaryAsync(x => x.Id, t => t.Name);

                baseResponse.Data = products;
                return baseResponse;
            }
            catch (Exception ex)
            {
                return new BaseResponse<Dictionary<long, string>>()
                {
                    Description = ex.Message,
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<IBaseResponse<bool>> DeleteDish(long id)
        {
            try
            {
                var product = await _dishRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
                if (product == null)
                {
                    return new BaseResponse<bool>()
                    {
                        Description = "Entity not found",
                        StatusCode = StatusCode.UserNotFound,
                        Data = false
                    };
                }

                await _dishRepository.Delete(product);

                return new BaseResponse<bool>()
                {
                    Data = true,
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Description = $"[DeleteDish] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<IBaseResponse<Dish>> Edit(DishViewModel model, long Id)
        {
            try
            {
                var dish = await _dishRepository.GetAll().FirstOrDefaultAsync(x => x.Id == Id);
                if (dish == null)
                {
                    return new BaseResponse<Dish>()
                    {
                        Description = "Dish not found",
                        StatusCode = StatusCode.ProductNotFound
                    };
                }

                dish.Description = model.Description;
                dish.Price = model.Price;
                
                dish.DateCreate = DateTime.Now;
                dish.Name = model.Name;

                await _dishRepository.Update(dish);


                return new BaseResponse<Dish>()
                {
                    Data = dish,
                    StatusCode = StatusCode.OK,
                };
               
            }
            catch (Exception ex)
            {
                return new BaseResponse<Dish>()
                {
                    Description = $"[Edit] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
    }
}
