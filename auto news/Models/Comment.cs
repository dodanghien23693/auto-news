namespace auto_news.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Comment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserId { get; set; }

        public int ArticleId { get; set; }

        public string Content { get; set; }

        public DateTime CreateDateTime { get; set; }
        public DateTime? ModifyDateTime { get; set; }

        //public virtual Category Category { get; set; }

        //public virtual NewsSource NewsSource { get; set; }
    }
}
