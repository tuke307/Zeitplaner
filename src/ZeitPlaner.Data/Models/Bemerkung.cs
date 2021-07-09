using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ZeitPlaner.Data.Models
{
    public class Bemerkung
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [ForeignKey("Kunde")]
        public int KundeID { get; set; }
        public DateTime? StartZeit { get; set; }
        public DateTime? EndZeit { get; set; }
        public Kunde Kunde { get; set; }
    }
}
