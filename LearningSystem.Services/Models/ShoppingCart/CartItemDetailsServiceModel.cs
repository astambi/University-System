namespace LearningSystem.Services.Models.ShoppingCart
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class CartItemDetailsServiceModel : IMapFrom<Course>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
    }
}
