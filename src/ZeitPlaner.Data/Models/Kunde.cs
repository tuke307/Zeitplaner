using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZeitPlaner.Data.Models
{
    /// <summary>
    /// Kunde.
    /// </summary>
    public class Kunde
    {
        /// <summary>
        /// Identifikation.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the bemerkungen.
        /// </summary>
        /// <value>
        /// The bemerkungen.
        /// </value>
        public List<Bemerkung> Bemerkungen { get; set; }
    }
}