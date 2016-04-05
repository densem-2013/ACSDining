﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACSDining.Core.Domains;
using ACSDining.Core.Repositories;
using ACSDining.Core.DTO.SuperUser;
using ACSDining.Repository.Repositories;

namespace ACSDining.Service
{
    public interface IDishService: IService<Dish>
    {
        List<DishModelDto> GetAllDishModelDto();
        List<DishModelDto> GetDishModelDtoByCategory(string category);
        void UpdateDishByDishModel(DishModelDto dmodel);
        //void InsertDishByDishModel(DishModelDto dmodel);
        List<Dish> AllDish();
        Dish GetDishById(int id);
        Task<bool> DeleteDishById(int id);
    }

    public class DishService : Service<Dish>, IDishService
    {
        private readonly IRepositoryAsync<Dish> _repository;

        public DishService(IRepositoryAsync<Dish> repository) : base(repository)
        {
            _repository = repository;
        }

        public List<DishModelDto> GetAllDishModelDto()
        {
            return _repository.AllDishModelDto();
        }

        public List<DishModelDto> GetDishModelDtoByCategory(string category)
        {
            return _repository.DishModelDtoByCategory(category);
        }

        public List<Dish> AllDish()
        {
            return _repository.Queryable().ToList();
        }

        public void UpdateDishByDishModel(DishModelDto dmodel)
        {
            _repository.UpdateDish(dmodel);
        }

        //public void InsertDishByDishModel(DishModelDto dmodel)
        //{
        //    _repository.InsertDish(dmodel);
        //}

        public Dish GetDishById(int id)
        {
            return _repository.Find(id);
        }

        public async void DeleteDishAsync(int dishid)
        {
            await _repository.DeleteAsync(dishid);
        }

        public async Task<bool> DeleteDishById(int id)
        {
            return await _repository.DeleteAsync(id);
        }

    }
}