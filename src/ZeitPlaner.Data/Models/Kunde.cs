using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ZeitPlaner.Data.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Kunde
    {
        /// <summary>
        /// Identifikation.
        /// </summary>
        [Key]
        public int ID { get; set; }
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }
        public List<Bemerkung> Bemerkungen { get; set; }
    }
}
