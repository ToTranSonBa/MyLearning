using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.FlashCard
{
    public  class CardMetaData
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public required Card Card { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
}
