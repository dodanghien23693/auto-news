namespace auto_news.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CrawlConfig
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CrawlConfig()
        {
            CrawlSchedules = new HashSet<CrawlSchedule>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [StringLength(200)]
        public string ImageUrl { get; set; }

        [StringLength(200)]
        public string CategoryName { get; set; }

        [Required]
        [StringLength(200)]
        public string Content { get; set; }

        public int? MethodType { get; set; }

        public string MethodDescription { get; set; }

        public int? NewsSourceId { get; set; }

        public int CategoryId { get; set; }

        [Column(TypeName = "text")]
        public string FormatObjectDescription { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CrawlSchedule> CrawlSchedules { get; set; }
    }
}
