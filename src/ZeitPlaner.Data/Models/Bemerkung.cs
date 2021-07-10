using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZeitPlaner.Data.Models
{
    /// <summary>
    /// Bemerkung.
    /// </summary>
    public class Bemerkung
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the kunde identifier.
        /// </summary>
        [Required]
        [ForeignKey("Kunde")]
        public int KundeID { get; set; }

        /// <summary>
        /// Gets or sets the start zeit.
        /// </summary>
        public DateTime? StartZeit { get; set; }

        /// <summary>
        /// Gets or sets the end zeit.
        /// </summary>
        public DateTime? EndZeit { get; set; }

        /// <summary>
        /// Gets or sets the kunde.
        /// </summary>
        public Kunde Kunde { get; set; }
    }
}