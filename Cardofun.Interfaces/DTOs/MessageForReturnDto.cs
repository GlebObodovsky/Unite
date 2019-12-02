using System;

namespace Cardofun.Interfaces.DTOs
{
    public class MessageForReturnDto
    {
        public Guid Id { get; set; }
        public Int32 SenderId { get; set; }
        public Int32 RecipientId { get; set; }
        public String Text { get; set; }
        public String PhotoUrl { get; set; }
        public DateTime SentAt { get; set; }
        public Boolean IsRead => ReadAt.HasValue;
        public DateTime? ReadAt { get; set; }
    }
}