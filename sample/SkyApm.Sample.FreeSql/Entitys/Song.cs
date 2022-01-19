using FreeSql.DataAnnotations;

namespace SkyApm.Sample.FreeSqlSqlite.Entitys
{
    public class Song
    {

        [Column(IsIdentity = true)]
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
