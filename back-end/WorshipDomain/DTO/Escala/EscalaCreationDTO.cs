﻿using System.ComponentModel.DataAnnotations;
using WorshipDomain.Enums;

namespace WorshipDomain.DTO.Escala
{
    public class EscalaCreationDTO
    {
        [Required]
        public string Data { get; set; }

        [Required]
        public Evento Evento { get; set; }
    }
}
