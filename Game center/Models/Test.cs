﻿using System.ComponentModel.DataAnnotations;

namespace GameCenter.Models
{
    public class Test //Создание пользователя (временная регистрация)
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }


        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
