namespace auto_news.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Article
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int NewsSourceId { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(400)]
        public string Description { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Column(TypeName = "text")]
        public string RawContent { get; set; }

        [Column(TypeName = "text")]
        public string FormatedContent { get; set; }

        public int CategoryId { get; set; }

        public int? CrawlConfigId { get; set; }

        [MaxLength(500)]
        public byte[] OriginUrl { get; set; }

        public virtual Category Category { get; set; }

        public virtual NewsSource NewsSource { get; set; }
    }
}
