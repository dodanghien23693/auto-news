namespace auto_news.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class NewsRaw
    {
        public DateTime? CreateDateTime { get; set; }

        [Column(TypeName = "text")]
        public string ObjectData { get; set; }

        public int? CrawlConfigId { get; set; }

        public int? ProcessStatus { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
    }
}
