﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Theatre.Data.Models.Enums;

namespace Theatre.Data.Models
{
    public class Play
    {
        public Play()
        {
            this.Casts = new List<Cast>();
            this.Tickets = new List<Ticket>();
        }
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Title { get; set; }

        [Required]
        
        public TimeSpan Duration { get; set; }

        [Required]
        [Range(typeof(float), "0.00", "10.00")]
        public float Rating { get; set; }

        [Required]
        public Genre Genre { get; set; }

        [Required]
        [StringLength(700)]
        public string Description { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 4)]
        public string Screenwriter { get; set; }

        public ICollection<Cast> Casts { get; set; }

        public ICollection<Ticket> Tickets { get; set; }
    }
}

//Duration – TimeSpan in format
//{ hours: minutes: seconds}, with a minimum length of 1 hour. (required) 


